using UnityEngine;

/// <summary>
/// Sluzi na vykreslovanie 2D obrysoveho obdlznika podla velkosti na pozicii urcenej odchylkov
/// </summary>
public class GizmosBoxDraw : MonoBehaviour
{
    [SerializeField] protected Vector2 size;
    [SerializeField] protected Vector3 offset = Vector3.zero;
    [SerializeField] protected Color color = Color.red;
    [SerializeField] protected bool drawAlwayes = false;

    
    protected virtual void Start()
    {

    }
    protected virtual void OnDrawGizmos()
    {
        if (drawAlwayes)
            OnDrawGizmosSelected();
    }
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        DrawWireCube();
    }
    protected virtual void DrawWireCube()
    {
        Gizmos.DrawWireCube(transform.position, size);
    }
}