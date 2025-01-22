using UnityEngine;

public class MapScript : MapSizer
{
    [SerializeField] Transform spawnpoint;
    protected override void Start()
    {
        Connector.instance.spawnPoint = spawnpoint;
    }
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 