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
    [SerializeField] protected ColorChainReference color;
    [SerializeField] protected SpriteRenderer weaponR, weaponL;
    [SerializeField] protected NetworkAnimator animator;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Collider2D coll;
    [SerializeField] protected AITarget aiTeam = AITarget.Team_2;
    [SerializeField] protected EntityController controller;
    [SerializeField]    protected   NetworkVariable<int> maxHp = new();
                        protected   NetworkList<FixedString64Bytes> equipment = new(/*null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner*/);    
                        // je to list ale sprava sa ako Dictionary
                        protected   NetworkVariable<int> hp = new();
                        public      NetworkVariable<float> speed = new();
                        public      NetworkVariable<byte> level = new(1);
                        public      NetworkVariable<bool> IsAlive = new(true);
                        protected   NetworkVariable<Attack> weaponAttack = new();
                        protected   NetworkVariable<WeaponIndex> weapE = new(new(-1), NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
#pragma warning disable IDE0004
    public float HP                     { get => (float)hp.Value/(float)maxHp.Value; }
#pragma warning restore IDE0004
    public virtual Quaternion Rotation  { get => transform.rotation; }
    public NetworkObject NetObject      { get => netObject; }
    public Rigidbody2D RigidBody2D      { get => rb; }
    public Transform AttackPoint        { get => attackPoint; }
    public Weapon EquipedWeapon         { get => Resources.Load<Weapon>(equipment[weapE.Value.eIndex].ToString()); }
    public AITarget TargetTeam          { get => aiTeam; }
    public Animator Animator            { get => animator.Animator; }
    public Vector2 View                 { get => controller.View; }
    public Color Color                  { get => color.Color; }
    public float ViewAngle              { get => Mathf.Atan2(View.x, View.y); }
    public bool AttackBoth              { get => weaponAttack.Value.bothHanded; }
    public bool Armed                   { get => equipment[(int)Equipment.Slot.WeaponL] != "" || "" !=  equipment[(int)Equipment.Slot.WeaponR]; }
    public virtual Defence Defence      
    { 
        get 
        { 
            if (IsServer) 
                return defence; 
            else
                return new();
        } 
    }
    public virtual Attack Attack        
    { 
        get 
        {
            if (IsServer)
                return weaponAttack.Value;
            else
                return new();
        }
    }
    public Action OnDeath;
    protected Defence defence;  // iba na servery/hoste
    protected float timeToDespawn = 0f;
    protected float atTime = 0;
    //private bool clampedDMG = true;
    public const float RANGED_ANIMATION_DUR = 1.5f, MELEE_ANIMATION_DUR = 1;
    public override void OnNetworkSpawn()
    {
        EntitySetUp();
        SubsOnNetValChanged();
        OwnerSubsOnNetValChanged(); 

        hpBar.value = hp.Value;
    }
    protected virtual void SubsOnNetValChanged()
    {
        if (IsServer)
        {
            weaponAttack.OnValueChanged += (Attack prevValue, Attack newValue) => 
            {
                if      (Attack.MeleeAttack(newValue.type))
                {
                    attackPoint.localPosition = new(attackPoint.localPosition.x, newValue.range);
                }
                else if (Attack.RangedAttack(newValue.type))
                {
                    Ranged r = (Ranged)EquipedWeapon;
                    attackPoint.localPosition = new(r.projSpawnPosition.x, r.projSpawnPosition.y);
                }
            };
            weapE.OnValueChanged += (WeaponIndex prevValue, WeaponIndex newValue) =>
            {
                if (newValue.Holding)
                {
                    try {
                        weaponAttack.Value = Weapon.GetItem(equipment[newValue.eIndex].ToString()).attack[newValue.aIndex];
                    } catch (Exception ex) {
                        string eqs = "";
                        for (int i = 0; i < equipment.Count; i++)
                        {
                            eqs += $"\n[{i}.]" + equipment[i].ToString();
                        }
                        Debug.LogWarning($"Equipment not set on\nWeaponIndex[{newValue}]\nEquipment: {eqs}\n" + ex.Message);
                    }
                }
                else
                    weaponAttack.Value = rase.attack;
            };
            maxHp.OnValueChanged += (int prevValue, int newValue) => 
            {
                hp.Value = maxHp.Value;
            };
        }
        equipment.OnListChanged += OnEquipmentUpdate;
        weaponAttack.OnValueChanged   += (Attack prevValue, Attack newValue) => 
        {
            bool 
                R = false, 
                L = false, 
                B = false;
            if (newValue.type != Attack.Type.RaseUnnarmed)
            {
                Weapon w = EquipedWeapon;
                Sprite sprite = Resources.Load<Sprite>(w.SpriteRef);
                //Debug.Log($"Setted weapon sprite to \"{w.SpriteRef}\"");
                weaponL.sprite = sprite;
                weaponL.color = w.color;
                weaponR.sprite = sprite;
                weaponR.color = w.color;
                R = w.slot == Equipment.Slot.WeaponR;
                L = w.slot == Equipment.Slot.WeaponL;
                B = w.slot == Equipment.Slot.WeaponBoth;
            }

            weaponR.gameObject.SetActive(R || B); 
            weaponL.gameObject.SetActive(L || B);
            if (IsOwner)
            {
                float atBlend = (R || B) ? 1 : -1;
                Animator.SetFloat("atBlend", atBlend);
            }
        };
        IsAlive.OnValueChanged  += (bool prev, bool alive) => 
        {
            if (IsServer && alive)
                hp.Value = maxHp.Value;
            hpBar.gameObject.SetActive(alive);
            coll.enabled = alive;
            if (!alive)
            {
                timeToDespawn = Time.time + 5f;

                OnDeath?.Invoke();
            }
        };
        hp.OnValueChanged += OnHpUpdate;
    }
    protected virtual void OwnerSubsOnNetValChanged()
    {
        // Server / Owner
        weaponAttack.OnValueChanged += (Attack old, Attack now) =>
        {
            Animator.SetFloat("weapon", (float)now.type);

            float speed = now.IsMelee ? MELEE_ANIMATION_DUR : RANGED_ANIMATION_DUR;
            speed /= now.AttackTime;
            Animator.SetFloat("atSpeed", speed);
        };
        IsAlive.OnValueChanged += (bool old, bool now) =>
        {            
            Animator.SetBool("isAlive", now);
        };
    }
    protected virtual void EntitySetUp()
    {
        name = name.Split('(')[0].Trim();
        if (IsServer)
        {
            weaponAttack.Value = new (rase.attack);

            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            level.Value = 1;

            speed.Value = rase.speed;
            
            int length = Enum.GetNames(typeof(Equipment.Slot)).Length;
            for (; equipment.Count < length;)
                equipment.Add("");

            defence = new(rase.resists);
            IsAlive.Value = true;
        }
        attackPoint.localPosition = new(attackPoint.localPosition.x, weaponAttack.Value.range);
    }
    protected virtual void Update()
    {
        if (IsServer && timeToDespawn != 0 && timeToDespawn < Time.time)
            netObject.Despawn();
    }
    protected virtual void OnHpUpdate(int prev, int now)
    {
        float value = HP;
        hpBar.value = value;
    } 
    protected virtual void OnEquipmentUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        if (changeEvent.Type != NetworkListEvent<FixedString64Bytes>.EventType.Value)
        {
            Debug.LogWarning("Equipment corrupted");
            equipment.SetDirty(true);
        }
        /*
        string eq = "Equipment Update\n";
        for (int i = 0; i < equipment.Count; i++)
            eq += $"[{i}.] Equiped= {equipment[i]!=""} | Path= {equipment[i]}\n";
        Debug.Log(eq + $"\n Event.Value= {curr}");
        */
    }    
    
    public virtual bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
            return false;

        int newDamage = defence.CalculateDMG(damage);
        hp.Value -= newDamage;
        
        // if (FileManager.debug)
        //Debug.Log($"Damage {damage.amount} from redused by Rezists to {newDamage}");
        
        if (hp.Value <= 0)
            IsAlive.Value = false;

        return !IsAlive.Value;
    }
    public virtual bool AttackTrigger()
    {
        if (Time.time >= atTime)
        {
            AttackRpc();
            atTime = Time.time + 1/weaponAttack.Value.rate;
            return true;
        }
        return false;
    }
    public virtual void KilledEnemy(EntityStats died)
    {

    }
    public virtual void SetWeaponIndex (sbyte attack, sbyte weapon= -1)
    {
        if      (weapon < 0 && 0 <= attack)
            weapE.Value = new (weapE.Value.eIndex, attack);
        else if (0 <= weapon && 0 <= attack)
            weapE.Value = new (weapon, attack);
        else if (attack < 0)
            weapE.Value = new (-1, -1);
    }
    
    protected virtual void TryLoadServerData()
    {

    }
    protected virtual EntityStats[] MeleeAttack()
    {
        List<EntityStats> targetStats = new();
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, weaponAttack.Value.range /*, layer mask */);
        foreach (Collider2D target in targets)
            if (target.TryGetComponent(out EntityStats stats))
                if (stats != this)
                    targetStats.Add(stats);

        //Debug.Log("Melee Hitted " + targetStats.Count + " targets");
        return targetStats.ToArray();
    }
    protected virtual void RangedAttack()
    {
        if (weapE.Value.eIndex >= 0 && weapE.Value.aIndex >= 0)
        {
            Ranged r = Resources.Load<Ranged>(equipment[weapE.Value.eIndex].ToString());
            //byte b = (byte)weapE.Value.aIndex;
            GameObject p = Instantiate(r.GetProjectile, attackPoint.position, Rotation);
            Projectile pp = p.GetComponent<Projectile>();
            pp.SetUp(Attack, this);
            NetworkObject netP = p.GetComponent<NetworkObject>();
            netP.Spawn(true);
            netP.TrySetParent(transform);
        }
        else
            Debug.Log("cannot do a ranged attack with " + weapE.Value.ToString());
    }
    
    // RPSs
    [Rpc(SendTo.Server)] public void SetEquipmentRpc(string reference, Equipment.Slot slot = Equipment.Slot.NoPreference)
    {
        /*if (slot == Equipment.Slot.NoPreference && reference != "")
        {
            Equipment i = Equipment.GetItem(reference)
            slot = i.slot;
        }*/
        equipment[(int)slot] = reference;
        Debug.Log($"Equiped {Equipment.GetItem(reference).name} on slot {(int)slot}={slot} with Weapon {Weapon.GetItem(reference)}");
    }
    [Rpc(SendTo.Server)] protected virtual void AttackRpc()
    {
        //Debug.Log("SERVER RPC attack !");
        if      (Attack.MeleeAttack(Attack.type))
        {
            foreach(EntityStats et in MeleeAttack())
                if (et.IsAlive.Value && et.TakeDamage(Attack.damage))    // pravdive ak target zomrie
                    KilledEnemy(et);
        }
        else if (Attack.RangedAttack(Attack.type))
        {
            RangedAttack();
        }
        else 
            Debug.Log($"Player {name} attack type {Enum.GetName(typeof(Attack.Type), Attack.type)} not yet defined");
    }
    [Rpc(SendTo.Server)] public virtual void PickedUpRpc(string reference)
    {
        Equipment e = Equipment.GetItem(reference);
        equipment[(int)e.slot] = e.GetReferency;
    }
    public enum AITarget { None, Player, Team_1, Team_2, Team_3, Boss }
}