using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using NUnit.Framework.Constraints;

/// <summary>
/// Sluzi pre ziskavanie, nastavenie a ukladanie dat
/// </summary>
public static class FileManager
{
    // FROM RECOURCES
    public const string TEXTURE_DEFAULT_PATH = @"Items/textures";
    public const string ITEM_DEFAULT_PATH = @"Items";
    public const string WEAPONS_DEFAULT_PATH = @"Items/weapons";
    //public const string ARMORS_DEFAULT_PATH = @"Items/armors";
    public const string WEAPONS_REF_DEFAULT_PATH = @"Items/textures/InGame";
    public const string ATTACKS_ICONS_PATH = @"UI/at_types";
    public const string PROJECTILES_OBJECTS_PATH = @"Projectiles/";
    public const string SKILLS_ICONS_PATH = @"UI/skills/";

    
    // FROM APP DATA PATH
    private const string LOG_DEFAULT_PATH = @"";
    private const string SETTINGS_DEFAULT_PATH = @"/settings.xml";
    private const string WORLD_DEFAULT_PATH = @"/saves/"; // + nazov


    public static string AppData { get =>  Application.persistentDataPath; }
    public static string SettingsPath   { get => AppData + SETTINGS_DEFAULT_PATH; }
    public static string LogPath        { get => AppData + LOG_DEFAULT_PATH;    }
    public static string WorldPath     { get => AppData + WORLD_DEFAULT_PATH;  }


    private static World world;     // Ulozene iba na servery
    public enum WorldAction
    {
        Create, Load, Save, Check
    }
    public static bool WorldAct(WorldAction action)
    {
        bool acted= false;

        switch (action)
        {
            case WorldAction.Create:
                world = new();
                acted = true;
                break;
            case WorldAction.Load:
                acted = LoadWorldData(WorldPath+"save1");
                break;
            case WorldAction.Save:
                acted = SaveWorldData(WorldPath+"save1");
                break;
            case WorldAction.Check:
                //acted = false;
                break;
        }

        return acted;
    }
    private static bool SaveWorldData(string path)
    {
        bool saved = false;
        BinaryFormatter formatter = new();
        FileStream stream = new(path, FileMode.Create);

        world = new(world);
        formatter.Serialize(stream, world);
        saved = File.Exists(path);        
        stream.Close();

        return saved;
    }
    private static bool LoadWorldData(string path)
    {
        bool loaded = false;



        return loaded;
    }

