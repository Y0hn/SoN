using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Sluzi pre spravne nacitanie udajov mapy a zaroven aj zo serveru vytvara nepriatelov na liniiach mapy
/// </summary>
public class MapScript : MapSizer
{
    public static MapScript map;
    [SerializeField] List<SpawnLine> spawLines;
    [SerializeField] Transform extractions;
    [SerializeField] Vector2 playerSpawnRange = new(5,5);
    [SerializeField] Transform PlayerSpawnPoint;
    [SerializeField] Transform BossSpawnPoint;
    [SerializeField] GameObject[] regularEnemiesTier1;
    [SerializeField] GameObject[] regularEnemiesTier2;

    public Transform PlaySpawn => PlayerSpawnPoint;
    public Transform BossSpawn => BossSpawnPoint;
    public Vector2 PlayerRandomSpawn => 
        new(PlaySpawn.position.x + Random.Range(-playerSpawnRange.x, playerSpawnRange.x), 
            PlaySpawn.position.y + Random.Range(-playerSpawnRange.y, playerSpawnRange.y));

    public static byte npCouter = 0;
    private const byte BASE_MAX_NPC_COUNT = 15;
    private Vector2 FurtherestPlayer
    {
        get 
        {
            Vector2 fP = new(-500,0);
            GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(g => 
            {
                float x = g.transform.position.x; 
                if (fP.x < x) 
                    fP = new (x, fP.y);
            });
            return fP;
        }
    }
    /// <summary>
    /// Najblizsiu neprekrocenu liniu k hracom
    /// </summary>
    private int LastLineCrossed 
    {
        get {
            float playerPos = FurtherestPlayer.x + 10;
            int i = spawLines.FindIndex(spL => playerPos < spL.Position.x);
            return 0 <= i && i < spawLines.Count ? i : spawLines.Count-1;
        }
    }
    private int lastSpawnedLine = 0;

    public bool SpawnEnemies => npCouter < MaxNPCounter;
    public int MaxNPCounter => BASE_MAX_NPC_COUNT + 10 * lastSpawnedLine;

    /// <summary>
    /// Zavola sa pred prvym snimkom obrazovky hry
    /// </summary>
    protected void Awake() => map = this;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Start()
    {
        NPStats.npcDied += delegate { if (0 < npCouter) npCouter--; };        
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
    /// <summary>
    /// Je volana z hraca, pri prvom pripojeni alebo zo servera ked sa hrac ozivy. <br />
    /// Presunie hraca do okruhu zaciatocneho bodu
    /// </summary>
    /// <param name="player">HRAC</param>
    public void SpawnPlayer(Transform player)
    {
        player.position = PlayerRandomSpawn;
        FileManager.Log($"Player respawned {player.name} on position ({player.position.x},{player.position.y})", FileLogType.RECORD);
    }
    /// <summary>
    /// Musi byt server aby spustil tuto funkciu
    /// </summary>
    public void SpawnEnemy()
    {
        //FileManager.Log("Started spawning");

        // ziska suradnice pre vytvorenie nepriatela
        lastSpawnedLine = LastLineCrossed;
        Vector2 pos = spawLines[lastSpawnedLine].SpawnPosition;
        //FileManager.Log("Spawing Got Position");

        // ak padne menej ako 7 pouzije slabsi model typ nepriatela
        bool firstTier = Random.Range(lastSpawnedLine,10) < 7; 
        //FileManager.Log("Spawing Got Tier");
        
        // Nahodne vyberie typ nepriatela v danej (zakladny / silnejsi) variante
        GameObject enemy = firstTier ? 
            regularEnemiesTier1[Random.Range(0,regularEnemiesTier1.Length)] 
                : 
            regularEnemiesTier2[Random.Range(0,regularEnemiesTier2.Length)];

        // Vytvori objekt nepriatela na pozicii
        enemy = Instantiate(enemy, pos, Quaternion.identity);
        NPStats npc = enemy.GetComponent<NPStats>();
        npc.NetObject.Spawn();
        //FileManager.Log("Spawing spawned");

        // Zvoli nahodny ciel
        npc.GetComponent<NPController>().SetDefaultTarget(extractions.GetChild(Random.Range(0,extractions.childCount)));
        npCouter++;

        FileManager.Log($"Enemy spawned {enemy.name} for {FurtherestPlayer} on [{pos.x}{pos.y}] line {lastSpawnedLine} current number of em= {npCouter}");
    }
    /// <summary>
    /// Nacita charakter nepiratela z ulozenych dat
    /// </summary>
    /// <param name="save">ulozene DATA</param>
    public void SpawnFromSave (World.EntitySave save)
    {
        GameObject e = null;
        string[] n = save.etName.Split('-');
        bool boss = save is World.BossSave;
        if (boss)
        {
            e = Resources.Load<GameObject>("Entities/Veles/Veles");
        }
        else
        {
            e = regularEnemiesTier1.ToList().Find(r => r.name == n[0]);
            e ??= regularEnemiesTier2.ToList().Find(r => r.name == n[0]);
        }

        if (e != null)
        {
            NetworkObject netO = Instantiate(e, save.Position, Quaternion.identity).GetComponent<NetworkObject>();

            if (boss)
                netO.GetComponent<BosController>().SetSensor(BossSpawn.GetComponent<BosSensor>());
            
            netO.Spawn();

            if (netO.TryGetComponent(out NPStats npS) && npS is not BosStats)
                npS.Load(save);

            //FileManager.Log($"Entity {save.etName} Save loaded ");
        }
        else
            FileManager.Log($"Entity Save name not found similiar {save.etName}", FileLogType.WARNING);
    }
    /// <summary>
    /// Musi byt server aby spustil tuto funkciu. <br />
    /// Spusta sa len pri vytvarani noveho sveta.
    /// </summary>
    public void SpawnBoss()
    {
        GameObject v = Resources.Load<GameObject>("Entities/Veles/Veles");
        FileManager.Log("BossLoaded");

        BosController bc = Instantiate(v, BossSpawn).GetComponent<BosController>();
        FileManager.Log("BossCreated");

        bc.SetSensor(BossSpawn.GetComponent<BosSensor>());
        FileManager.Log("BossSensoredUp");

        bc.SetDefaultTarget(BossSpawn);
        FileManager.Log("BossDefaulted");

        bc.Stats.NetObject.Spawn();
        FileManager.Log("BossSpawned");
    }
    /// <summary>
    /// Ziska ciel pre nepriatela podla jeho nazvu
    /// </summary>
    /// <param name="_name"></param>
    /// <returns></returns>
    public Transform RequestDefaultTarget(string _name)
    {
        // Ziska cilovy bod podla nazvu
        Transform dTarget = extractions.GetComponentsInChildren<Transform>().ToList().Find(t => t.name == _name);

        return dTarget;
    }
}
 