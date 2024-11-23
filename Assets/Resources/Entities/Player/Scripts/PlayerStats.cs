using Unity.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class PlayerStats : EntityStats
{
    /*  ZDEDENE ATRIBUTY
     *      
     *  [SF] protected TMP_Text nameTag;
     *  [SF] protected Rase rase;
     *  [SF] protected Slider hpBar;
     *  [SF] protected GameObject body;
     *  [SF] protected NetworkObject netObject;
     *  [SF] protected Transform attackPoint;
     *  [SF] protected SpriteRenderer weaponR, weaponL;
     *  [SF] protected NetworkAnimator animator;
     *  [SF] protected Rigidbody2D rb;
     *
     *  [SF]    protected NetworkVariable<int> maxHp = new();
     *
     *  protected NetworkVariable<int> hp = new();
     *  protected NetworkVariable<Attack> attack = new ();
     *  protected NetworkVariable<FixedString128Bytes> weapRef = new();
     *  public NetworkVariable<bool> IsAlive = new(true);
     *  public NetworkVariable<float> speed = new();
     *  public NetworkVariable<byte> level = new(1);
     *
     *  public float HP { get { return (float)hp.Value/(float)maxHp.Value; } }
     *  public NetworkObject NetObject { get { return netObject; } }
     *  public Animator Animator { get { return animator.Animator; } }
     *  public Rigidbody2D RigidBody2D { get { return rb; } }
     *
     *  protected const float timeToDespawn = 0f;
     *  private bool clampedDMG = true;
     *  protected Defence defence;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] GameObject chatField;
    [SerializeField] TMP_Text chatBox;
    //public RpcParams OwnerRPC { get { return RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp); } }
    float chatTimer; const float chatTime = 5.0f;
    Slider xpBar;       // UI nastavene len pre Ownera
    float atTime = 0;   // pouziva len owner
    int xpMin = 0;
    protected NetworkVariable<int> xp = new(0);
    protected NetworkVariable<int> xpMax = new(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkList<FixedString64Bytes> inventory = new(null, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString128Bytes> message = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected GameManager game;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OwnerSubsOnNetValChanged();
    }
    protected override void EntitySetUp()
    {
        base.EntitySetUp();

        if (IsOwner)
        {
            game = GameManager.instance;
            hpBar.gameObject.SetActive(false);
            hpBar = game.GetBar("hp");
            xpBar = game.GetBar("xp");
            xpBar.minValue = xpMin;
            xpBar.maxValue = xpMax.Value;
            xpBar.value = xp.Value;

            playerName.Value = game.PlayerName;
        }
        chatTimer = 0;
        chatBox.text = "";
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
        GetComponent<NetworkObject>().name = nameTag.text;
    }
    protected override void SubsOnNetValChanged()
    {
        base.SubsOnNetValChanged();
        if (IsServer)
            xpMax.OnValueChanged += (int prev, int newValue) => level.Value++;
        playerName.OnValueChanged += (FixedString32Bytes prevValue, FixedString32Bytes newValue) => { nameTag.text = newValue.ToString(); };
        message.OnValueChanged += (FixedString128Bytes prevValue, FixedString128Bytes newMess) => 
        {
            chatBox.text = newMess.ToString();
            chatField.SetActive(true);
            chatTimer = Time.time + chatTime;
        };
    }
    protected void OwnerSubsOnNetValChanged()
    {
        if (!IsOwner) return;
        xp.OnValueChanged += (int prevValue, int newValue) => 
        { 
            if (newValue < xpMax.Value)
                xpBar.value = xp.Value; 
            else
            {
                xpMax.Value += xp.Value * level.Value;
                xpBar.minValue = xp.Value;
                xpBar.maxValue = xpMax.Value;
            }
        };
        hp.OnValueChanged += (int prevValue, int newValue) => 
        { 
            if (newValue < prevValue)
                game.AnimateFace("got-hit");
            game.AnimateFace(HP);
        };
        inventory.OnListChanged += (NetworkListEvent<FixedString64Bytes> changeEvent)   => OnInventoryUpdate(changeEvent);
        IsAlive.OnValueChanged  += (bool prevValue, bool newValue)                      => game.SetPlayerUI(newValue);
    }
    protected void OnInventoryUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)  // iba lokalne
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
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        xp.Value += died.level.Value * 5 ;
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
    public void PickUpItem(string refItem)
    {
        inventory.Add(refItem);
    }
}