    private static Settings settings = new();
    public static void RegeneradeSettings() // Called on ConnectToSever/SettingsClose
    {
        settings = new Settings();
        TextWriter writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(Settings));
            writer = new StreamWriter(SettingsPath);
            serializer.Serialize(writer, settings);
        }
        finally
        {
            writer?.Close();
            Debug.Log("Settings regenerated:\n"+settings);
        }
    }
    public static void LoadSettings()
    {/*
        if (settings == null)
            settings = new(false);*/
        if (File.Exists(SettingsPath))
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(Settings));
                reader = new StreamReader(SettingsPath);
                settings.SetSettings((Settings)serializer.Deserialize(reader));
            }
            finally
            {
                reader?.Close();
                Debug.Log("Settings loaded:\n"+settings);
            }
        }
    }
    /// <summary>
    /// Vrati cestu k ikone utoku podla typu utoku 
    /// </summary>
    /// <param name="type">typ utoku</param>
    /// <returns>CESTA_K_IKONE</returns>
    public static string GetAttackRefferency(Attack.Type type)
    {
        string refer = ATTACKS_ICONS_PATH + "/";
        switch (type)
        {
            case Attack.Type.MeleeSlash:
                refer += "sword-slash";
                break;
            case Attack.Type.MeleeStab:
                refer += "sword-thrust";
                break;
            case Attack.Type.RaseUnnarmed:
                refer += "fist";
                break;
            case Attack.Type.BowSingle:
                refer += "bow-single";
                break;
            case Attack.Type.BowMulti:
                refer += "bow-triple";
                break;
            case Attack.Type.BatSwing:
                refer += "bat-swing";
                break;
        }
        //Debug.Log("Returning at ref on: " + refer);
        return refer;
    }
    /// <summary>
    /// Ziska ikonu schopnosti podla Schopnosti
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public static string[] GetSkillRefferency(Skill skill)
    {
        List<string> list = new();

        if (skill is Utility)
            list.Add(SKILLS_ICONS_PATH + "utility"); 
        else if (skill is ModDamage mD)
        {
            if (mD.damage)
            {
                list.Add(SKILLS_ICONS_PATH + "attacks");
                if (mD.isSpeed)
                    list.Add(SKILLS_ICONS_PATH + "rateUPC");
                else
                    list.Add(SKILLS_ICONS_PATH + "valueUP");
            }
            else
            {
                list.Add(SKILLS_ICONS_PATH + "shieldUP");
                list.Add(SKILLS_ICONS_PATH + "valueUP");                
            }
        }        
        else if (skill is ModSkill mS)
        {
            if (mS.isSpeed)
                list.Add(SKILLS_ICONS_PATH + "speedUP");
            else
                list.Add(SKILLS_ICONS_PATH + "healthUP");
            list.Add(SKILLS_ICONS_PATH + "valueUP");
        }

        return list.ToArray();
    }

    public enum MessageType { LOG, RECORD, ERROR, WARNING }
    public static void Log(string message, MessageType type = MessageType.LOG)
    {
        switch (type)
        {
            case MessageType.RECORD:
                Debug.Log("[RECORDED] " + message);
                break;
            case MessageType.ERROR:
                Debug.LogWarning("[RECORDED] " + message);
                break;
            case MessageType.WARNING:
                Debug.LogError("[RECORDED] " + message);
                break;
        }
    }
    public static void SaveClientData(EntityStats stats)
    {

    }
}
/// <summary>
/// Drzi informacie o poslednej konfiguracii nastaveni
/// </summary>
[Serializable] public class Settings
{
    public bool online;
    public bool fullSc;
    public int quality;
    public string playerName;
    public string lastConnection;
    public float[] audioS;
    // ...

    /// <summary>
    /// Ziska si hodnoty zo statickych clenov menu
    /// </summary>
    public Settings()
    {
        try {
            lastConnection = Connector.instance.codeText.text;
            playerName = GameManager.instance.PlayerName;
            quality = GameManager.UI.Quality;
            audioS = GameManager.UI.Audios;
            online = GameManager.UI.Online;
            fullSc = GameManager.UI.FullSc;
        } catch (Exception ex) {
            Debug.LogWarning($"Setting Creation Error \nExeption: {ex.Message}\nSource: {ex.Source}");
            // Revert to defaults Settings
        }
    }
    /// <summary>
    /// Nastavi hodnoty do statickych clenov menu
    /// </summary>
    /// <param name="settings"></param>
    public void SetSettings(Settings settings)
    {
        online = settings.online;
        fullSc = settings.fullSc;
        audioS = settings.audioS;
        quality = settings.quality;
        playerName = settings.playerName;
        lastConnection = settings.lastConnection;
        
        Connector.instance.codeText.text = lastConnection;
        GameManager.instance.PlayerName = playerName;
        GameManager.UI.Audios = audioS;
        GameManager.UI.Quality = quality;
        GameManager.UI.Online = online;
        GameManager.UI.FullSc = fullSc;
    }
    /// <summary>
    /// Sluzi ako moznost kontroly spravnosti
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string auL = "[ ";
        for (int i = 0; i < audioS.Length; i++)
        {
            auL += audioS[i];
            if (i < audioS.Length -1 )
                auL+=" | ";
            else
                auL+= " ]";
        }

        return
            $"Player NameTag= {playerName}\n"+
            $"Last played online= {online}\n"+
            $"Last game Connected= {lastConnection}\n"+
            $"Quality setting= {quality}\n"+
            $"Fullscreen= {fullSc}\n"+
            $"Auidos list: {auL}";
    }
}
/// <summary>
/// Drzi hodnoty pre cely svet
/// </summary>
[Serializable] public class World
{
    public readonly List<ItemOnFoor> items;
    public readonly List<PlayerSave> players;
    public readonly List<EntitySave> entities;
    public readonly BossSave boss;

    /// <summary>
    /// Ziska udaje o svete a zapise ich od premennych
    /// </summary>
    /// <exception cref="Ak nie je server tak zlyha"></exception>
    public World()
    {
        if (GameManager.instance.IsServer)
        {
            items = new ();
            players = new ();
            entities = new ();
            foreach (GameObject e in GameObject.FindGameObjectsWithTag("Entity"))
            {
                if (e.TryGetComponent(out EntityStats eS))
                {
                    if (eS is BosStats bS)
                        boss = new (bS);
                    else
                        entities.Add(new (eS));
                }
                else
                    Debug.LogWarning($"NotEntity with tag Entity: {e.name} was incorectly handled in savefile");
            }
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (p.TryGetComponent(out PlayerStats pS))
                    players.Add(new (pS));
                else
                    Debug.LogWarning($"NotPlayer with tag Player: {p.name} was incorectly handled in savefile");

            }
            foreach (GameObject d in GameObject.FindGameObjectsWithTag("Drop"))
            {
                if (d.TryGetComponent(out ItemDrop iD))
                    items.Add(new (iD));
                else
                    Debug.LogWarning($"NotDropItem with tag Drop: {d.name} was incorectly handled in savefile");
            }
        }
        else
        {
            Debug.LogWarning("World tried to save on klient");
            throw new Exception("World tried to save on klient");
        }
    }
    /// <summary>
    /// Spaja data (o hracoch) z stareho aj aktualneho ulozenia
    /// </summary>
    /// <param name="old_world"></param>
    public World(World old_world)
    {
        World new_world= new();
        items = new_world.items;
        players = new_world.players;
        entities = new_world.entities;

        old_world.players.ForEach(p => {if (!players.Contains(p)) players.Add(p); } ); // najde vsetkych hracov co nesu pripojeny
    }

    /// <summary>
    /// Drzi udaje o itemoch na zemi
    /// </summary>
    [Serializable] public class ItemOnFoor
    {
        public Cordinates pos;
        public string itemRef;
        public ItemOnFoor(Vector2 _pos, string _itemRef)
        {
            pos = new (_pos);
            itemRef = _itemRef;
        }
        public ItemOnFoor(ItemDrop iDrop)
        {
            pos = new (iDrop.transform.position);
            itemRef = iDrop.Item.GetReferency;
        }
    }
    /// <summary>
    /// Drzi informacie o charakteroch vo svete
    /// </summary>
    [Serializable] public class EntitySave
    {
        public Cordinates position;
        public string etName;
        public float hp;

        public EntitySave(EntityStats entity)
        {
            position = new(entity.transform.position.x,entity.transform.position.y);
            etName = entity.transform.name;
            hp = entity.HP;
        }

        /// <summary>
        /// Vypis informacii charaktera
        /// </summary>
        /// <returns>Meno, poziciu, pocetZivotov</returns>
        public override string ToString()
        {
            return $"{etName} on {position} with hp=  {hp}";
        }
    }
    /// <summary>
    /// Drzi informacie o hracovi, ktori sa pripojil na server
    /// </summary>
    [Serializable] public class PlayerSave : EntitySave
    {
        public InventorySave inventory;
        public SkillTreeSave skillTree;
        public int maxHp;
        public PlayerSave(PlayerStats player) : base(player)
        {

        }

        /// <summary>
        /// Zrdi informacie o inventari hraca
        /// </summary>
        [Serializable] public class InventorySave
        {
            private string[] items;
            private string[] equiped;

            /// <summary>
            /// Vypis vlasnosti ulozeneho inventara hraca
            /// </summary>
            /// <returns>pocetPredmetov pocetPoredmetovNaSebe</returns>
            public override string ToString()
            {
                return $"items.Lenght= {items.Length} equiped.Lenght= {equiped.Length}";
            }
        }
        /// <summary>
        /// Drzi informacie o ziskanych a pouzivanych schopnostiach hraca
        /// </summary>
        [Serializable] public class SkillTreeSave
        {
            public Skill[] skills;
            public string[] usingUtils;

            /// <summary>
            /// Vypis vlastosti ulozeneho stromu schopnosti 
            /// </summary>
            /// <returns>pocetSchonosti pocetPozivanychSchopnosti</returns>
            public override string ToString()
            {
                string s = "";
                foreach (string uS in usingUtils)
                    s += $"{uS} ";
                return $"skills.Lenght= {skills.Length} usingUtils = [{s}]";
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/>, maximalneZdravie, inventar, stromSchonpnosti</returns>
        public override string ToString()
        {
            string s = base.ToString();
            s += $", MaxHP= {maxHp}";
            s += $", Inventar= {inventory}";
            s += $", SkillTree= {skillTree}";
            return s;
        }
        /// <summary>
        /// Porovnava save dvoch hracov
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Ak sa jedna o jedneho hraca</returns>
        public bool Equals(PlayerSave other)
        {
            return etName == other.etName;
        }
    }
    /// <summary>
    /// Drzi informacie o hlavnom nepriatelovi
    /// </summary>
    [Serializable] public class BossSave : EntitySave
    {
        public BossSave(BosStats entity) : base(entity)
        {

        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            string s = base.ToString();

            return s;
        }
    }

    /// <summary>
    /// Drzi informacie o polohe v dvoj rozmernom svete <br />
    /// "Vector2" nie je systemovo Srializovatelny
    /// </summary>
    [Serializable] public class Cordinates
    {
        public float x,y;
        public Cordinates(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Cordinates(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
        public Cordinates(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
        }
        public Vector2 Vector { get { return new Vector2(x,y); } }
        /// <summary>
        /// Vypis pozicie
        /// </summary>
        /// <returns>(x,y)</returns>
        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}
/********************************************************************************** OLD CODE *****************************************************************************\
public static class SaveSystem
{
    public static bool CheckSaveNeed()
    {
        return CompareData(GetData(), Load());
    }
    public static bool SaveDataExist(string filename = "data")
    {
        Data d = Load();
        return DataCheck(d);
    }
    private static bool DataCheck(Data data)
    {
        bool check = true;
        try
        {
            // Level number
            check &= data != null;
            check &= data.curLevel >= 0;
            check &= data.level.roomsPos.Length == data.level.rooms.Length;


            // Entities => min. 1 {Player}
            check &= data.entities.Length >= 1;
            foreach (Data.CharakterData e in data.entities)
                check &= e.curHealth >= 0 && e.charName != "" && e.position != null;

            // Inventory + Equipment DUPLICATE CHECK
            check &= data.inventory.items.Count >= 0;
            check &= data.inventory.equipment.Length >= 0;
            List<ItemData> list = new();
            for (int i = 0, e = 0; (i < data.inventory.items.Count || data.inventory.equipment.Length > e) && check; i++, e++)
            {
                if (i < data.inventory.items.Count)
                {
                    check &= !list.Contains(data.inventory.items[i]);
                    list.Add(data.inventory.items[i]);
                }
                if (e < data.inventory.equipment.Length)
                {
                    check &= !list.Contains(data.inventory.equipment[e]);
                    list.Add(data.inventory.equipment[e]);
                }
            }
            list.Clear();

            // Interactables
            check &= data.interactables.positions.Length == data.interactables.items.Length;
        }
        catch
        {
            check = false;
        }
        if (check)
            Debug.Log("Passed data check!");
        else
            Debug.Log("Failed data check!");
        return check;
    }
    private static void DebugLogForComperators(bool pass, string message)
    {
        if (debug)
        {
            if (pass)
                Debug.Log(message + " passed!");
            else
                Debug.Log(message + " failed!");
        }
    }
    private static void DebugLogDataOut(Data data, string action)
    {
        if (debug)
            Debug.Log($"Data {action} " + data);
    }
    public static void Save()
    {
        BinaryFormatter formatter = new();
        FileStream stream = new(Path("data"), FileMode.Create);

        Data data = GetData();
        formatter.Serialize(stream, data);
        stream.Close();
        DebugLogDataOut(data, "saved");
    }
    private static Data GetData()
    {
        // Entities DATA
        List<Data.CharakterData> charakterData = new()
        { GameManager.instance.playerStats.SaveData() };
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
            charakterData.Add(e.GetComponent<EnemyStats>().SaveData());
        if (GameManager.instance.boss != null)
            charakterData.Add(GameManager.instance.boss.SaveData());

        return new(charakterData);
    }
    public static Data Load()
    {
        if (File.Exists(Path("data")))
        {
            BinaryFormatter formatter = new();
            FileStream steam = new(Path("data"), FileMode.Open);

            Data data = formatter.Deserialize(steam) as Data;
            steam.Close();
            DebugLogDataOut(data, "loaded");
            return data;
        }
        else
        {
            Debug.LogError("Savefile not found: " + Path("data"));
            return null;
        }
    }
    private static string Path(string filename)
    {
        string path = Application.persistentDataPath + "/" + filename + ".file";
        return path;
    }*/