using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/RangedWeapon"), Serializable]
public class Ranged : Weapon
{
    /// <summary>
    /// Cesta ku strele
    /// </summary>
    [SerializeField] string projectile;
    /// <summary>
    /// Lokalna pozicia miesta na ktorej sa ma vytvorit strela tak aby to davalo zmysel
    /// </summary>
    public Vector2 projSpawnPosition;
    /// <summary>
    /// Vrati strelu urcenu pre tuto zbran podla cesty
    /// </summary>
    public GameObject GetProjectile => Resources.Load<GameObject>(FileManager.PROJECTILES_OBJECTS_PATH + projectile);
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        return 
            base.ToString() + "\n" +
            $"Projectile Referency \"{projectile}\"\n" +
            $"Projectile spawn position [{projSpawnPosition.x},{projSpawnPosition.y}]\n";
    }
}