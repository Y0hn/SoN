using System;
using UnityEngine;
/// <summary>
/// Meni zbran a jej utok (Weaponindex) po dodiahnuti prahu zivotov
/// </summary>
[Serializable] public class WeaponChange
{
    [field:SerializeField] float ownerHP;
    [field:SerializeField] WeaponIndex wpI;
    public bool ReachedHP(float hp)
    {
        return hp < ownerHP;
    }
    public WeaponIndex weaponIndex => wpI;
} 