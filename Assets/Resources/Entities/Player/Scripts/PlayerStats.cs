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
     * [SF] protected TMP_Text nameTag;
     * [SF] protected Rase rase;
     * [SF] protected Slider hpBar;
     * [SF] protected GameObject body;
     * [SF] protected NetworkObject netObject;
     * [SF] protected Transform attackPoint;
     * [SF] protected ColorChainReference color;
     * [SF] protected SpriteRenderer weaponR, weaponL;
     * [SF] protected NetworkAnimator animator;
     * [SF] protected Rigidbody2D rb;
     * [SF] protected Collider2D coll;
     * [SF] protected AITarget aiTeam = AITarget.Team_2;
     * [SF] protected EntityController controller;
     * [SF, SD("Nazov","Audios")] protected SerializedDictionary<string, AudioSource> audioSources;
     *
     * [HII] public NetworkVariable<float> speed = new(100);
     * [HII] public NetworkVariable<byte> level = new(1);
     * [HII] public NetworkVariable<bool> IsAlive = new(true);
     * protected   NetworkVariable<int> hp = new(100);
     * protected   NetworkVariable<int> maxHp = new(100);    
     * protected   NetworkVariable<byte> stepIndex = new(0);
     * protected   NetworkVariable<WeaponIndex> weapE = new(new(0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
     *
     * public float HP                     => (float)hp.Value/(float)maxHp.Value;    
     * public virtual Quaternion Rotation  => transform.rotation;
     * protected virtual Weapon[] Weapons  => rase.weapons;
     * public Weapon EquipedWeapon         => Weapons[weapE.Value.eIndex];
     * public NetworkObject NetObject      => netObject;
     * public Rigidbody2D RigidBody2D      => rb;
     * public Transform AttackPoint        => attackPoint;
     * public virtual Attack Attack        => GetAttackByWeaponIndex(weapE.Value);  
     * public AITarget TargetTeam          => aiTeam;
     * public Animator Animator            => animator.Animator;
     * public Vector2 View                 => controller.View;
     * public Color Color                  => color.Color;
     * public float ViewAngle              => Mathf.Atan2(View.x, View.y);
     * public bool AttackBoth              => Attack.bothHanded;
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
    public World.PlayerSave.SkillTreeSave SkillTreeSave => IsServer ? skillTree.SkillTreeSave : null;
    public World.PlayerSave.InventorySave InventorySave 
    {
        get
        {
            string[] inv = new string[inventory.Count];
            string[] eq = new string[equipment.Count];
            for (int i = 0; i < inventory.Count; i++)
                inv[i] = inventory[i].ToString();
            for (int i = 0; i < equipment.Count; i++)
                eq[i] = equipment[i].ToString();

            return new (inv, eq);
        }
    }
    public override Attack Attack => IsServer && skillTree != null ? skillTree.ModAttack(base.Attack) : base.Attack;    
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ServerRequestData();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Update()
    {
        if (chatTimer != 0 && chatTimer <= Time.time)
        {
            chatField.SetActive(false);
            chatBox.text = "";
            chatTimer = 0;
        }
    }
    /// <summary>
    /// Server sa pokusi nacitat data o hracovi z ulozenia sveta <br />
    /// Ak sa podari, zavola metodu pre nacitanie ziskanych udajov. <br />
    /// Ak nie nastavi poziciu v okruhu miesta zrodenia.
    /// </summary>
    protected void ServerRequestData()
    {
        if (!IsServer) return;
        
        if (FileManager.World.TryGetPlayerSave(name, out var saved))
            LoadSavedData(saved);
        else 
            // ak data o hracovi nenajde nastavi jeho poziciu v okruhu zaciatocneho bodu
            MapScript.map.SpawnPlayer(transform);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void LoadSavedData(World.EntitySave save)
    {
        if (!IsServer) return;
        var pSave = (World.PlayerSave)save;

        // Nacitaj data inventara
        foreach (var item in pSave.inventory.items)
            inventory.Add(item);
        // Nacita data o pouzitych predmetoch
        foreach (var eq in pSave.inventory.equiped)
            equipment.Add(eq);


        foreach (var skill in pSave.skillTree.skills)
            AddSkill(skill);
        /*foreach (var uSill in pSave.skillTree.usingUtils)
            skillTree.*/

        base.LoadSavedData(pSave);
    }

    /// <summary>
    /// Pri odpojeni hraca ulozi jeho data na server
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        // Ulozi hodnoty iba na servery
        FileManager.SaveClientData(new (this));
    }
    /// <summary>
    /// Nastavi zacinajuce hodnoty pre charakter
    /// </summary>
    protected override void EntitySetUp()
    {
        base.EntitySetUp();
        if (IsServer)
        {
            skillTree = new(this);
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

            playerName.Value = Menu.menu.PlayerName;
        }
        chatTimer = 0;
        chatBox.text = "";
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
        onDeathWait = false;
        GetComponent<NetworkObject>().name = nameTag.text;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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
    /// <summary>
    /// Nastavi odber zmenenych sietovych premennych
    /// </summary>
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
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Die()
    {
        OnDeath?.Invoke();
        OnDeath = null;
    }
    /// <summary>
    /// Volane lokalne pre klienta ak sa zmeni inventar
    /// </summary>
    /// <param name="changeEvent"></param>
    protected void OnInventoryUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)
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
    /// <summary>
    /// zmena utkou podla ID rychlej volby utku
    /// </summary>
    /// <param name="id"></param>
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
        //Debug.Log($"Setting ID={id} to weapon index to new(att= {att} | wea= {wea})\nWeapons count: {Weapons.Length}");
        SetWeaponIndex(att, wea);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public override bool TakeDamage(Damage damage)
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
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override bool AttackTrigger()
    {
        return base.AttackTrigger();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="died"><inheritdoc/></param>
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        xp.Value += (uint)(died.level.Value * 100);
    }
    /// <summary>
    /// Pokusi sa prerusit utok
    /// </summary>
    /// <returns>PRAVDA ak bol utok preruseny</returns>
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

    /*   _____  _____   _____     
     *  |  __ \|  __ \ / ____|    
     *  | |__) | |__) | |     ___ 
     *  |  _  /|  ___/| |    / __|
     *  | | \ \| |    | |____\__ \
     *  |_|  \_\_|     \_____|___/
     *  *  *  *  *  *  *  *  *  *  */
    /// <summary>
    /// Prida schopnost do stromu schopnosti
    /// </summary>
    /// <param name="skill"></param>
    public void AddSkill(Skill skill)
    {
        BoughtSkillRpc(skill.name);

        if      (skill is ModDamage mD)
            AddSkillRpc(mD);
        else if (skill is ModSkill mS)
            AddSkillRpc(mS);
        else if (skill is Utility ut)
            AddSkillRpc(ut);
    }
    [Rpc(SendTo.Owner)]  protected void BoughtSkillRpc (string name)     { game.SkillTree.BuySkill(name); }
    [Rpc(SendTo.Server)] protected void AddSkillRpc (Utility skill)      { skillTree.Add(skill); }
    [Rpc(SendTo.Server)] protected void AddSkillRpc (ModSkill skill)     { skillTree.Add(skill); }
    [Rpc(SendTo.Server)] protected void AddSkillRpc (ModDamage skill)    { skillTree.Add(skill); }
    
    /// <summary>
    /// Odomkne funkciu "Utiliti" schopnosti lokalne na klientovy 
    /// </summary>
    /// <param name="skill"></param>
    [Rpc(SendTo.Owner)] public void UnlockUtilityRpc (Utility skill)
    {
        game.AddUtility(skill);
    }
    /// <summary>
    /// Prida maximalne zivoty
    /// </summary>
    /// <param name="addHealth"></param>
    [Rpc(SendTo.Server)] public virtual void AddMaxHealthRpc (float addHealth)
    {
        if (addHealth % 100 == 0)
            maxHp.Value += (int)addHealth;
        else
            maxHp.Value = Mathf.RoundToInt((float)maxHp.Value * (float)(1+addHealth));
    }
    /// <summary>
    /// Zastavi aktualne prebiehajuci strelny utok znicenim projektilu
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
    /// <summary>
    /// prida level hracovi
    /// </summary>
    [Rpc(SendTo.Server)] public void AddLvlRpc()
    {
        //Debug.Log("XP added to player");
        xp.Value = xpMax.Value;
    }
}