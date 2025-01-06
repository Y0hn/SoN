using Unity.Collections;
using Unity.Netcode;
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
    [SerializeField] Camera cam;
    //public RpcParams OwnerRPC { get { return RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp); } }
    protected float chatTimer; const float chatTime = 5.0f;
    protected XpSliderScript xpBar;       // UI nastavene len pre Ownera
    protected GameManager game;
    protected Inventory inventUI;
            protected NetworkVariable<int> xp = new(0);
            protected NetworkVariable<int> xpMax = new(10, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
            protected NetworkList<FixedString64Bytes> inventory = new(null, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
            protected NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
            public NetworkVariable<FixedString128Bytes> message = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    protected SkillTree skillTree;  // iba na servery
   
    public override Attack Attack 
    { 
        get { 
            Attack a = new(base.Attack);
            if (a.IsSet)
                a.AddDamage(skillTree.GetDamage(base.Attack.damage.type)); 
            return a;
        } 
    }
    public override Defence Defence
    {
        get {
            return base.Defence;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TryLoadServerData();
    }
    protected override void Update()
    {
        if (chatTimer != 0 && chatTimer <= Time.time)
        {
            chatField.SetActive(false);
            chatBox.text = "";
            chatTimer = 0;
        }
    }
    protected override void TryLoadServerData()
    {
        
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        // Ulozi hodnoty na servery
        FileManager.SaveClientData(this);
    }
    protected override void EntitySetUp()
    {
        base.EntitySetUp();

        if (IsOwner)
        {
            game = GameManager.instance;
            inventUI = GameManager.instance.inventory;
            hpBar.gameObject.SetActive(false);
            hpBar = game.GetHpBar();
            xpBar = game.GetXpBar();
            xpMax.Value = 100;
            xpBar.SliderValue = xp.Value;
            xpBar.LevelUP(level.Value, xpMax.Value);

            playerName.Value = game.PlayerName;
        }
        chatTimer = 0;
        chatBox.text = "";
        skillTree = new();
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
        GetComponent<NetworkObject>().name = nameTag.text;
    }
    protected override void SubsOnNetValChanged()
    {
        base.SubsOnNetValChanged();
        if (IsServer)
        {
            xpMax.OnValueChanged += (int prev, int newValue) => level.Value++;
            xp.OnValueChanged += (int prevValue, int newValue) => 
            {
                if (xpMax.Value <= newValue)
                {
                    int add = level.Value * 10;
                    xpMax.Value += xpMax.Value + add;
                }
            };
        }
        playerName.OnValueChanged += (FixedString32Bytes prevValue, FixedString32Bytes newValue) => 
        { 
            nameTag.text = newValue.ToString(); 
        };
        message.OnValueChanged += (FixedString128Bytes prevValue, FixedString128Bytes newMess) => 
        {
            chatBox.text = newMess.ToString();
            chatField.SetActive(true);
            chatTimer = Time.time + chatTime;
        };
    }
    protected override void OwnerSubsOnNetValChanged()
    {
        if (!IsOwner) return;
        base.OwnerSubsOnNetValChanged();
        xp.OnValueChanged += (int prevValue, int newValue) => 
        { 
            xpBar.SliderValue = newValue; 
            
        };
        /*xpMax.OnValueChanged += (int prevValue, int newValue) =>
        {
            xpBar.AddMax(xpMax.Value);
        };*/
        hp.OnValueChanged += (int prev, int now) => 
        {
            if (now < prev)
                game.AnimateFace("got-hit");
            game.AnimateFace(HP);
        };
        inventory.OnListChanged += (NetworkListEvent<FixedString64Bytes> changeEvent) => 
        {
            OnInventoryUpdate(changeEvent);
        };
        IsAlive.OnValueChanged  += (bool perv, bool now) => 
        {
            game.AnimateUI("isAlive", now);
            cam.gameObject.SetActive(now);
            game.SetPlayerUI(now);
        };
        level.OnValueChanged += (byte prev, byte now) =>
        {
            xpBar.LevelUP(now, xpMax.Value);
        };
    }
    protected void OnInventoryUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)  // iba lokalne
    {        
        switch (changeEvent.Type)
        {
            case NetworkListEvent<FixedString64Bytes>.EventType.Add:
                //int index = changeEvent.Index; // the position of the added value in the list
                inventUI.Add(changeEvent.Value.ToString()); // the new value at the index position
                break;
            case NetworkListEvent<FixedString64Bytes>.EventType.Remove:
            case NetworkListEvent<FixedString64Bytes>.EventType.RemoveAt:
                inventUI.Remove(changeEvent.Value.ToString());
                break;
            case NetworkListEvent<FixedString64Bytes>.EventType.Full:
            case NetworkListEvent<FixedString64Bytes>.EventType.Clear:
            case NetworkListEvent<FixedString64Bytes>.EventType.Value:
            case NetworkListEvent<FixedString64Bytes>.EventType.Insert:
            default:
                break;
        }
    }
    public virtual void SetWeaponIndex (sbyte id)
    {
        sbyte att, wea= -1;

        att = (sbyte)(id % 10 - 1);
        if (0 <= att)
            wea = id/10 == 1 ? (sbyte)Equipment.Slot.WeaponR : (sbyte)Equipment.Slot.WeaponL;
        
        //Debug.Log($"Setting weapon index to new(att= {att} | wea= {wea})");
        SetWeaponIndex(att, wea);
    }
    public override bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
            return false;

        int newDamage = defence.CalculateDMG(damage, skillTree.GetResist(damage.type));
        hp.Value -= newDamage;
        
        // if (FileManager.debug)
        //Debug.Log($"Damage {damage.amount} from redused by Rezists to {newDamage}");
        
        if (hp.Value <= 0)
            IsAlive.Value = false;

        return !IsAlive.Value;
    }
    public override bool AttackTrigger()
    {
        return base.AttackTrigger();
    }
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        xp.Value += died.level.Value * 5 ;
    }    
    [Rpc(SendTo.Server)] public override void PickedUpRpc(string refItem)
    {
        inventory.Add(refItem);
    }
    [Rpc(SendTo.Server)] public virtual void AddSkillRpc(SkillTree.Skill skill)
    {
        skillTree.Add(skill);
    }
    [Rpc(SendTo.Server)] public void AddXPRpc(int xp)
    {
        Debug.Log("XP added to player");
        this.xp.Value += xp;
    }
}