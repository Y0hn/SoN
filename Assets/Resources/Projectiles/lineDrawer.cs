using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.parent.position, transform.position);
    }
}
