using UnityEngine;

public class StonePath : MonoBehaviour
{
    [SerializeField] float speedModifier = 1.2f;
    public bool Active => GameManager.instance.IsServer;
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (Active && collider.TryGetComponent(out EntityStats es))
        {
            es.TerrainChangeRpc(speedModifier);
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (Active && collider.TryGetComponent(out EntityStats es))
        {
            es.TerrainChangeRpc(1f/speedModifier);
        }
    }
}
