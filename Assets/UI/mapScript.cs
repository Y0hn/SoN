using Unity.Netcode;
using UnityEngine;
using System.Linq;
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
        NPStats.npcDied += delegate { npCouter--; };        
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
        for (int i = 0; i < spawLines.childCount; i++)
        {
            // Mal by si vybrat najblizsiu neprekrocenu liniu k hracom
            Vector2 spawnL = spawLines.GetChild(i).position;
            if (furtherestPlayer.x < spawnL.x)
            {
                // po celej vyske linie vyberie nahodnu hodnotu
                pos = new (spawnL.x, Random.Range(-size.y/2, size.y/2), 0);

                // ak je za strednou liniou ma sancu 3/10
                if (spawLines.childCount/2 < i)
                    firstTier = Random.Range(0,10) < 3; // ak padne [0,1,2] zostava zakladny typ nepriatela
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

        FileManager.Log($"Enemy spawned {enemy.name} current number of em= {npCouter}");
    }
    /// <summary>
    /// Nacita charakter nepiratela z ulozenych dat
    /// </summary>
    /// <param name="save">ulozene DATA</param>
    public void SpawnFromSave (World.EntitySave save)
    {
        GameObject e = null;
        string[] n = save.etName.Split('-');
        if (save is World.BossSave boss)
        {
            e = Resources.Load<GameObject>("Entities/Veles/Veles");
        }
        else
        {
            e = regularEnemiesTier1.First(r => r.name == n[0]);
            e ??= regularEnemiesTier2.First(r => r.name == n[0]);
        }

        if (e != null)
        {
            NetworkObject netO = Instantiate(e, save.Position, Quaternion.identity).GetComponent<NetworkObject>();
            netO.Spawn();
            netO.GetComponent<NPStats>().Load(save);

            FileManager.Log($"Entity {save.etName} Save loaded ");
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
        Instantiate(v, BossSpawn).GetComponent<NetworkObject>().Spawn();
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
 