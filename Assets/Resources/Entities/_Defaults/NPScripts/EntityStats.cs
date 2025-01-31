using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Random = UnityEngine.Random;
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
    [SerializeField] protected AudioSource aAS;
    [SerializeField] protected AITarget aiTeam = AITarget.Team_2;
    [SerializeField] protected EntityController controller;

    protected   NetworkVariable<int> maxHp = new();    
    protected   NetworkVariable<int> hp = new();
    public      NetworkVariable<float> speed = new();
    public      NetworkVariable<byte> level = new(1);
    public      NetworkVariable<bool> IsAlive = new(true);
    protected   NetworkVariable<WeaponIndex> weapE = new(new(0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

#pragma warning disable IDE0004
    /// <summary>
    /// Vrati pomer hp/maxHp
    /// </summary>
    public float HP                     { get => (float)hp.Value/(float)maxHp.Value; }
#pragma warning restore IDE0004
    public virtual Quaternion Rotation  { get => transform.rotation; }
    protected virtual Weapon[] Weapons  { get => rase.weapons; }
    public Weapon EquipedWeapon         { get => Weapons[weapE.Value.eIndex];}
    public NetworkObject NetObject      { get => netObject; }
    public Rigidbody2D RigidBody2D      { get => rb; }
    public Transform AttackPoint        { get => attackPoint; }
    public virtual Attack Attack        { get => GetAttackByWeaponIndex(weapE.Value);  } 
    public AITarget TargetTeam          { get => aiTeam; }
    public Animator Animator            { get => animator.Animator; }
    public Vector2 View                 { get => controller.View; }
    public Color Color                  { get => color.Color; }
    public float ViewAngle              { get => Mathf.Atan2(View.x, View.y); }
    public bool AttackBoth              { get => Attack.bothHanded; }
    public virtual Defence Defence      
    { 
        get 
        { 
            if (IsServer) 
                return defence; 
            else
            {
                Debug.LogWarning("Defence requested from non Server");
                return new();
            }
        } 
        set => defence = value;
    }

    public Action OnDeath;
    protected Defence defence;  // iba na servery/hoste
    protected float timeToDespawn = 0f;
    protected float atTime = 0;
    protected bool onDeathWait;
    

    public const float 
        RANGED_ANIMATION_DUR = 1.5f, 
        MELEE_ANIMATION_DUR  = 1,
        TIME_TO_DESPAWN      = 5;


    /// <summary>
    /// Zavolane pri spawne u vsetkych
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Najprv nastavi zakladne hodnoty
        EntitySetUp();

        // Dalej nastavi odber pre zmenu na Zdielanych premennych
        ServerSubsOnNetValChanged();
        OwnerSubsOnNetValChanged(); 
        SubsOnNetValChanged();
    }
    /// <summary>
    /// Zabezpecuje spravne nastavenie a chod 
    /// </summary>
    protected virtual void ServerSubsOnNetValChanged()
    {
        if (!IsServer) return;
        weapE.OnValueChanged += (WeaponIndex old, WeaponIndex now) =>
        {
            Attack a = GetAttackByWeaponIndex(now);
            if      (Attack.MeleeAttack(a.type))
            {
                attackPoint.localPosition = new(attackPoint.localPosition.x, Attack.range);
            }
            else if (Attack.RangedAttack(a.type))
            {
                Ranged r = (Ranged)Weapons[now.eIndex];
                attackPoint.localPosition = new(r.projSpawnPosition.x, r.projSpawnPosition.y);
                //Debug.Log($"{name}'s attack point position set to [{attackPoint.localPosition.x},{attackPoint.localPosition.y}]");
            }
        };
        maxHp.OnValueChanged += (int prevValue, int newValue) => 
        {
            hp.Value = maxHp.Value;
        };
        IsAlive.OnValueChanged += (bool old, bool now) =>
        {            
            if (now)
                hp.Value = maxHp.Value;
            else
                Die();
        };
    }
    /// <summary>
    /// Zavolane v netSpawne, zabezpecuje synchronizaciu hodnot/vlastnosti componentov, ktore vidia vsetci
    /// </summary>
    protected virtual void SubsOnNetValChanged()
    {
        weapE.OnValueChanged   += (WeaponIndex old, WeaponIndex now) => 
        {
            bool 
                R = false, 
                L = false, 
                B = false;
            Attack a = GetAttackByWeaponIndex(now);

            if (a.type != Attack.Type.RaseUnnarmed)
            {
                Weapon w = Weapons[now.eIndex];
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
            hpBar.gameObject.SetActive(alive);
            coll.enabled = alive;
        };
        hp.OnValueChanged += OnHpUpdate;
        hpBar.value = hp.Value;
    }
    /// <summary>
    /// Zabezpecuje synchronizaciu zmeny hodnoty/vlastnosti componentov, ktore su dôleźité iba pre majitela/server alebo spúšťa animacie
    /// </summary>
    protected virtual void OwnerSubsOnNetValChanged()
    {
        // Server / Owner
        weapE.OnValueChanged += (WeaponIndex old, WeaponIndex now) =>
        {
            Animator.SetFloat("weapon", (float)Attack.type);

            float aSpeed = Attack.IsMelee ? MELEE_ANIMATION_DUR : RANGED_ANIMATION_DUR;
            aSpeed /= Attack.AttackTime;
            Animator.SetFloat("atSpeed", aSpeed);
            //Debug.Log($"Attack animation set on weapon {Animator.GetFloat("weapon")} to speed {speed}");
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
            // Nastavenie zivotov
            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            // Nastavenie zaciatocneho levelu
            level.Value = rase.level;

            // Nastavenie rychlosti chodze
            speed.Value = rase.speed;
            Animator.SetFloat("wSpeed", rase.speed/(100f*transform.localScale.x));
            
            // Nastavenie obrany
            Defence = new(rase.resists);
            IsAlive.Value = true;

            // Nastavenie zakladneho utoku
            weapE.Value = new (0);
            Animator.SetFloat("weapon", (float)Weapons[0].attack[0].type);

            // Oneskorenie zmiznutia po smrti
            onDeathWait = true;
        }
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
        if (now <= 0)
            Die();
    }
    /// <summary>
    /// Zavolana ak zivoty chraktera dosiahnu 0
    /// </summary>
    protected virtual void Die()
    {
        if (onDeathWait)
            timeToDespawn = Time.time + TIME_TO_DESPAWN;
        foreach (Delegate d in OnDeath?.GetInvocationList())
            if (d.Target == null)  // Ak je objekt odstránený, odpojíme metódu
                OnDeath -= (Action)d;
        foreach (var s in GetComponentsInChildren<SpriteRenderer>())
            s.sortingLayerName = "DeadEntity";
        
        OnDeath?.Invoke();
    }
    /// <summary>
    /// Uberie hodnotu ublíženia zo źivotov podla obrany proti konkretnemu typu
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>PRAVDA ak nepriatel zomrel</returns>
    public virtual bool TakeDamage(Damage damage)
    {
        if (!IsServer) 
            return false;

        int newDamage = Defence.CalculateDMG(damage);
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
    /// <returns>PRAVDA ak moze utocit</returns>
    public virtual bool AttackTrigger()
    {
        if (atTime < Time.time)
        {
            AttackRpc();
            atTime = Time.time + Attack.AttackTime;
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
    /// Nastaví aktualnu zbran a jej utok
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="weapon"></param>
    public virtual void SetWeaponIndex (sbyte attack, sbyte weapon)
    {
        WeaponIndex wi = weapE.Value;

        weapE.Value = new (weapon, attack);

        weapE.OnValueChanged.Invoke(wi, weapE.Value);
        //Debug.Log($"Setted Weapon Index= {weapE.Value}");
    }
    public virtual void SerWeaponIndex (WeaponIndex WeI)
    {
        SetWeaponIndex(WeI.aIndex, WeI.eIndex);
    }

    /// <summary>
    /// Vyziada si uložené dáta 
    /// </summary>
    protected virtual void TryLoadServerData()
    {

    }
    protected virtual Attack GetAttackByWeaponIndex(WeaponIndex wIndex)
    {
        string debug = $"GetAttackByWeaponIndex({wIndex})\nReturning:";
        try {
            debug += $"\nFrom Weapons: {Weapons.Length}";
            debug += $"\nFrom Attacks: {Weapons[wIndex.eIndex].attack.Count}";
        } finally {
            if (this is PlayerStats)
                Debug.Log(debug);
        }
        return Weapons[wIndex.eIndex].attack[wIndex.aIndex];
    }
    
    /*   _____  _____   _____     
     *  |  __ \|  __ \ / ____|    
     *  | |__) | |__) | |     ___ 
     *  |  _  /|  ___/| |    / __|
     *  | | \ \| |    | |____\__ \
     *  |_|  \_\_|     \_____|___/
     */
    /// <summary>
    /// Vyhodi item, tak ze vytvori objekt na nahodnych suradniciach v dosahu
    /// a prida mu ho aku atribut
    /// </summary>
    /// <param name="itemPath"></param>
    /// <param name="dropRange"></param>
    [Rpc(SendTo.Server)] public void DropRpc(string itemPath, Vector2 dropRange)
    {
        Vector2 pos = new (
            transform.position.x + 
                Random.Range(-dropRange.x, dropRange.x), 
            transform.position.y + 
                Random.Range(-dropRange.y, dropRange.y));

        GameObject i = Instantiate(Resources.Load<GameObject>("Items/ItemDrop"), pos, Quaternion.identity);
        i.GetComponent<ItemDrop>().Item = Item.GetItem(itemPath);
        i.GetComponent<NetworkObject>().Spawn();
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
    /// Meni rychlost charakteru, pri zmene terenu
    /// </summary>
    /// <param name="speedMod"></param>
    [Rpc(SendTo.Server)] public virtual void TerrainChangeRpc(float speedMod)
    {
        try {
            speed.Value *= speedMod;
        } catch {

        }
    }
    /// <summary>
    /// Urcuje skupinu pre cielenie a ublizovanie si navzajom ;)
    /// (skupiny sa navzajom nemaju radi a None je  neutralna)
    /// </summary>
    public enum AITarget { None, Player, Team_1, Team_2, Team_3, Boss }
}