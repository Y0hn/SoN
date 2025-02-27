using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class PlayerStats : EntityStats
{
    [SerializeField] GameObject chatField;
    [SerializeField] Transform canvas;
    [SerializeField] TMP_Text chatBox;
    
    public const uint NEEDED_XP_TO_NEXT_LEVEL = 100;
    protected const float REGAIN_HP_ON_LEVEL_UP = 0.3f;

    protected float chatTimer; const float chatTime = 5.0f;
    protected XpSliderScript xpBar;       // UI nastavene len pre Ownera
    protected GameManager game;
    protected Connector conn;
    protected Inventory inventUI;

    public NetworkVariable<Experience> xp = new(new (0,0,50));
    protected NetworkList<FixedString64Bytes> inventory = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkList<FixedString64Bytes> equipment = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // je to list ale sprava sa ako Dictionary
    protected NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString128Bytes> message = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    protected SkillTree skillTree;  // iba na servery
    public int MaxHP => maxHp.Value;

    public Projectile Projectile { get; set; }
    protected Equipment[] Equipments     
    { 
        get 
        {
            List<Equipment> eq = new();
            foreach (var e in equipment)
                eq.Add(Equipment.GetItem(e.ToString()));
            
            string log = "";
            foreach (var e in equipment)
                log += $"[{e.ToString()}]\n";

            FileManager.Log($"Additional weapons {eq.Count}\n{log}");
            return eq.ToArray();
        }
    }
    protected override Weapon[] Weapons 
    { 
        get 
        { 
            var w = base.Weapons.ToList();
            w.AddRange(Equipments); 
            FileManager.Log($"Returning weapons [{w.Count}]");
            return w.ToArray();
        } 
    }
    public World.PlayerSave.SkillTreeSave SkillTreeSave => IsServer ? skillTree.SkillTreeSave : null;
    public World.PlayerSave.InventorySave InventorySave 
    {
        get
        {
            string[] inv = new string[inventory.Count];
            string[] eq = new string[2];
            for (int i = 0; i < inventory.Count; i++)
                inv[i] = inventory[i].ToString();
            for (int i = 0; i < equipment.Count; i++)
                eq[i] = equipment[i].ToString();

            return new (inv, eq);
        }
    }
    public override Attack Attack => IsServer && skillTree != null ? skillTree.ModAttack(base.Attack) : base.Attack;    
    public bool ImunityToCoruption => skillTree.hasImunityToCoruption;
    public bool Corruped { get; set; }

#region SetUp
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
    /// Pri odpojeni hraca ulozi jeho data na server
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        // Ulozi hodnoty iba na servery
        FileManager.SaveClientData(new (this));
    }
    /// <summary>
    /// Nastavi zacinajuce hodnoty pre charakter hraca na vsetkych klientoch, <br /> 
    /// ale niktrore len pre vlasnika alebo len kilenta
    /// </summary>
    protected override void EntitySetUp()
    {
        base.EntitySetUp();
        if (IsServer)
        {
            skillTree = new(this);
            xp.Value = new(0,0,50);
            level.Value = 0;
        }
        if (IsOwner)
        {
            conn = Connector.instance;
            game = GameManager.instance;

            inventUI = GameManager.instance.inventory;
            resists = GameManager.instance.LocalDefence;

            hpBar.gameObject.SetActive(false);
            hpBar = game.HpBar;
            xpBar = game.XpBar;

            // zursi si valstne povolenia pre canvas vo svete
            UtilitySkillScript[] ussr = canvas.GetComponents<UtilitySkillScript>();
            foreach (var us in ussr)
                Destroy(us);

            // Nastavenie 
            for (; equipment.Count < 2;)
                equipment.Add("");

            playerName.Value = Menu.menu.PlayerName;
        }
        chatTimer = 0;
        chatBox.text = "";
        chatField.SetActive(false);
        name = playerName.Value.ToString();
        nameTag.text = name;
        
        // Povoli pole mena len pre nevlasniaceho hraca
        nameTag.gameObject.SetActive(!IsOwner);

        onDeathWait = false;
        GetComponent<NetworkObject>().name = nameTag.text;
    }
