using UnityEngine;

public class WeaponTester : MonoBehaviour
{
    [SerializeField] Weapon weapon;
    [SerializeField] Transform atPiont;
    [SerializeField] int AttackIndex = 0;
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
