using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
/// <summary>
/// Sluzi pre spravne nacitanie udajov mapy a zaroven aj zo serveru vytvara nepriatelov na liniiach mapy
/// </summary>
public class MapScript : MapSizer
{
    public static MapScript map;
    [SerializeField] Transform spawLines;
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
        // ziska hraca, ktory zasiel najdalej
        Vector2 furtherestPlayer = Vector2.zero;
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(g => 
            {
                float x = g.transform.position.x; 
                if (furtherestPlayer.x < x) 
                    furtherestPlayer = new (x, furtherestPlayer.y);
            });

        // ziska liniu pre vytvorenie nepriatela
        bool firstTier = true;
        Vector3 pos = Vector2.zero;
        SpawnLine[] spawns = spawLines.GetComponentsInChildren<SpawnLine>();
        for (int i = 0; i < spawns.Length; i++)
        {
            // po vyske linie vyberie nahodnu hodnotu
            Vector2 spawnL = spawns[i].Position;
            
            // Mal by si vybrat najblizsiu neprekrocenu liniu k hracom
            if (furtherestPlayer.x < spawnL.x)
            {
                pos = spawnL;

                // ak je za prvou liniou ma sancu 3/10 na silnejsieho nepriatela
                if (1 < i)
                    firstTier = Random.Range(i,10) < 3; // ak padne [0,1,2] zostava zakladny typ nepriatela
                break;
            }
        }
        
        // Nahodne vyberie typ nepriatela (zakladny / silnejsi) variant
        GameObject enemy = firstTier ? 
            regularEnemiesTier1[Random.Range(0,regularEnemiesTier1.Length)] 
                : 
            regularEnemiesTier2[Random.Range(0,regularEnemiesTier2.Length)];

        // Vytvori objekt nepriatela na pozicii
        enemy = Instantiate(enemy, pos, Quaternion.identity);
        NPStats npc = enemy.GetComponent<NPStats>();
        npc.NetObject.Spawn();

        // Zvoli nahodny ciel
        npc.GetComponent<NPController>().SetDefaultTarget(extractions.GetChild(Random.Range(0,extractions.childCount)));
        npCouter++;

        //FileManager.Log($"Enemy spawned {enemy.name} current number of em= {npCouter}");
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
 