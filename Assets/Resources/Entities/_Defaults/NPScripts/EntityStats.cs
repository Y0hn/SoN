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
                        protected   NetworkVariable<int> maxHp = new();
                        protected   NetworkList<FixedString64Bytes> equipment = new(/*null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner*/);    
                        // je to list ale sprava sa ako Dictionary
                        protected   NetworkVariable<int> hp = new();
                        public      NetworkVariable<float> speed = new();
                        public      NetworkVariable<byte> level = new(1);
                        public      NetworkVariable<bool> IsAlive = new(true);
                        protected   NetworkVariable<Attack> weaponAttack = new();
                        protected   NetworkVariable<WeaponIndex> weapE = new(new(-1), NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
#pragma warning disable IDE0004
    /// <summary>
    /// Vrati pomer hp/maxHp
    /// </summary>
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
    /// <summary>
    /// Zavolane pri spawne u vsetkych
    /// </summary>
    public override void OnNetworkSpawn()
    {
        EntitySetUp();
        SubsOnNetValChanged();
        OwnerSubsOnNetValChanged(); 

        hpBar.value = hp.Value;
    }
    /// <summary>
    /// Zavolane v netSpawne, zabezpecuje synchronizaciu hodnot/vlastnosti componentov, ktore vidia vsetci
    /// </summary>
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
    /// <summary>
    /// Zabezpecuje synchronizaciu zmeny hodnoty/vlastnosti componentov, ktore su dôleźité iba pre majitela/server alebo spúšťa animacie
    /// </summary>
    protected virtual void OwnerSubsOnNetValChanged()
    {
        // Server / Owner
        weaponAttack.OnValueChanged += (Attack old, Attack now) =>
        {
            Animator.SetFloat("weapon", (float)now.type);

            float speed = now.IsMelee ? MELEE_ANIMATION_DUR : RANGED_ANIMATION_DUR;
            speed /= now.AttackTime;
            Animator.SetFloat("atSpeed", speed);
            Debug.Log("Attack animation set to time " + speed);
        };
        speed.OnValueChanged += (float old, float now) =>
        {
            Animator.SetFloat("wSpeed", now/100f);
        };
        IsAlive.OnValueChanged += (bool old, bool now) =>
        {            
            Animator.SetBool("isAlive", now);
        };
    }
	/// <summary>
	/// Nataví zakladné vlastnosti entity
	/// </summary>
    protected virtual void EntitySetUp()
    {
        name = name.Split('(')[0].Trim();
        if (IsServer)
        {
            // Nastavenie zakladneho utoku
            weaponAttack.Value = new (rase.attack);

            // Nastavenie zivotov
            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            // Nastavenie zaciatocneho levelu
            level.Value = rase.level;

            // Nastavenie rychlosti chodze
            speed.Value = rase.speed;
            Animator.SetFloat("wSpeed", rase.speed/(100f*transform.localScale.x));
            
            // Nastavenie 
            int length = Enum.GetNames(typeof(Equipment.Slot)).Length;
            for (; equipment.Count < length;)
                equipment.Add("");

            // Nastavenie obrany
            defence = new(rase.resists);
            IsAlive.Value = true;
        }
        attackPoint.localPosition = new(attackPoint.localPosition.x, weaponAttack.Value.range);
    }
    /// <summary>
    /// Spúšta sa kazdy frame a stará sa o despawn po smrti
    /// </summary>
    protected virtual void Update()
    {
        if (IsServer && timeToDespawn != 0 && timeToDespawn < Time.time)
            netObject.Despawn();
    }
    /// <summary>
    /// Spúšta sa po každej zmene životov, 
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="now"></param>
    protected virtual void OnHpUpdate(int prev, int now)
    {
        float value = HP;
        hpBar.value = value;
    } 
    /// <summary>
    /// Spúšťa sa po každej zmene zbrane
    /// </summary>
    /// <param name="changeEvent"></param>
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
    /// <summary>
    /// Uberie hodnotu ublíženia zo źivotov podla obrany proti konkretnemu typu
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Posle serveru prikaz na utok
    /// </summary>
    /// <returns>vrati ci sa je mozne utocit</returns>
    public virtual bool AttackTrigger()
    {
        if (atTime < Time.time)
        {
            AttackRpc();
            atTime = Time.time + weaponAttack.Value.AttackTime;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Vykoná sa po zabití entity
    /// </summary>
    /// <param name="died">koho som zabil</param>
    public virtual void KilledEnemy(EntityStats died)
    {

    }
    /// <summary>
    /// Nastaví aktualnu zbran a jej utok alebo iba zmení jej útok
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="weapon"></param>
    public virtual void SetWeaponIndex (sbyte attack, sbyte weapon= -1)
    {
        if      (weapon < 0 && 0 <= attack)
            weapE.Value = new (weapE.Value.eIndex, attack);
        else if (0 <= weapon && 0 <= attack)
            weapE.Value = new (weapon, attack);
        else if (attack < 0)
            weapE.Value = new (-1, -1);
    }
    /// <summary>
    /// Vyziada si uložené dáta 
    /// </summary>
    protected virtual void TryLoadServerData()
    {

    }

    // RPSs
    /// <summary>
    /// Pridáva/Odoberā zbrame
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="slot"></param>
    [Rpc(SendTo.Server)] public void SetEquipmentRpc(string reference, Equipment.Slot slot = Equipment.Slot.NoPreference)
    {
        equipment[(int)slot] = reference;
        Debug.Log($"Equiped {Equipment.GetItem(reference).name} on slot {(int)slot}={slot} with Weapon {Weapon.GetItem(reference)}");
    }
    /// <summary>
    /// Utocí za entitu podla typu útoku
    /// </summary>
    [Rpc(SendTo.Server)] protected virtual void AttackRpc()
    {
        foreach(EntityStats et in Attack.Trigger(this))                 // ak ranged tak count = 0
            if (et.IsAlive.Value && et.TakeDamage(Attack.damage))    // pravdive ak target zomrie
                KilledEnemy(et);
    }
    /// <summary>
    /// Zbiera a equipuje zbrane
    /// </summary>
    /// <param name="reference"></param>tile.Stop();
    [Rpc(SendTo.Server)] public virtual void PickedUpRpc(string reference)
    {
        Equipment e = Equipment.GetItem(reference);
        equipment[(int)e.slot] = e.GetReferency;
    }
    public enum AITarget { None, Player, Team_1, Team_2, Team_3, Boss }
}