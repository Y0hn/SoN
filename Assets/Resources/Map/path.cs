using UnityEngine;

public class StonePath : MonoBehaviour
{
    [SerializeField] float speedModifier = 1.2f;
    /// <summary>
    /// Aktivne len na servery
    /// </summary>
    public bool Active => GameManager.instance.IsServer;

    /// <summary>
    /// Pri vstupe na cestu
    /// </summary>
    /// <param name="collider"></param>
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (Active && collider.TryGetComponent(out EntityStats es))
        {
            es.TerrainChangeRpc(speedModifier);
        }
    }
    /// <summary>
    /// Po opusteni cesty
    /// </summary>
    /// <param name="collider"></param>
    void OnTriggerExit2D(Collider2D collider)
    {
        if (Active && collider.TryGetComponent(out EntityStats es) && es.IsSpawned)
            es.TerrainChangeRpc(1f/speedModifier);
    }
}
