using UnityEngine;

public class StonePath : MonoBehaviour
{
    [SerializeField] float speedModifier = 1.2f;
    [SerializeField] bool playerOnly = true;
    [SerializeField] bool isCoruption = false;
    /// <summary>
    /// Aktivne len na servery
    /// </summary>
    public bool Active => GameManager.instance.IsServer;
    public bool Coruption => isCoruption; 

    /// <summary>
    /// Pri vstupe na cestu
    /// </summary>
    /// <param name="collider"></param>
    void OnTriggerEnter2D(Collider2D collider)
    {
        try {
            if (Active && collider.TryGetComponent(out EntityStats es)&& (!playerOnly || es is not NPStats))
            {
                if (!isCoruption || !(es is PlayerStats pl && pl.ImunityToCoruption))
                {
                    es.TerrainChangeRpc(speedModifier, true);
                    ((PlayerStats)es).Corruped |= Coruption;
                }
            }
        } catch {

        }
    }
    /// <summary>
    /// Po opusteni cesty
    /// </summary>
    /// <param name="collider"></param>
    public void OnTriggerExit2D(Collider2D collider)
    {
        try {
            if (Active && collider.TryGetComponent(out EntityStats es) && es.IsSpawned && (!playerOnly || es is not NPStats))
                if (!isCoruption || !(es is PlayerStats pl && pl.ImunityToCoruption) || pl.Corruped)
                    es.TerrainChangeRpc(1f/speedModifier, true);
        } catch {

        }
    }
}
