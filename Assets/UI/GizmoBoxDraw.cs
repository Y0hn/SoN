using UnityEngine;

/// <summary>
/// Sluzi na vykreslovanie 2D obrysoveho obdlznika podla velkosti na pozicii urcenej odchylkov
/// </summary>
public class GizmosBoxDraw : MonoBehaviour
{
    [SerializeField] protected Vector2 size;
    [SerializeField] protected Vector3 offset = Vector3.zero;
    [SerializeField] protected Color color = Color.red;
    [SerializeField] protected bool drawAlways = false;

    /// <summary>
    /// Nastava po povoleni objektu
    /// </summary>
    protected virtual void Start()
    {

    }
    /// <summary>
    /// Nakresli obdlznik <br />
    /// Ak je povoleny atribut "drawAlways"
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        if (drawAlways)
            OnDrawGizmosSelected();
    }
    /// <summary>
    /// Nakresli obdlznik <br />
    /// Ak je objekt oznaceny "drawAlways"
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        DrawWireCube();
    }
    /// <summary>
    /// Nakresli kablovy obdlznik (napriek nazvu)
    /// </summary>
    protected virtual void DrawWireCube()
    {
        Gizmos.DrawWireCube(transform.position, size);
    }
}