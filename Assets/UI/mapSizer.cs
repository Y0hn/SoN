using UnityEngine;

public class MapSizer : GizmosBoxDraw
{
    [SerializeField] protected GameObject mapPrefab;
    protected override void Start()
    {
        Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 