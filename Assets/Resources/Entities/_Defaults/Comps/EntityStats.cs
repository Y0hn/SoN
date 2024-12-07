using Unity.Netcode.Components;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
/// <summary>
/// Drzi hodnoty pre entitu
/// </summary>
public abstract class EntityStats : NetworkBehaviour
{
    // Server Autoritative
    [SerializeField] protected TMP_Text nameTag;
    [SerializeField] protected Rase rase;
    [SerializeField] protected Slider hpBar;
    [SerializeField] protected GameObject body;
    [SerializeField] protected NetworkObject netObject;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected SpriteRenderer weaponR, weaponL;
    [SerializeField] protected NetworkAnimator animator;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected AITarget aiTeam = AITarget.Team_2;

    [SerializeField]    protected   NetworkList<FixedString64Bytes> equipment = new(/*null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner*/);    
    // Sprava sa ako Dictionary ale je to list
    [SerializeField]    protected   NetworkVariable<int> maxHp = new();
                        protected   NetworkVariable<int> hp = new();
                        public      NetworkVariable<float> speed = new();
                        public      NetworkVariable<byte> level = new(1);
                        public      NetworkVariable<bool> IsAlive = new(true);
                        protected   NetworkVariable<Attack> attack = new();
                        protected   NetworkVariable<WeaponIndex> weapE = new(new(-1), NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
    public float HP                 { get { return (float)hp.Value/(float)maxHp.Value; } }
    public NetworkObject NetObject  { get { return netObject; } }
    public Rigidbody2D RigidBody2D  { get { return rb; } }
    public AITarget TargetTeam      { get { return aiTeam; } }
    public Animator Animator        { get { return animator.Animator; } }
    public bool AttackBoth          { get { return attack.Value.bothHanded; } }
    public bool Armed               { get { return equipment[(int)Equipment.Slot.WeaponL] != "" || "" !=  equipment[(int)Equipment.Slot.WeaponR]; } }
    protected Defence defence;  // iba na servery/hoste
    protected float atTime = 0;
    protected const float timeToDespawn = 0f;
    private bool clampedDMG = true;

    public override void OnNetworkSpawn()
    {
        EntitySetUp();
        SubsOnNetValChanged();

        hpBar.value = hp.Value;
    }
    protected virtual void SubsOnNetValChanged()
    {
        if (IsServer)
            maxHp.OnValueChanged += (int prevValue, int newValue) => hp.Value = maxHp.Value;

        equipment.OnListChanged += (NetworkListEvent<FixedString64Bytes> listEvent) => OnEquipmentUpdate(listEvent);
        attack.OnValueChanged   += (Attack prevValue, Attack newValue) => 
        {
            if (newValue.type == Attack.Type.RaseUnnarmed)
            {
                weaponL.gameObject.SetActive(false);
                weaponR.gameObject.SetActive(false);
            }
            Animator.SetFloat("weapon", (float)newValue.type);
            attackPoint.localPosition = new(attackPoint.localPosition.x, newValue.range);
        };
        IsAlive.OnValueChanged  += (bool prev, bool alive)          => SetLive(alive);
        hp.OnValueChanged       += (int prevValue, int newValue)    => OnHpUpdate();
    }
    protected virtual void EntitySetUp()
    {
        name = name.Split('(')[0].Trim();
        if (IsServer)
        {
            attack.Value = new(rase.attack);

            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            level.Value = 1;

            speed.Value = rase.speed;
            
            int length = Enum.GetNames(typeof(Equipment.Slot)).Length;
            for (; equipment.Count < length;)
                equipment.Add("");

            defence = new(rase.naturalArmor);
            IsAlive.Value = true;
        }
        else // requesting changed Values from server
        {
            foreach (FixedString64Bytes e in equipment)
                if (e != "")
                    UpdateEquipment(Equipment.GetItem(e.ToString()));
        }
        attackPoint.localPosition = new(attackPoint.localPosition.x, attack.Value.range);
    }
    protected virtual void Update()
    {

    }
    protected virtual void OnHpUpdate()
    {
        float value = HP;
        hpBar.value = value;
    } 
    protected virtual void OnEquipmentUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        string curr = changeEvent.Value.ToString();
        string prev = changeEvent.PreviousValue.ToString();
        Equipment.Slot slot = (Equipment.Slot)changeEvent.Index;

        if (changeEvent.Type == NetworkListEvent<FixedString64Bytes>.EventType.Value)
        {
            if (IsServer && curr == "" )
            {
                if      (Equipment.IsArmor(slot))
                    defence.Remove(Armor.GetItem(prev));
                else if (Equipment.IsWeapon(slot))
                    attack.Value = rase.attack;
            }
            else
            {
                UpdateEquipment(Equipment.GetItem(curr));
            }
        }
        else
        {
            Debug.LogWarning("Equipment corrupted");
            equipment.SetDirty(true);
        }
    }
    protected virtual void UpdateEquipment(Equipment equip)
    {
        if      (equip is Armor a)
        {
            defence.Add(a);
        }
        else if (equip is Weapon w)
        {
            if (IsServer) // !Armed
                attack.Value = w.attack[0];
            Sprite sprite = Resources.Load<Sprite>(w.SpriteRef);
            weaponL.sprite = sprite;
            weaponL.color = w.color;
            weaponR.sprite = sprite;
            weaponR.color = w.color;
            bool 
            R = w.slot == Equipment.Slot.WeaponR,                             
            L = w.slot == Equipment.Slot.WeaponL, 
            B = w.slot == Equipment.Slot.WeaponBoth;
            weaponR.gameObject.SetActive(R || B); 
            weaponL.gameObject.SetActive(L || B);
            float atBlend = (R || B) ? 1 : -1;
            Animator.SetFloat("atBlend", atBlend);
        }
    }
    public virtual bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
            return false;
        /*Debug.Log("Called from not server ");*/ 

