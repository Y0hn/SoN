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
        transform.name = nameTag.text;
    }
    public override bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            if (attack.Value.type == Attack.Type.Melee)
                MeleeAttack();
            else
                Debug.Log($"Player {name} attack type not defined");

            atTime = Time.time + 1/attack.Value.rate;
            return true;
        }
        return false;
    }
    protected override NetworkObject[] MeleeAttack()
    {
        foreach (NetworkObject netO in base.MeleeAttack())
        {
            ulong hitID = netO.OwnerClientId;
            if (OwnerClientId != hitID)
            {
                TakeDamageServerRpc(attack.Value.damage, OwnerClientId, hitID);
                Debug.Log($"'{name}' (ID: {OwnerClientId}) attacking player '{netO.name}' with ID: {hitID}");
            }
        }
        return null;
    }
    [ServerRpc] protected void TakeDamageServerRpc(Damage damage, ulong dealerId, ulong targetId)
    {
        var playerDamaged = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject.GetComponent<PlayerStats>();
        if (playerDamaged != null)
        {
            if (playerDamaged.IsAlive.Value)
                if (playerDamaged.TakeDamage(damage))
                {
                    playerDamaged.Die();
                    NetworkManager.Singleton.ConnectedClients[dealerId].PlayerObject.GetComponent<PlayerStats>().KilledEnemy(playerDamaged);                    
                }
                else
                    Debug.Log("Player {targetId} lives");                
            else
                Debug.Log($"Player {targetId} already dead");
        }
        else
        {
            Debug.Log($"Player {targetId} not found");
        }
    }
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        xp += died.level.Value;
    }
    protected override void Die()
    {
        base.Die();
    }
}
