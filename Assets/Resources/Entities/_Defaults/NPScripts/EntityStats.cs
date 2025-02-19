using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using AYellowpaper.SerializedCollections;
using Random = UnityEngine.Random;
using NUnit.Framework.Internal;
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
    [SerializeField] protected AudioSource audioSource;

    [HideInInspector] public NetworkVariable<float> speed = new(100);
    [HideInInspector] public NetworkVariable<bool> IsAlive = new(true);
    protected   NetworkVariable<int> maxHp = new(100);    
    protected   NetworkVariable<int> hp = new(100);
    protected NetworkVariable<byte> level = new(1);    
    protected   NetworkVariable<WeaponIndex> weapE = new(new(0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<bool> onPath = new(false);

#region Ukazovace
    /// <summary>
    /// Vrati pomer hp/maxHp
    /// </summary>
    public float HP                     => (float)hp.Value/(float)maxHp.Value;
    public virtual Quaternion Rotation  => transform.rotation;
    public WeaponIndex WeaponPrameter   => weapE.Value; 
    protected virtual Weapon[] Weapons  => rase.weapons;
    public Weapon EquipedWeapon         => Weapons[weapE.Value.eIndex];
    public NetworkObject NetObject      => netObject;
    public Rigidbody2D RigidBody2D      => rb;
    public Transform AttackPoint        => attackPoint;
    public virtual Attack Attack        => GetAttackByWeaponIndex(weapE.Value);
    public AITarget TargetTeam          => aiTeam;
    public Animator Animator            => animator.Animator;
    public Sound AtackSound             => Weapons[weapE.Value.eIndex].clips[weapE.Value.aIndex];
    public Vector2 View                 => controller.View;
    public Color Color                  => color.Color;
    public float ViewAngle              => Mathf.Atan2(View.x, View.y);
    public bool AttackBoth              => Attack.bothHanded;
    public byte Level                    => level.Value;
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
    /// <summary>
    /// Akcia sa zavola pri smrti
    /// </summary>
    public Action OnDeath;
    protected Defence defence;  // iba na servery/hoste
    protected float timeToDespawn = 0f;
    protected bool onDeathWait;
    /// <summary>
    /// Zisti utok z dostupnych zbrani podla 
    /// </summary>
    /// <param name="wIndex">Index zbrane</param>
    /// <returns>UTOK zo zbrane</returns>
    protected virtual Attack GetAttackByWeaponIndex(WeaponIndex wIndex) => Weapons[wIndex.eIndex].attack[wIndex.aIndex];
    

    public const float 
        RANGED_ANIMATION_DUR = 1.5f, 
        MELEE_ANIMATION_DUR  = 1,
        TIME_TO_DESPAWN      = 5;
#endregion
#region SetUp
    /// <summary>
    /// Zavolane pri vzniku objektu v sieti
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Najprv nastavi zakladne hodnoty
        EntitySetUp();

        // Dalej nastavi odber pre zmenu na Zdielanych premennych
        ServerSubsOnNetValChanged();
        OwnerSubsOnNetValChanged(); 
        SubsOnNetValChanged();

        weapE.OnValueChanged.Invoke( weapE.Value, weapE.Value);
    }
    /// <summary>
    /// Zavolane pri zaniku abjektu v sieti
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        //FileManager.Log($"{name} despawned");
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
            Animator.SetFloat("weapon", (float)Weapons[0].attack[0].damage.type);

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
#endregion
#region NetValueSubscribe
    /// <summary>
    /// Zabezpecuje spravne nastavenie a chod 
    /// </summary>
    protected virtual void ServerSubsOnNetValChanged()
    {
        if (!IsServer) return;
        weapE.OnValueChanged += (old, now) =>
        {
            Attack a = GetAttackByWeaponIndex(now);
            if      (a.IsMelee)
            {
                attackPoint.localPosition = new(attackPoint.localPosition.x, Attack.range);
            }
            else if (a.IsRanged)
            {
                Ranged r = (Ranged)Weapons[now.eIndex];
                attackPoint.localPosition = new(r.projSpawnPosition.x, r.projSpawnPosition.y);
                //Debug.Log($"{name}'s attack point position set to [{attackPoint.localPosition.x},{attackPoint.localPosition.y}]");
            }
        };
        maxHp.OnValueChanged += (prevValue, newValue) => 
        {
            hp.Value = maxHp.Value;
        };
        IsAlive.OnValueChanged += (old, now) =>
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
        weapE.OnValueChanged   += (old, now) => 
        {
            bool 
                R = false, 
                L = false, 
                B = false;
            Attack a = GetAttackByWeaponIndex(now);

            if (a.damage.type != Damage.Type.FIST)
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
            
            Animator.SetFloat("weapon", (float)Attack.damage.type);

            float aSpeed = Attack.IsMelee ? MELEE_ANIMATION_DUR : RANGED_ANIMATION_DUR;
            aSpeed /= Attack.AttackTime;
            Animator.SetFloat("atSpeed", aSpeed);
            
            //FileManager.Log($"Attack animation set on weapon {Animator.GetFloat("weapon")} to speed {speed}");

            weaponR.gameObject.SetActive(R || B); 
            weaponL.gameObject.SetActive(L || B);
            if (IsOwner)
            {
                float atBlend = (R || B) ? 1 : -1;
                Animator.SetFloat("atBlend", atBlend);
            }
        };
        IsAlive.OnValueChanged  += (prev, alive) => 
        {
            if (this is not PlayerStats)
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
        speed.OnValueChanged += (old, now) =>
        {
            Animator.SetFloat("wSpeed", now/100f);
        };
        IsAlive.OnValueChanged += (old, now) =>
        {            
            Animator.SetBool("isAlive", now);
        };
    }
#endregion
#region Events
    /// <summary>
    /// Spúšta sa po každej zmene životov, 
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="now"></param>
    protected virtual void OnHpUpdate(int prev, int now)
    {
        float value = HP;
        hpBar.value = value;
        if (prev > now)
            PlaySound("onHitted");
        if (now <= 0 && IsServer)
            IsAlive.Value = false;
    }
    /// <summary>
    /// Zavolana ak zivoty chraktera dosiahnu 0
    /// </summary>
    protected virtual void Die()
    {
        PlaySound("onDeath");

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

        //weapE.OnValueChanged.Invoke(wi, weapE.Value);
        //Debug.Log($"Setted Weapon Index= {weapE.Value}");
    }
    public virtual void SetWeaponIndex (WeaponIndex WeI)
    {
        SetWeaponIndex(WeI.aIndex, WeI.eIndex);
    }
#endregion
#region LoadSavedData
    /// <summary>
    /// Vyziada si uložené dáta 
    /// </summary>
    protected virtual void LoadSavedData(World.EntitySave save)
    {
        hp.Value = Mathf.RoundToInt(save.hp * (float)maxHp.Value);
        transform.position = save.Position;
        SetWeaponIndex(save.weapon);
    }
#endregion
#region Sounds
    /// <summary>
    /// Zahra zvuk len na jednom positaci
    /// </summary>
    /// <param name="soundType"></param>
    /// <param name="index"></param>
    protected void PlaySound(string soundType, int index = -1)
    {
        if (index < 0)
            index = Random.Range(1, 4);

        if (soundType == "step")
            soundType = (onPath.Value ? "stone" :"grass" ) + "Step";

        // Ziska kaudio podla nazvu a cisla
        string i = (soundType != "onDeath") ? index.ToString() : "";
        
        // Prehra ho zvuk
        rase.sounds[soundType + i].Play(ref audioSource);
    }
#endregion
#region RPCs
    /// <summary>
    /// Vyhodi item, tak ze vytvori objekt na nahodnych suradniciach v dosahu
    /// a prida mu ho aku atribut
    /// </summary>
    /// <param name="itemPath"></param>
    /// <param name="dropRange"></param>
    [Rpc(SendTo.Server)] public void DropRpc(string itemPath, Vector2 dropRange, Vector2 minRange)
    {
        // nahodna pozicia v medziach
        Vector2 pos = new (
                Random.Range(minRange.x, dropRange.x), 
                Random.Range(minRange.y, dropRange.y));

        // smer
        pos.x *= Random.Range(0, 2) < 1 ? -1 : 1;
        pos.y *= Random.Range(0, 2) < 1 ? -1 : 1;

        // okolie
        pos.x += transform.position.x;
        pos.y += transform.position.y;

        GameObject i = Instantiate(Resources.Load<GameObject>("Items/ItemDrop"), pos, Quaternion.identity);
        i.GetComponent<ItemDrop>().Item = Item.GetItem(itemPath);
        i.GetComponent<NetworkObject>().Spawn();

        FileManager.Log($"Item {itemPath} droped");
    }
    /// <summary>
    /// Utocí za entitu podla typu útoku
    /// </summary>
    [Rpc(SendTo.Server)] public virtual void AttackRpc()
    {
        foreach(EntityStats et in Attack.Trigger(this))                 // ak ranged tak count = 0
            if (et.IsAlive.Value && et.TakeDamage(Attack.damage))    // pravdive ak target zomrie
                KilledEnemy(et);
    }
    /// <summary>
    /// Meni rychlost charakteru, pri zmene terenu
    /// </summary>
    /// <param name="speedMod"></param>
    [Rpc(SendTo.Server)] public virtual void TerrainChangeRpc(float speedMod, bool path = false)
    {
        try {
            if (path)
                onPath.Value = 1 < speedMod;

            speed.Value *= speedMod;
        } catch {
            //FileManager.Log("Error in speed change");
        }
    }
    /// <summary>
    /// Vyda zvuk pre vsetkych, budu ho vsak pocut len ti v blizkosti
    /// </summary>
    /// <param name="name">KLUCova hodnota zdroja zvuku</param>
    [Rpc(SendTo.Everyone)] public virtual void PlaySoundRpc (string soundType, int index = -1) => PlaySound(soundType, index);
    /// <summary>
    /// Prehra klip pre vsetkych, podla jeho cesty
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="vol"></param>
    [Rpc(SendTo.Everyone)] public virtual void PlaySoundRpc (string clipPath, float vol)
    {
        AudioClip ac = Resources.Load<AudioClip>(clipPath);
        audioSource.PlayOneShot(ac, vol);
    }
#endregion
    /// <summary>
    /// Urcuje skupinu pre cielenie a ublizovanie si navzajom ;)
    /// (skupiny sa navzajom nemaju radi a None je  neutralna)
    /// </summary>
    public enum AITarget { None, Player, Team_1, Team_2, Team_3, Boss }
}