        int newDamage = defence.CalculateDMG(damage, clampedDMG);
        hp.Value -= newDamage;
        Debug.Log($"Damage {damage.amount} from redused by Rezists to {newDamage}");
        
        if (hp.Value <= 0)
            IsAlive.Value = false;

        return !IsAlive.Value;
    }
    public virtual bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            AttackRpc();
            atTime = Time.time + 1/attack.Value.rate;
            return true;
        }
        return false;
    }
    public virtual void KilledEnemy(EntityStats died)
    {

    }
    protected virtual void SetLive(bool alive)
    {
        if (IsServer && alive)
            hp.Value = maxHp.Value;
        hpBar.gameObject.SetActive(alive);
        // Play animation 
        gameObject.SetActive(alive);
    }
    [Rpc(SendTo.Server)] public void SetEquipmentRpc(string reference, Equipment.Slot slot = Equipment.Slot.NoPreference)
    {
        if (slot == Equipment.Slot.NoPreference && reference != "")
            slot = ((Equipment)Item.GetItem(reference)).slot;
        equipment[(int)slot] = reference;
    }
    [Rpc(SendTo.Server)] protected void AttackRpc()
    {
        Debug.Log("SERVER RPC attack !");
        if      (Attack.MeleeAttack(attack.Value.type))
        {
            foreach(EntityStats et in MeleeAttack())
                if (et.IsAlive.Value && et.TakeDamage(attack.Value.damage))    // pravdive ak target zomrie
                    KilledEnemy(et);
        }
        else if (Attack.RangedAttack(attack.Value.type))
        {

        }
        else 
            Debug.Log($"Player {name} attack type {Enum.GetName(typeof(Attack.Type), attack.Value.type)} not yet defined");
    }
    protected virtual EntityStats[] MeleeAttack()
    {
        List<EntityStats> targetStats = new();
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attack.Value.range /*, layer mask */);
        foreach (Collider2D target in targets)
            if (target.TryGetComponent(out EntityStats stats))
                if (stats != this)
                    targetStats.Add(stats);

        Debug.Log("Melee Hitted " + targetStats.Count + " targets");
        return targetStats.ToArray();
    }
    [Rpc(SendTo.Server)] public virtual void SetAttackTypeRpc(byte t)
    {
        t--;
        List<Attack> a = ((Weapon)Item.GetItem(equipment[(int)Equipment.Slot.WeaponR].ToString())).attack;
        if (a.Count > t)
            attack.Value = a[t];
    }
    [Rpc(SendTo.Server)] public virtual void PickedUpRpc(string reference)
    {
        Equipment e = Equipment.GetItem(reference);
        equipment[(int)e.slot] = e.GetReferency;
    }
}
[Serializable] public class Defence
{
    List<Armor> armors;
    public Defence()
    {
        armors = new();
    }
    public Defence(Armor armor)
    {
        armors = new();
        Add(armor);
    }
    public int CalculateDMG(Damage damage, bool clamp = true)
    {
        float sum = 0f, per = 0f;

        armors.ForEach(a=> a.GetElemental(damage.type).ForEach(r=>  // vyberie len 1 damage type
        {   if (r.amount < 1)
                per = (per < r.amount && r.amount < 1) ? r.amount: per;     // scita Pocetny rezisty
            else
                sum += (r.amount > 1) ? r.amount : 0f;                      // najvacsi percentualny Rezist
        }));
        per *= damage.amount;  // nastavi ciselnu vysku 


        int recieved = Mathf.RoundToInt(damage.amount - (sum + per));
        if (clamp)
            recieved = Math.Clamp(recieved, 0, int.MaxValue);
        return recieved;
    }
    public bool Add(Armor a, bool over = false)
    {
        if (!armors.Contains(a))
        {
            Armor ar = armors.Find(ar=> ar.slot == a.slot);
            if (ar != null || over)
                armors.Remove(ar);
            armors.Add(a);
            return true;
        }
        return false;
    }
    public bool Remove(Armor a)
    {
        if (armors.Contains(a))
        {
            armors.Remove(a);
            return true;
        }
        return false;
    }
    public Class CallculateDC()
    {
        if (armors.Count > 0)
        {
            float total = 0;
            int count = 0;

            foreach (Armor a in armors)
                foreach (Armor.Resistance r in a.resists)
                {
                    total += (r.amount > 1) ? r.amount : r.amount * 100;
                    count++;
                }
            total /= count; 
            
            if      (total < 40)
                return Class.Small;
            else if (total < 70)
                return Class.Medium;
            else if (count < 5)
                return Class.Dedicated;
            else
                return Class.Heavy;
        }
        else
            return Class.None;
    }
    public enum Class  { None, Small, Medium, Heavy, Dedicated }
}
[Serializable] public struct Attack : INetworkSerializable, IEquatable<Attack>
{
    public Damage damage;
    public float range;
    public float rate;
    public Type type;
    public bool bothHanded;
    public bool IsSet { get { return range != 0 && rate != 0;} }
    public Attack (Damage damage, float range, float rate, Type type, bool both = false)
    {
        this.bothHanded = both;
        this.damage = damage;
        this.range = range;
        this.rate = rate;
        this.type = type;
    }
    public Attack (Attack attack)
    {
        this.bothHanded =  attack.bothHanded;
        this.damage =  attack.damage;
        this.range = attack.range;
        this.rate =  attack.rate;
        this.type =  attack.type;

    }
    public enum Type
    {
        RaseUnnarmed, MeleeSlash, MeleeStab, BowSingle, BowMulti // Magi
    }
    public static bool MeleeAttack(Type t)
    {
        return Type.RaseUnnarmed == t || t == Type.MeleeSlash || Type.MeleeStab == t;
    }
    public static bool RangedAttack(Type t)
    {
        return Type.BowSingle == t || t == Type.BowMulti;
    }
    public bool Equals (Attack other)
    {
        return
        other.type.Equals(type) &&
        other.rate.Equals(rate) &&
        other.range.Equals(range) && 
        other.damage.Equals(damage);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref bothHanded);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
        serializer.SerializeValue(ref type);
    }
    public override string ToString()
    {
        string s = "";

        s += $"Both handed: {bothHanded} | ";
        s += $"Damage: {damage} | ";
        s += $"Range: {range} | ";
        s += $"Rate: {rate} ac/s |";
        s += $"Attack Type: {Enum.GetName(typeof(Type), type)}";

        return s;
    }
}
[Serializable] public struct WeaponIndex : INetworkSerializable
{
    public int eIndex;
    public int aIndex;
    public WeaponIndex(int e, int a = 0)
    {
        eIndex = e;
        aIndex = a;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref eIndex);
        serializer.SerializeValue(ref aIndex);
    }
}
[Serializable] public struct Damage : INetworkSerializable, IEquatable<Damage>
{
    public Type type;
    public int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
    [Serializable] public enum Type
    {
        // STANDARD
        bludgeoning, slashing, piercing, 
        // ELEMENTAL
        cold, fire, holy, lightning, dark, acid,
        //  OVER TIME
        poison    
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref amount);
    }

    public bool Equals(Damage other)
    {
        return 
        type == other.type && 
        amount == other.amount;
    }
    public override string ToString()
    {
        string s = $"Type: {Enum.GetName(typeof(Type), type)} Amount: ";

        if (amount > 1)
            s += amount.ToString();
        else
            s += amount.ToString() + " %";

        return s;
    }
}
public struct BodyEquipment
{
    public Dictionary<Equipment.Slot, string> references;/* = new()
    {
        {Equipment.Slot.WeaponL,    ""},
        {Equipment.Slot.WeaponR,    ""},
        {Equipment.Slot.WeaponBoth, ""},
        //references.Add(Equipment.Slot.NoPreference,"");

        {Equipment.Slot.Head,       ""},
        {Equipment.Slot.Torso,      ""},
        {Equipment.Slot.Body,       ""},
        {Equipment.Slot.Legs,       ""}
    };*/

    public BodyEquipment(string s)
    {
        references = new();
    }
}
public enum AITarget 
{
    None, Player, Team_1, Team_2, Team_3, Boss
}