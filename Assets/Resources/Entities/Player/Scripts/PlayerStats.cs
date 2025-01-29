using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
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

    protected NetworkVariable<uint> xp = new(0);
    protected NetworkVariable<uint> xpMax = new(10, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    protected NetworkList<FixedString64Bytes> inventory = new(null, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    protected NetworkList<FixedString64Bytes> equipment = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // je to list ale sprava sa ako Dictionary
    protected NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString128Bytes> message = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    protected SkillTree skillTree;  // iba na servery
   
    public Projectile Projectile { get; set; }
    protected Equipment[] Equipments     
    { 
        get 
        {
            List<Equipment> eq = new();
            foreach (var e in equipment)
                eq.Add(Equipment.GetItem(e.ToString()));
            //Debug.Log($"Additional weapons {eq.Count}");
            return eq.ToArray();
        }
    }
    protected override Weapon[] Weapons 
    { 
        get 
        { 
            
            var w = base.Weapons.ToList();
            w.AddRange(Equipments); 
            //Debug.Log($"Returning weapons [{w.Count}]");
            return w.ToArray();
        } 
    }
    public override Attack Attack 
    { 
        get {
            Attack a = new(base.Attack);
            if (IsServer && a.IsSet)
                a.AddDamage(SkillsTree.GetDamage(a.damage.type)); 
            return a;
        } 
    }
    protected SkillTree SkillsTree  
    {
        get 
        {
            if (IsServer)
                return skillTree;
            else
                Debug.LogWarning("");
            return null;
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
        if (IsServer)
        {
            xpMax.Value = 50;
            level.Value = 0;
        }
        if (IsOwner)
        {
            game = GameManager.instance;
            inventUI = GameManager.instance.inventory;
            hpBar.gameObject.SetActive(false);
            hpBar = game.GetHpBar();
            xpBar = game.GetXpBar();
            xpBar.SliderValue = xp.Value;
            xpBar.LevelUP(1, xpMax.Value);

            // Nastavenie 
            int length = Enum.GetNames(typeof(Equipment.Slot)).Length-1;
            for (; equipment.Count < length;)
                equipment.Add("");

            playerName.Value = game.PlayerName;
        }
        chatTimer = 0;
        chatBox.text = "";
        skillTree = new();
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
        onDeathWait = false;
        GetComponent<NetworkObject>().name = nameTag.text;
    }
    protected override void SubsOnNetValChanged()
    {
        base.SubsOnNetValChanged();
        if (IsServer)
        {
            xpMax.OnValueChanged += (uint prev, uint newValue) => level.Value++;
            xp.OnValueChanged += (uint prevValue, uint newValue) => 
            {
                if (xpMax.Value <= newValue)
                {
                    // prida potrebne exp do dalsieho levelu
                    int add = (level.Value+1) * 10;
                    xpMax.Value += (uint)(xpMax.Value + add);
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
        xp.OnValueChanged += (uint prevValue, uint newValue) => 
        { 
            xpBar.SliderValue = newValue; 
            
        };
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
            xpBar.LevelUP((byte)(now+1), xpMax.Value);
        };
    }
    protected override void Die()
    {
        OnDeath?.Invoke();
        OnDeath = null;
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
    public void SetWeaponIndex (sbyte id)
    {
        sbyte att, wea= -1;

        if (id < 0)
        {
            att = 0;
            wea = 0;
        }
        else 
        {
            att = (sbyte)(id % 10 - 1);
            if (0 <= att)
                wea = id/10 == 1 ? (sbyte)Equipment.Slot.WeaponR : (sbyte)Equipment.Slot.WeaponL;
            wea++;
        }
        Debug.Log($"Setting ID={id} to weapon index to new(att= {att} | wea= {wea})\nWeapons count: {Weapons.Length}");
        SetWeaponIndex(att, wea);
    }
    public override bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
            return false;

        int newDamage = Defence.CalculateDMG(damage, skillTree.GetResist(damage.type));
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
        xp.Value += (uint)(died.level.Value * 100);
    }
    public virtual bool TryInteruptAttack()
    {
        bool canStop = Attack.IsRanged;

        if (canStop)
        {
            StopRanAttackRpc();
            atTime = 0;
        }

        return canStop;
    }

    // RPCs
    [Rpc(SendTo.Server)] public virtual void AddSkillRpc(SkillTree.Skill skill)
    {
        skillTree.Add(skill);
    }
    
    /// <summary>
    /// Zastavi utok
    /// </summary>
    [Rpc(SendTo.Server)] protected void StopRanAttackRpc()
    {
        Projectile.StopAttack();
    }
    /// <summary>
    /// Pridáva/Odoberā zbrame
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="slot"></param>
    [Rpc(SendTo.Server)] public void SetEquipmentRpc(string reference, Equipment.Slot slot/* = Equipment.Slot.NoPreference*/)
    {
        equipment[(int)slot] = reference;
        //Debug.Log($"Equiped {Equipment.GetItem(reference).name} on slot {(int)slot}={slot} with Weapon {Weapon.GetItem(reference)}");
    }
    /// <summary>
    /// Zbiera a equipuje zbrane
    /// </summary>
    /// <param name="reference"></param>tile.Stop();
    [Rpc(SendTo.Server)] public virtual void PickedUpRpc(string reference)
    {
        inventory.Add(reference);
    }
    [Rpc(SendTo.Server)] public void AddLvlRpc()
    {
        //Debug.Log("XP added to player");
        xp.Value = xpMax.Value;
    }
}