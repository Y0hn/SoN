using System;
using UnityEngine;
/// <summary>
/// Meni zbran a jej utok (Weaponindex) po dodiahnuti prahu zivotov
/// </summary>
[Serializable] public class WeaponChange
{
    [field:SerializeField] float ownerHP;
    [field:SerializeField] WeaponIndex wpI;
    /// <summary>
    /// Kontrola dosiahnutia potreblneho poctu zivotov
    /// </summary>
    /// <param name="hp">aktualny pocet ZIVOTOV</param>
    /// <returns>PRAVDA ak je splnena podmienka</returns>
    public bool ReachedHP(float hp)
    {
        return hp < ownerHP;
    }
    /// <summary>
    /// Ziska index zbrane
    /// </summary>
    public WeaponIndex weaponIndex => wpI;
    public float HP => ownerHP;
} 