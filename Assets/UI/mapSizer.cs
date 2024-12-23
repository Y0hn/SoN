using UnityEngine;

public class mapSizer : MonoBehaviour
{
    [SerializeField] Vector2 size;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] Color color = Color.red;
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 