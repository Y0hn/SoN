using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/RangedWeapon"), Serializable]
public class Ranged : Weapon
{
    /// <summary>
    /// Lokalna pozicia miesta na ktorej sa ma vytvorit strela tak aby to davalo zmysel
    /// </summary>
    public Vector2 projSpawnPosition;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        return 
            base.ToString() + "\n" +
            $"Projectile spawn position [{projSpawnPosition.x},{projSpawnPosition.y}]\n";
    }
    /// <summary>
    /// Ziska strelu
    /// </summary>
    public static GameObject Projectile(Damage.Type type)
    {
        string path= FileManager.PROJECTILES_OBJECTS_PATH;

        switch (type)
        {
            case Damage.Type.BOW_SINLE: path+= "arrow";     break; 
            case Damage.Type.BOW_MULTI: path+= "arrow 3";   break;
            default: return null;
        }

        return Resources.Load<GameObject>(path);
    }
}