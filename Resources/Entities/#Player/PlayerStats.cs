using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : EntityStats
{
    /*  Inhereted Variables
     * 
     * [SF] protected TMP_Text nameTag;
     * protected NetworkList<Rezistance> rezists = new();
     * [SF] protected NetworkVariable<int> maxHp = new();
     * protected NetworkVariable<int> hp = new();
     * [SF] protected Slider hpBar;
     * [SF]    public NetworkVariable<int> speed = new();
     * [SF]    protected Transform attackPoint;
     * public NetworkVariable<bool> IsAlive = new();
     * [SF]    protected NetworkVariable<Attack> attack = new ();
     * protected const float timeToDespawn = 0f;
     *
     */

    Slider xpBar;
    int xpMax = 10, xpMin = 0, xp = 0;

    float atTime = 0;
    NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        playerName.OnValueChanged += (FixedString32Bytes prevValue, FixedString32Bytes newValue) => { nameTag.text = newValue.ToString(); };

        if (IsOwner)
        {
            hpBar.gameObject.SetActive(false);
            hpBar = GameManager.instance.GetBar("hp");
            xpBar = GameManager.instance.GetBar("xp");
            xpBar.maxValue = xpMax;
            xpBar.minValue = xpMin;
            xpBar.value = xp;

            playerName.Value = GameManager.instance.GetPlayerName();
        }
        nameTag.text = playerName.Value.ToString();
        name = nameTag.text;
    }
    public override bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            if (attack.Value.type == Attack.Type.Melee)
                base.MeleeAttack();

            atTime = Time.time + 1/attack.Value.rate;
            return true;
        }
        return false;
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        Debug.Log($"Player taking damage");
    }
    protected override void Die()
    {
        GameManager.instance.PlayerDied(transform.position);
        base.Die();
    }
}
