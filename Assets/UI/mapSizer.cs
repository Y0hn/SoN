using UnityEngine;

public class MapSizer : GizmosBoxDraw
{
    [SerializeField] private GameObject mapPrefab;
    protected override void Start()
    {
        if (transform.childCount < 1)
            Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 