using UnityEngine;
/// <summary>
/// sluzi na tesotvanie dosahov zbrani
/// </summary>
public class WeaponTester : MonoBehaviour
{
    [SerializeField] Weapon weapon;
    [SerializeField] Transform atPiont;
    [SerializeField] int AttackIndex = 0;
    /// <summary>
    /// Nastava ak je v editore zapnute Gizmos a je urcena zbran
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (weapon != null && AttackIndex < weapon.attack.Count)
        {
            atPiont.localPosition = new(atPiont.localPosition.x, weapon.attack[AttackIndex].range);
            Gizmos.DrawWireSphere(atPiont.position, weapon.attack[AttackIndex].range);
        }
    }
}
