using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class PlayerStats : EntityStats
{
    /*  Inhereted Variables
     * 
     * [SF] protected TMP_Text nameTag;
     * protected NetworkList<Rezistance> rezists = new();
     * [SF] protected NetworkVariable<int> maxHp = new();
     * protected NetworkVariable<int> hp = new();
     * [SF] protected Slider hpBar;
     * [SF] public float speed;
     * protected const float timeToDespawn = 0f;
     *
     */
    NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        playerName.OnValueChanged += (FixedString32Bytes prevValue, FixedString32Bytes newValue) => { nameTag.text = newValue.ToString(); };

        if (IsOwner)
        {
            hpBar.gameObject.SetActive(false);
            hpBar = GameManager.instance.GetPlayerHpBar();
            playerName.Value = GameManager.instance.GetPlayerName();
        }
        nameTag.text = playerName.Value.ToString();
        name = playerName.Value.ToString();
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);

        Debug.Log("Taken damage");
    }
    [ClientRpc]
    public void TakenDamageClientRpc(Damage damage, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;
        Debug.Log($"Client got damaged by {damage.amount} as {damage.type}");
    }
}
