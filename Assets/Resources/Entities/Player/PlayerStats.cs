using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
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
    public RpcParams OwnerRPC { get { return RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp); } }
    float chatTimer; const float chatTime = 5.0f;
    Slider xpBar;       // UI nastavene len pre Ownera
    float atTime = 0;   // pouziva len owner
    int xpMax = 10, xpMin = 0;
    protected NetworkVariable<int> xp = new(0);
    NetworkList<FixedString64Bytes> inventory = new(null, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected GameManager game;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        playerName.OnValueChanged += (FixedString32Bytes prevValue, FixedString32Bytes newValue) => { nameTag.text = newValue.ToString(); };

        if (IsOwner)
        {
            game = GameManager.instance;
            hpBar.gameObject.SetActive(false);
            hpBar = game.GetBar("hp");
            xpBar = game.GetBar("xp");
            xpBar.minValue = xpMin;
            xpBar.maxValue = xpMax;
            xpBar.value = xp.Value;

            xp.OnValueChanged += (int prevValue, int newValue) => OnXpUpdate();

            playerName.Value = GameManager.instance.PlayerName;

            hp.OnValueChanged += (int prevValue, int newValue) => GameManager.instance.AnimateFace(HP);

            inventory.OnListChanged += (NetworkListEvent<FixedString64Bytes> changeEvent) => { OnInventoryUpdate(ref changeEvent); };
        }
        chatTimer = 0;
        chatBox.text = "";
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
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
    protected void OnXpUpdate()     // iba lokalne
    {
        xpBar.value = xp.Value;
    }
    protected void OnLevelUp()      // iba lokalne
    {
        xpMax += xp.Value * level.Value;
        xpBar.minValue = xp.Value;
        xpBar.maxValue = xpMax;
    }
    protected void OnInventoryUpdate(ref NetworkListEvent<FixedString64Bytes> changeEvent)  // iba lokalne
    {        
        switch (changeEvent.Type)
        {
            case NetworkListEvent<FixedString64Bytes>.EventType.Add:
                //int index = changeEvent.Index; // the position of the added value in the list
                game.inventory.Add(changeEvent.Value.ToString()); // the new value at the index position
                break;
            case NetworkListEvent<FixedString64Bytes>.EventType.Remove:
            case NetworkListEvent<FixedString64Bytes>.EventType.RemoveAt:
                game.inventory.Remove(changeEvent.Value.ToString());
                break;
            case NetworkListEvent<FixedString64Bytes>.EventType.Full:
            case NetworkListEvent<FixedString64Bytes>.EventType.Clear:
            case NetworkListEvent<FixedString64Bytes>.EventType.Value:
            case NetworkListEvent<FixedString64Bytes>.EventType.Insert:
            default:
                break;
        }
    }
    public override bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            if (attack.Value.type == Attack.Type.MeleeSlash || Attack.Type.RaseUnnarmed == attack.Value.type)
            {
                foreach(EntityStats et in MeleeAttack())
                {
                    if (et is PlayerStats)
                    {
                        ulong hitID = et.GetComponent<NetworkObject>().OwnerClientId;
                        DamagePlayerRpc(attack.Value.damage, OwnerClientId, hitID);

                        //Debug.Log($"'{name}' (ID: {OwnerClientId}) attacking player '{stats.name}' with ID: {hitID}");
                    }
                }
            }
            else 
                Debug.Log($"Player {name} attack type {System.Enum.GetName(typeof(Attack.Type), attack.Value.type)} not defined");

            atTime = Time.time + 1/attack.Value.rate;
            return true;
        }
        return false;
    }
    protected override EntityStats[] MeleeAttack()
    {
        return base.MeleeAttack();
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
        if (!IsServer) { Debug.Log("Called from noooo server"); return false; }

        OwnerTakenDamageRpc();
        return base.TakeDamage(damage);
    }
    /// <summary>
    /// Server does this for Player doing damage to another Player
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="dealerId"></param>
    /// <param name="targetId"></param>
    [Rpc(SendTo.Server)] protected void DamagePlayerRpc(Damage damage, ulong dealerId, ulong targetId)
    {
        PlayerStats playerTarget = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject.GetComponent<PlayerStats>();
        PlayerStats playerDealer = NetworkManager.Singleton.ConnectedClients[dealerId].PlayerObject.GetComponent<PlayerStats>();

        if (playerTarget != null)
        {
            if (playerTarget.IsAlive.Value)
                if (playerTarget.TakeDamage(damage))    // pravdive ak target zomrie
                {
                    playerDealer.KilledEnemy(playerTarget);
                }
            Debug.Log("Player hitted");
        }
        else
        {
            Debug.LogWarning($"Player {targetId} not found");
        }
    }
    [Rpc(SendTo.Owner)] protected void OwnerTakenDamageRpc()
    {
        GameManager.instance.AnimateFace("got-hit");
    }
    public void PickUpItem(string refItem)
    {
        if (IsServer)
            inventory.Add(refItem);
    }
    [ServerRpc] public void ChangeEquipmentServerRpc(string refEquip, bool equip)
    {
        Item stuff = Item.GetItem(refEquip);
        bool error = false;

        if      (stuff is Weapon w)
        {
            if (equip)
            {
                // attack.Value = w.attack;
                attack.Value = new (w.attack.damage, w.attack.range, w.attack.rate, w.attack.type);
                weapRef.Value = w.spriteRef;
            }
            else
            {
                attack.Value = rase.attack;
                weapRef.Value = "";
            }
        }
        else if (stuff is Armor a)
        {
            if (equip)
                defence.Add(a);
            else
                defence.Remove(a);
        }

        if (error)
        {
            PickUpItem(refEquip);
        }
        //Debug.Log("Player stats changed becouse of equipment change");
    }
    [ClientRpc] protected void SetLiveClientRpc(bool alive)
    {
        if (IsOwner)
            GameManager.instance.SetPlayerUI(alive);
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
