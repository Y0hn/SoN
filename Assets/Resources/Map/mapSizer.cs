using UnityEngine;

/// <summary>
/// Vykresluje velkost hracej mapy a taktiez ju vytvara pri povoleni
/// </summary>
public class MapSizer : GizmosBoxDraw
{
    [SerializeField] private GameObject mapPrefab;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Start()
    {
        for (;0 < transform.childCount;)
            Destroy(transform.GetChild(0).gameObject);

        // Zatial
        SpawnMap();
    }
    /// <summary>
    /// Vytvori mapu na suradniciach tohoto objektu ako jeho "dieta"
    /// </summary>
    public void SpawnMap()
    {
        if (0 < transform.childCount)
            Start();
        Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void DrawWireCube()
    {        
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
 