using System.Collections.Generic;
using System;
using Unity.Netcode.Components;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(EntityController))]
public abstract class EntityStats : NetworkBehaviour
{
    // Server Autoritative
    [SerializeField]    protected TMP_Text nameTag;
    [SerializeField]    protected Rase rase;
    [SerializeField]    protected Slider hpBar;
    [SerializeField]    protected GameObject body;
    [SerializeField]    protected NetworkObject netObject;
    [SerializeField]    protected Transform attackPoint;
    [SerializeField]    protected SpriteRenderer weaponR, weaponL;
    [SerializeField]    protected NetworkAnimator animator;
    [SerializeField]    protected Rigidbody2D rb;
    [SerializeField]    protected NetworkList<Rezistance> rezists = new();
    [SerializeField]    protected NetworkVariable<int> maxHp = new();
                        protected NetworkVariable<int> hp = new();
                        public NetworkVariable<float> speed = new();
                        public NetworkVariable<byte> level = new();
                        public NetworkVariable<bool> IsAlive = new(true);
                        protected NetworkVariable<Attack> attack = new ();
                        protected NetworkVariable<FixedString128Bytes> weapRef = new();
                        protected const float timeToDespawn = 0f;
                        protected float HP { get { return (float)hp.Value/(float)maxHp.Value; } }

                        public NetworkObject NetObject { get { return netObject; } }
                        public Animator Animator { get { return animator.Animator; } }
                        public Rigidbody2D RigidBody2D { get { return rb; } }

    public override void OnNetworkSpawn()
    {
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

            for (int i = 0; i < rase.rezistances.Count; i++)
                rezists.Add(rase.rezistances[Enum.GetName(typeof(Damage.Type), i)]);    

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
        if (IsServer)
        {
            //int newDamage = rezists[(int)damage.type].GetDamage(damage.amount);
            //hp.Value -= newDamage;

            //Debug.Log($"Damage {damage.amount} from redused by Rezists to {newDamage}");
            
            if (hp.Value <= 0)
                return true;
        }
        return false;
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

[Serializable] public struct Rezistance : INetworkSerializable, IEquatable<Rezistance>
{
    [field: SerializeField] public int amount;
    // Amount value     (-∞ <=> ∞) Stacks with +
    // Precentil value  (-1 <=> 1) Stacks with avg
    public Damage.Type Damage   { get { return damageType; }    }
    public Equipment.Slot Slot  { get { return slot; }          }

    private Equipment.Slot slot;
    [field: SerializeField] private Damage.Type damageType;

    public Rezistance (int _amount, Equipment.Slot _slot, Damage.Type _damageType)
    {
        damageType = _damageType;
        amount = _amount; 
        slot = _slot;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref damageType);
        serializer.SerializeValue(ref amount);
        serializer.SerializeValue(ref slot);
    }
    public bool Equals(Rezistance other)
    {
        return other.amount == amount;
    }
    public static float CalculateDMG(List<Rezistance> rezists, Damage dmg)
    {
        float rez = 0f, avg = 0f;

        rezists= rezists.FindAll(r => r.damageType == dmg.type);
        // 
        rezists.ForEach(r => rez += (r.amount > 1) ? r.amount : 0f); // pre vsetky Celkove rezisty

        if (rez > dmg.amount)   // ak je damage vacsi ako velkost Celkovych rezistov
        {
            int count = 0;
             rezists.ForEach(r => { // pre prercentualne rezisty
                if (r.amount < 1) 
                {
                    count++; 
                avg+=r.amount;
                }});    
        }

        return rez;
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
    public enum Type
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