#endregion
#region ZmenyHodnot
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void SubsOnNetValChanged()
    {
        base.SubsOnNetValChanged();

        // Len pre server
        if (IsServer)
        {
            xp.OnValueChanged += (old, now) => 
            {
                if (now.LevelUP)
                {
                    level.Value+= now.gainedLevel;
                }
            };
            level.OnValueChanged += (old, now) => 
            {
                // pre pripad viacnasobneho levelupu
                for (int i = old; i < now; i++)
                {
                    // pripocita zivoty
                    float addingHP = maxHp.Value * REGAIN_HP_ON_LEVEL_UP;
                    addingHP += hp.Value;
                    addingHP = Mathf.Clamp(addingHP, hp.Value, maxHp.Value);
                    hp.Value = Mathf.RoundToInt(addingHP);
                }
            };
        }

        // Pre vsetkych
        playerName.OnValueChanged += (old, now) => 
        { 
            nameTag.text = now.ToString(); 
        };
        message.OnValueChanged += (old, now) => 
        {
            chatBox.text = now.ToString();
            chatField.SetActive(true);
            chatTimer = Time.time + chatTime;
        };
    }
    /// <summary>
    /// Nastavi odber zmenenych sietovych premennych pre vlastnika
    /// </summary>
    protected override void OwnerSubsOnNetValChanged()
    {
        if (!IsOwner) return;

        base.OwnerSubsOnNetValChanged();

        xp.OnValueChanged += (old, now) => 
        {
            xpBar.QueueChange(now.XP, level.Value);
        };
        hp.OnValueChanged += (old, now) => 
        {
            if (now < old)
                game.AnimateFace("got-hit");
            game.AnimateFace(HP);
        };
        inventory.OnListChanged += changeEvent =>
        {
            OnInventoryUpdate(changeEvent);
        };
        IsAlive.OnValueChanged  += (old, now) => 
        {
            game.AnimateUI("isAlive", now);
            game.SetPlayerUI(now);
        };
        level.OnValueChanged += (old, now) =>
        {
            for (int i = old; i < now; i++)
                xpBar.QueueChange(xp.Value.XP, now);
            FileManager.Log($"Leveled up {old}->{now}", FileLogType.RECORD);
        };
        maxHp.OnValueChanged += (old, now) =>
        {
            game.SetMaxHp(now);
        };
    }
    /// <summary>
    /// Volane lokalne pre klienta ak sa zmeni inventar
    /// </summary>
    /// <param name="changeEvent"></param>
    protected void OnInventoryUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)
    {        
        if (changeEvent.Value != "")
            switch (changeEvent.Type)
            {
                case NetworkListEvent<FixedString64Bytes>.EventType.Add:
                    //int index = changeEvent.Index; // the position of the added value in the list
                    inventUI.Add(changeEvent.Value.ToString()); // the new value at the index position
                    break;
                case NetworkListEvent<FixedString64Bytes>.EventType.Remove:
                    inventUI.Delete(changeEvent.Value.ToString());
                    break;
                default:
                    break;
            }
    }
    public override void SetWeaponIndex(WeaponIndex WeI)
    {
        FileManager.Log($"Setting weapon index to {WeI}");
        base.SetWeaponIndex(WeI);
    }
    #endregion
    #region Udalosti
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Die()
    {
        OnDeath?.Invoke();
        OnDeath = null;
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
                
        if (hp.Value <= 0)
            IsAlive.Value = false;

        return !IsAlive.Value;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="died"><inheritdoc/></param>
    public override void KilledEnemy(EntityStats died)
    {
        base.KilledEnemy(died);
        Experience x = new (xp.Value, (uint)(died.Level * Random.Range(25f, 50f)));
        xp.Value = x;
    }
    protected override void ShowRezists()
    {
        // Vymaze obrany
        int n = resists.childCount-1;
        for (int i = n; 0 <= i; i--)
            Destroy(resists.GetChild(i).gameObject);
        // Nastavi obrany
        foreach (var d in game.defences)
        {
            try {
                Sprite s = Resources.Load<Sprite>(FileManager.GetDamageReff(d));
                Instantiate(res, resists).GetComponent<DefType>().image.sprite = s;
            } catch (Exception ex) {
                FileManager.Log($"{name} failed to set up rezists {ex.Message}", FileLogType.ERROR);
            }
        }
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
            GetComponent<PlayerController>().ResetAttack();
        }

        return canStop;
    }
