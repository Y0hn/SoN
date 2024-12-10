using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/RangedWeapon"), Serializable]
public class Ranged : Weapon
{
    [SerializeField] string projectile;
    public float projSpawnDistance = 0.5f;
    public GameObject GetProjectile
    {
        get => Resources.Load<GameObject>(FileManager.PROJECTILES_OBJECTS_PATH + projectile);
    }
}