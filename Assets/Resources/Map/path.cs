using UnityEngine;

public class StonePath : MonoBehaviour
{
    [SerializeField] float speedModifier = 1.2f;
    [SerializeField] bool playerOnly = true;
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
        try {
            if (Active && collider.TryGetComponent(out EntityStats es)&& (!playerOnly || es is not NPStats))
                es.TerrainChangeRpc(speedModifier, true);
        } catch {

        }
    }
    /// <summary>
    /// Po opusteni cesty
    /// </summary>
    /// <param name="collider"></param>
    void OnTriggerExit2D(Collider2D collider)
    {
        try {
            if (Active && collider.TryGetComponent(out EntityStats es) && es.IsSpawned && (!playerOnly || es is not NPStats))
                es.TerrainChangeRpc(1f/speedModifier, true);
        } catch {

        }
    }
}
