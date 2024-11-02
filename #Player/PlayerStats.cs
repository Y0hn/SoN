using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    [SerializeField] GameObject chatField;
    [SerializeField] TMP_Text chatBox;
    float chatTimer; const float chatTime = 5.0f;
    
    Slider xpBar;
    float atTime = 0;
    int xpMax = 10, xpMin = 0;
    protected Inventory inventory;
    protected NetworkVariable<int> xp = new(0);
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
            xpBar.minValue = xpMin;
            xpBar.maxValue = xpMax;
            xpBar.value = xp.Value;

            xp.OnValueChanged += (int prevValue, int newValue) => OnXpUpdate();

            playerName.Value = GameManager.instance.PlayerName;

            hp.OnValueChanged += (int prevValue, int newValue) => GameManager.instance.AnimateFace(HP);
        }
        chatTimer = 0;
        chatBox.text = "";
        chatField.SetActive(false);
        nameTag.text = playerName.Value.ToString();
        GetComponent<NetworkObject>().name = nameTag.text;
    }
    protected override void Update()
    {
        if (chatTimer != 0)
            if (chatTimer <= Time.time)
            {
                chatField.SetActive(false);
                chatBox.text = "";
                chatTimer = 0;
            }
    }
    protected void OnXpUpdate()
    {
        xpBar.value = xp.Value;
    }
    protected void OnLevelUp()
    {
        xpMax += xp.Value * level.Value;
        xpBar.minValue = xp.Value;
        xpBar.maxValue = xpMax;
    }
    public override bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            if (attack.Value.type == Attack.Type.MeleeSlash)
                MeleeAttack();
            //else Debug.Log($"Player {name} attack type {Enum.GetName(typeof(Attack.Type), attack.Value.type)} not defined");

            atTime = Time.time + 1/attack.Value.rate;
            return true;
        }
        return false;
    }
    protected override EntityStats[] MeleeAttack()
    {
        foreach (EntityStats stats in base.MeleeAttack())
        {
            if (stats is PlayerStats)
            {
                ulong hitID = stats.GetComponent<NetworkObject>().OwnerClientId;
                DamagePlayerServerRpc(attack.Value.damage, OwnerClientId, hitID);
                //Debug.Log($"'{name}' (ID: {OwnerClientId}) attacking player '{stats.name}' with ID: {hitID}");
            }
        }
        return null;
    }
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        xp.Value += died.level.Value;
    }
    protected override void SetLive(bool alive)
    {
        base.SetLive(alive);
        
        SetLiveClientRpc(alive);
    }
    public override bool TakeDamage(Damage damage)
    {
        TakenDamageClientRpc();
        return base.TakeDamage(damage);
    }
    /// <summary>
    /// Server does this for Player doing damage to another Player
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="dealerId"></param>
    /// <param name="targetId"></param>
    [ServerRpc] protected void DamagePlayerServerRpc(Damage damage, ulong dealerId, ulong targetId)
    {
        PlayerStats playerTarget = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject.GetComponent<PlayerStats>();
        PlayerStats playerDealer = NetworkManager.Singleton.ConnectedClients[dealerId].PlayerObject.GetComponent<PlayerStats>();

        if (playerTarget != null)
        {
            if (playerTarget.IsAlive.Value)
                if (playerTarget.TakeDamage(damage))
                {
                    playerTarget.IsAlive.Value = false;
                    playerDealer.KilledEnemy(playerTarget);
                }
                ///else Debug.Log($"Player {targetId} lives");                
            //else Debug.Log($"Player {targetId} already dead");
        }
        else
        {
            Debug.Log($"Player {targetId} not found");
        }
    }
    [ClientRpc] public void AddItemClientRpc(Item item)
    {
        if (IsOwner)
            Inventory.instance.AddItem(item);
    }

    [ClientRpc] protected void SetLiveClientRpc(bool alive)
    {
        if (IsOwner)
            GameManager.instance.SetPlayerUI(alive);
    }
    [ClientRpc] protected void TakenDamageClientRpc()
    {
        if (IsOwner)
            GameManager.instance.AnimateFace("got-hit");
    }
    [ClientRpc] public void ItemPickUpClientRpc(Item item)
    {
        if (IsOwner)
            inventory.AddItem(item);
    }
    [ServerRpc] public void SendMessageServerRpc(string message)
    {
        SendMessageClientRpc(message);
    }
    [ClientRpc] protected void SendMessageClientRpc(string message)
    {
        SpeakText(message);
    }
    void SpeakText(string text)
    {
        chatBox.text = text;
        chatField.SetActive(true);
        chatTimer = Time.time + chatTime;
    }
}