#endregion
#region LoadSavedData
    /// <summary>
    /// Server sa pokusi nacitat data o hracovi z ulozenia sveta <br />
    /// Ak sa podari, zavola metodu pre nacitanie ziskanych udajov. <br />
    /// Ak nie nastavi poziciu v okruhu miesta zrodenia.
    /// </summary>
    protected void ServerRequestData()
    {
        // kontroluje ci je volaná zo servera
        if (!IsServer) return;
        
        // pokusi sa ziskat ulozene udaje hraca podla jeho mena
        if (FileManager.World.TryGetPlayerSave(name, out var saved))
        
            // Nacita ulozene data
            LoadSavedData(saved);
        
        // ak data o hracovi nenajde nastavi jeho poziciu v okruhu zaciatocneho bodu
        else 
            MapScript.map.SpawnPlayer(transform);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void LoadSavedData(World.EntitySave save)
    {
        var pSave = (World.PlayerSave)save;

        // Nacitaj data do inventara
        foreach (var item in pSave.inventory.items)
            if (item != "")
                inventory.Add(item);
        foreach (var item in pSave.inventory.equiped)
            if (item != "")
                inventory.Add(item);

        int min = pSave.level*100;
        if (0 < pSave.level)
            min+= 50;
        Experience exp = new(min, (int)pSave.xp, (pSave.level+1)*100+50);

        if (IsOwner)
        {
            game.LocalPlayer = this;
            xpBar.Load(exp.XP, pSave.level);
            game.SkillTree.LoadSkills(pSave);
            game.inventory.ReloadAttacks();
        }
        if (IsServer) 
        {
            level.Value = pSave.level;
            //maxHp.Value = pSave.level*100+50;
            //speed.Value = pSave.speed;

            xp.Value = exp;

            // inhereted
            hp.Value = Mathf.RoundToInt(save.hp * (float)pSave.maxHp);
            transform.position = save.Position;
        }
                

        FileManager.Log("Player Data loaded: " + pSave);
    }
    /// <summary>
    /// Zbiera a equipuje zbrane <br />
    /// Volane len zo servera z vypadnuteho itemu pre navratovu hodnotu
    /// </summary>
    /// <param name="reference"></param>
    public virtual bool PickedUp(string reference)
    {
        if (!(IsOwner || IsServer)) return false;

        bool free = reference != "" && !inventory.Contains(reference) && !equipment.Contains(reference);

        if (free)
            inventory.Add(reference);

        return free;
    }
#endregion
#region RPCs
    /// <summary>
    /// Prida schopnost do stromu schopnosti <br />
    /// Dvolezite rozdelenie kvoli prenosu cez RPC -cka
    /// </summary>
    /// <param name="skill"></param>
    public void AddSkill(Skill skill)
    {
        BoughtSkillRpc(skill.name);

        if      (skill is ModDamage mD)
        {
            AddSkillRpc(mD);

            if (IsOwner && !mD.damage)
            {
                game.AddResist(mD.condition);
                ShowRezists();
            }
        }
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
        maxHp.Value += (int)addHealth;
        FileManager.Log($"Player hp changed to {maxHp.Value}", FileLogType.RECORD);
    }
    /// <summary>
    /// Zastavi aktualne prebiehajuci strelny utok znicenim projektilu
    /// </summary>
    [Rpc(SendTo.Server)] protected void StopRanAttackRpc()
    {
        if (Projectile != null)
            Projectile.StopAttack();
    }
    /// <summary>
    /// Pridáva/Odoberā zbrame
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="slot"></param>
    public void SetEquipmentRpc(string reference, Equipment.Slot slot)
    {
        if (equipment[(int)slot] != "")
            inventory.Add(equipment[(int)slot]);
            
        inventory.Remove(reference);
        equipment[(int)slot] = reference;  
        //FileManager.Log($"Equiped {Equipment.GetItem(reference).name} on slot {(int)slot}={slot} with Weapon {Weapon.GetItem(reference)}");
    }
    /// <summary>
    /// prida level hracovi
    /// </summary>
    [Rpc(SendTo.Server)] public void AddLvlRpc()
    {
        xp.Value = new (xp.Value, NEEDED_XP_TO_NEXT_LEVEL);
    }
    /// <summary>
    /// Znovu zrodi hraca
    /// </summary>
    [Rpc(SendTo.Server)] public void ReviveRpc()
    {
        IsAlive.Value = true;
        ResetPositionRpc();
    }
    [Rpc(SendTo.Owner)] public void ResetPositionRpc()
    {
        MapScript.map.SpawnPlayer(transform);
    }
    [Rpc(SendTo.Owner)] public void QuitRpc()
    {
        game.Quit();
    }
    [Rpc(SendTo.Server)] public void RemoveItemFromInventoryRpc(string i)
    {
        inventory.Remove(i);
        DropRpc(i, new (2,2), new (1,1));
    }
    [Rpc(SendTo.Owner)] public void SetRemotePlayerRpc(ulong id, bool connect = true)
    {
        if (id != OwnerClientId && conn.netMan.ConnectedClients.TryGetValue(id, out var client))
        {
            Transform t = client.PlayerObject.transform;

            if (connect)
                game.AddRemotePlayer(t);
            else
                game.RemoveRemotePlayer(t);
        }
    }
#endregion
}