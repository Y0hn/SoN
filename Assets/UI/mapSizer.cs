using UnityEngine;

public class mapSizer : MonoBehaviour
{
    [SerializeField] Vector2 size;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] Color color = Color.red;
    [SerializeField] GameObject mapPrefab;
    
#pragma warning disable IDE0051 // Remove unused private members
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
    void Start()
    {
        Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }
#pragma warning restore IDE0051 // Remove unused private members
}
 