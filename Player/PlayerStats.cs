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
    }
    protected override void Update()
    {
        base.Update();
    }
}
