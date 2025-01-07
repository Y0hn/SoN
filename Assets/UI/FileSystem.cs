using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
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
    public static string[] GetSkillRefferency(SkillSlot.SkillCreator.SkillType type)
    {
        List<string> list = new();

        switch (type)
        {
            case SkillSlot.SkillCreator.SkillType.Utility:
                list.Add(SKILLS_ICONS_PATH + "utility");             
                break;
            case SkillSlot.SkillCreator.SkillType.Health: 
                list.Add(SKILLS_ICONS_PATH + "healthUP");
                list.Add(SKILLS_ICONS_PATH + "valueUP");                    
                break;
            case SkillSlot.SkillCreator.SkillType.Protection: 
                list.Add(SKILLS_ICONS_PATH + "shieldUP");
                list.Add(SKILLS_ICONS_PATH + "valueUP");                    
                break;
            case SkillSlot.SkillCreator.SkillType.AttackDamage: 
                list.Add(SKILLS_ICONS_PATH + "attacks");
                list.Add(SKILLS_ICONS_PATH + "valueUP");                
                break;
            case SkillSlot.SkillCreator.SkillType.AttackRate: 
                list.Add(SKILLS_ICONS_PATH + "attacks");
                list.Add(SKILLS_ICONS_PATH + "rateUPC");
                break;            
            case SkillSlot.SkillCreator.SkillType.MovementSpeed:
                list.Add(SKILLS_ICONS_PATH + "speedUP");
                list.Add(SKILLS_ICONS_PATH + "valueUP");
                break;
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
[Serializable] public class Settings
{
    public bool online;
    public bool fullSc;
    public int quality;
    public string playerName;
    public string lastConnection;
    public float[] audioS;
    // ...
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
[Serializable] public class World
{
    readonly List<ItemOnFoor> items;
    readonly List<EntitySave> players;
    readonly List<EntitySave> entities;

    public World(bool get = false)
    {
        if (get)
        {
            if (GameManager.IsServer)
            {
                foreach (GameObject e in GameObject.FindGameObjectsWithTag("Entity"))
                {
                    EntitySave es = new (e.GetComponent<EntityStats>());

                    if (es.isPlayer)
                        players.Add(es);
                    else
                        entities.Add(es);
                }
                foreach (GameObject d in GameObject.FindGameObjectsWithTag("Drop"))
                    items.Add(new (d.GetComponent<ItemDrop>()));
            }
            // Client nemoze Ukladat stav sveta
        }
        else
        {
            items = new();
            entities = new();
        }
    }
    /// <summary>
    /// Spaja data (o hracoch) z stareho aj aktualneho ulozenia
    /// </summary>
    /// <param name="old_world"></param>
    public World(World old_world)
    {
        World new_world= new(true);
        items = new_world.items;
        players = new_world.players;
        entities = new_world.entities;

        List<EntitySave> pl = old_world.entities.FindAll(e => e.isPlayer);
        pl = players.FindAll(e => entities.Find(en => en.etName == e.etName) == null); // najde vsetkych hracov co nesu pripojeny
        players.AddRange(pl);
    }
    [Serializable] public class ItemOnFoor
    {
        public Vector2 pos;
        public string itemRef;
        public ItemOnFoor(Vector2 _pos, string _itemRef)
        {
            pos = _pos;
            itemRef = _itemRef;
        }
        public ItemOnFoor(ItemDrop iDrop)
        {
            pos = iDrop.transform.position;
            itemRef = iDrop.Item.GetReferency;
        }
    }
    [Serializable] public class EntitySave
    {
        protected Defence defence;
        protected Vector2 position;
        public string etName;
        public bool isPlayer;
        protected float hp;

        public EntitySave(EntityStats entity)
        {
            position = new(entity.transform.position.x,entity.transform.position.y);
            isPlayer = entity is PlayerStats;
            etName = entity.transform.name;
            hp = entity.HP;
        }
    }
    [Serializable] public class PlayerSave : EntitySave
    {
        InventorySave inventory;
        public PlayerSave(EntityStats player) : base(player)
        {

        }

        [Serializable] public class InventorySave
        {
            public readonly string playerName;
            private string[] items;
            private string[] equiped;/*
            public InventorySave(Inventory inventory)
            {
                playerName = GameManager.instance.PlayerName;
                foreach (ItemSlot it in inventory.inventoryGrid)
                {

                }
            }*/
        }
    }
    
    /// <summary>
    /// Nahrada za Vector2 v World pre FileSystem
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