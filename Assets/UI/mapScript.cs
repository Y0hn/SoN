using Unity.Netcode;
using UnityEngine;
using Pathfinding;

public class MapScript : MapSizer
{
    [SerializeField] Transform spawLines;
    [SerializeField] Transform extractions;
    [SerializeField] Transform spawnpoint;
    [SerializeField] GameObject[] regularEnemiesTier1;
    [SerializeField] GameObject[] regularEnemiesTier2;
    public static MapScript map;
    protected void Awake()
    {
        if (map == null)
            map = this;
    }
    protected override void Start()
    {
        Connector.instance.spawnPoint = spawnpoint;
    }
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
    public void SpawnEnemy(bool firstTier = true)
    {
        // Musi byt server
        GameObject enemy = firstTier ? 
            regularEnemiesTier1[Random.Range(0,regularEnemiesTier1.Length)] 
                : 
            regularEnemiesTier2[Random.Range(0,regularEnemiesTier2.Length)];

        // Mal by si vybrat najblizsiu neprekrocenu liniu k hracom
        Vector2 spawnL = spawLines.GetChild(Random.Range(0,spawLines.childCount)).position;
        Vector3 pos = new (spawnL.x, Random.Range(-size.y/2, size.y/2), 0);
        enemy = Instantiate(enemy, pos, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn();

        // Zvoli nahodny ale mal by si vybrat najblizsi
        enemy.GetComponent<AIDestinationSetter>().target = extractions.GetChild(Random.Range(0,extractions.childCount));
        Debug.Log("enemy spawned");
    }
}
 