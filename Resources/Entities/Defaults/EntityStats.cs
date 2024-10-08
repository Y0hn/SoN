using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(EntityController))]
public abstract class EntityStats : NetworkBehaviour
{
    // Server Autoritative
    [SerializeField]    protected TMP_Text nameTag;
    [SerializeField]    protected NetworkList<Rezistance> rezists = new();
    [SerializeField]    protected NetworkVariable<int> maxHp = new();
                        protected NetworkVariable<int> hp = new();
    [SerializeField]    protected Slider hpBar;
                        public NetworkVariable<float> speed = new();
                        public NetworkVariable<byte> level = new();
    [SerializeField]    protected GameObject body;
    [SerializeField]    protected Transform attackPoint;
                        public NetworkVariable<bool> IsAlive = new(true);
    [SerializeField]    protected Rase rase;
                        protected NetworkVariable<Attack> attack = new ();
    [SerializeField]    protected NetworkVariable<Weapon> weapon = new ();
                        protected const float timeToDespawn = 0f;
                        protected float HP { get { return (float)hp.Value/(float)maxHp.Value; } }
                        protected SpriteRenderer weaponR, weaponL;

    public override void OnNetworkSpawn()
    {
        RaseSetUp();

        // Health values
        hp.OnValueChanged += (int prevValue, int newValue) => OnHpUpdate();
        maxHp.OnValueChanged += (int prevValue, int newValue) => 
        {
            if (IsServer)
                hp.Value = maxHp.Value;
        };
        hpBar.value = hp.Value;

        attackPoint.position = new(attackPoint.position.x, attack.Value.range);

        IsAlive.OnValueChanged += (bool prev, bool alive) => 
        { 
            SetLive(alive);
        };
        weapon.OnValueChanged += (Weapon p, Weapon n) =>
        {
            Sprite s = Resources.Load<Sprite>(n.spriteRef);

            weaponL.sprite = s;
            weaponR.sprite = s;
        };
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
            int newDamage = rezists[(int)damage.type].GetDamage(damage.amount);
            hp.Value -= newDamage;

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
    [ServerRpc] public virtual void SetWeaponServerRpc(Weapon newWeapon, bool unequip = false)
    {
        if (!IsServer) return;
        if (unequip)
        {
            attack.Value = rase.attack;
            weaponL.enabled = false;
            weaponR.enabled = false;
        }
        else
        {
            attack.Value = newWeapon.attack;
            weapon.Value = newWeapon;
        }
    }
}

[Serializable] public struct Rezistance : INetworkSerializable, IEquatable<Rezistance>
{
    [field: SerializeField] private  int rezAmount; // Amount value     (-∞ <=> ∞)
    // Stacks with avg
    [field: SerializeField] private  float rezTil;  // Precentil value  (-1 <=> 1)
    // Stacks with +

    public Rezistance (int amount, float percetil)
    {
        rezAmount = amount;
        rezTil = percetil;            
    }
    public void ModRez(int amount)      { rezAmount += amount;  }
    public void ModRez(float percetil)  { rezTil += percetil;   }
    public int GetDamage(int damage)
    {
        return (int)Mathf.Round(damage * (1 - rezTil) - rezAmount);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref rezAmount);
        serializer.SerializeValue(ref rezTil);
    }
    public bool Equals(Rezistance other)
    {
        //return other.rezAmount == rezAmount && other.rezTil == rezTil;
        return false;
    }
}

[Serializable] public struct Attack : INetworkSerializable
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
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
    }
}

[Serializable] public struct Damage : INetworkSerializable
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
}