using System.Collections.Generic;
using System;
using Unity.Netcode.Components;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using AYellowpaper.SerializedCollections;
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(EntityController))]
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
    protected Defence defence;  // iba na servery/hoste Servery

    [SerializeField]    protected NetworkVariable<int> maxHp = new();
                        protected NetworkVariable<int> hp = new();
                        public NetworkVariable<float> speed = new();
                        public NetworkVariable<byte> level = new(1);
                        public NetworkVariable<bool> IsAlive = new(true);
                        protected NetworkVariable<Attack> attack = new ();
                        protected NetworkVariable<FixedString128Bytes> weapRef = new();

                        protected const float timeToDespawn = 0f;
                        public float HP { get { return (float)hp.Value/(float)maxHp.Value; } }
                        public NetworkObject NetObject { get { return netObject; } }
                        public Animator Animator { get { return animator.Animator; } }
                        public Rigidbody2D RigidBody2D { get { return rb; } }
                        private bool clampedDMG = true;

    public override void OnNetworkSpawn()
    {
        name = name.Split('(')[0].Trim();
        RaseSetUp();

        SubscribeOnNetworkValueChanged();

        attackPoint.position = new(attackPoint.position.x, attack.Value.range);
        hpBar.value = hp.Value;
        IsAlive.OnValueChanged += (bool prev, bool alive) => SetLive(alive);
    }
    private void SubscribeOnNetworkValueChanged()
    {
        hp.OnValueChanged += (int prevValue, int newValue) => OnHpUpdate();
        maxHp.OnValueChanged += (int prevValue, int newValue) => 
        {
            if (IsServer)
                hp.Value = maxHp.Value;
        };        
        weapRef.OnValueChanged += (FixedString128Bytes o, FixedString128Bytes s) =>
        {
            if (s != "") 
            {
                Sprite texture = Resources.Load<Sprite>(s.ToString());
                weaponL.sprite = texture;
                weaponR.sprite = texture;
            }
            weaponL.enabled = s != "";
            weaponR.enabled = s != "";
        };
        attack.OnValueChanged += (Attack prevValue, Attack newValue) => Animator.SetFloat("weapon", (float)newValue.type);
    }
    protected virtual void RaseSetUp()
    {
        if (IsServer)
        {
            attack.Value = rase.attack;

            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            level.Value = 1;

            speed.Value = rase.speed;
            
            defence = new(rase.naturalArmor);

            IsAlive.Value = true;
        }
    }
    protected virtual void Update()
    {

    }
    protected virtual void OnHpUpdate()
    {
        float value = HP;
        hpBar.value = value;
    } 
    public virtual bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
        { Debug.Log("Called from not server "); return false; }

        int newDamage = defence.CalculateDMG(damage, clampedDMG);
        hp.Value -= newDamage;
        Debug.Log($"Damage {damage.amount} from redused by Rezists to {newDamage}");
        
        if (hp.Value <= 0)
            IsAlive.Value = false;

        return !IsAlive.Value;
    }
    protected virtual EntityStats[] MeleeAttack()
    {
        List<EntityStats> targetStats = new();
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attack.Value.range /*, layer mask */);
        foreach (Collider2D target in targets)
            if (target.TryGetComponent(out EntityStats stats))
                if (stats != this)
                    targetStats.Add(stats);

        return targetStats.ToArray();
    }
    public virtual bool AttackTrigger()
    {
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
}
[Serializable] public struct Attack : INetworkSerializable, IEquatable<Attack>
{
    public Damage damage;
    public float range;
    public float rate;
    public Type type;
    public Attack (Damage damage, float range, float rate, Type type)
    {
        this.damage = damage;
        this.range = range;
        this.rate = rate;
        this.type = type;
    }
    public enum Type
    {
        RaseUnnarmed, MeleeSlash//, MeleeStab, Melee, RangeBow
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
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
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