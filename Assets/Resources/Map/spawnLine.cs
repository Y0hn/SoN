using UnityEngine;
public class SpawnLine : MonoBehaviour 
{
    [SerializeField] float up;
    [SerializeField] float down;

    public Vector2 Position => new (transform.position.x, Random.Range(down, up));

    void OnDrawGizmosSelected()
    {
        Vector2 size = new (0.5f , up + down);

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new (transform.position.x, transform.position.y-down/2+up/2), size);
    }
}