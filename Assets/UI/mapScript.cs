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

    /// <summary>
    /// Zavola sa pred prvym snimkom obrazovky hry
    /// </summary>
    protected void Awake() => map = this;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Start()
    {
        if (FileManager.World.boss == null)
        {
            SpawnBoss();
        }
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
        enemy.GetComponent<NetworkObject>().Spawn();

        // Zvoli nahodny ale mal by si vybrat najblizsi
        Transform target = extractions.GetChild(Random.Range(0,extractions.childCount));
        enemy.GetComponent<NPController>().SetDefaultTarget(target);
    }
    /// <summary>
    /// Musi byt server aby spustil tuto funkciu. <br />
    /// Spusta sa len pri vytvarani noveho sveta.
    /// </summary>
    void SpawnBoss()
    {
        GameObject v = Resources.Load<GameObject>("Entities/Veles/Veles");
        Instantiate(v, BossSpawn).GetComponent<NetworkObject>().Spawn();
    }
}
 