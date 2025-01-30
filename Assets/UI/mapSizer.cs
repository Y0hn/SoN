using UnityEngine;

public class MapSizer : GizmosBoxDraw
{
    [SerializeField] private GameObject mapPrefab;
    protected override void Start()
    {
        for (;0 < transform.childCount;)
            Destroy(transform.GetChild(0).gameObject);

        // Zatial
        SpawnMap();
    }
    public void SpawnMap()
    {
        if (0 < transform.childCount)
            Start();
        Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 