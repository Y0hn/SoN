using UnityEngine;
public class SpawnLine : MonoBehaviour 
{
    [SerializeField] float up   = 0;
    [SerializeField] float down = 0;
    [SerializeField] float left = 0;
    [SerializeField] float right= 0;

    public Vector2 Position => transform.position;

    public Vector2 SpawnPosition => 
        new (   Random.Range(transform.position.x-left, transform.position.x+right+1), 
                Random.Range(transform.position.y-down, transform.position.y+up   +1));

    void OnDrawGizmosSelected()
    {
        Vector2 size = new (0.5f , up + down);

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new (transform.position.x, transform.position.y-down/2+up/2), size);
    }
}