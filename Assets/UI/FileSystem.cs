using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Sluzi pre ziskavanie, nastavenie a ukladanie dat
/// </summary>
public static class FileManager
{
    // relativna cesta v priecinku "RECOURCES"
    public const string ITEM_DEFAULT_PATH = @"Items";
    public const string SKILLS_ICONS_PATH = @"UI/skills/";
    public const string ATTACKS_ICONS_PATH = @"UI/at_types";
    public const string WEAPONS_DEFAULT_PATH = @"Items/weapons";
    public const string TEXTURE_DEFAULT_PATH = @"Items/textures";
    public const string PROJECTILES_OBJECTS_PATH = @"Projectiles/";
    public const string WEAPONS_REF_DEFAULT_PATH = @"Items/textures/InGame";

    // Pridavky k ceste "Application.persistentDataPath"
    private const string LOG_DEFAULT_PATH = @"/debug.log";
    private const string SETTINGS_DEFAULT_PATH = @"/settings.xml";
    private const string WORLD_DEFAULT_PATH = @"/saves/"; // + nazov

    // Ukazovatele na cesty ukladaina
    public static string AppData        => Application.persistentDataPath;
    public static string SettingsPath   => AppData + SETTINGS_DEFAULT_PATH;
    public static string LogPath        => AppData + LOG_DEFAULT_PATH;
    public static string WorldPath      => AppData + WORLD_DEFAULT_PATH;

    /// <summary>
    /// Drzi udaje o aktualnom ulozeni sveta <br />
    /// Ulozene iba na Servery
    /// </summary>
    private static World world;
    public static World World => world;

    /// <summary>
    /// Drzi udaje o aktualnom nastaveni hry <br />
    /// Ulozene na kazdom klientovy lokalne
    /// </summary>
    private static Settings settings;
    /// <summary>
    /// Typ akcie na vykonanie so svetom
    /// </summary>
    public enum WorldAction { Create, Load, Save, Check }

    /// <summary>
    /// Akcia spustenia hry
    /// </summary>
    /// <param name="action"></param>
    /// <returns>PRAVDA ak prebehlo uspesne</returns>
    public static bool WorldAct(string name, WorldAction action)
    {
        bool acted= false;

        switch (action)
        {
            case WorldAction.Create:
                world = new();
                acted = true;
                break;
            case WorldAction.Load:
                acted = LoadWorldData(WorldPath+name);
                break;
            case WorldAction.Save:
                acted = SaveWorldData(WorldPath+name);
                break;
            case WorldAction.Check:
                break;
        }

        return acted;
    }
    /// <summary>
    /// Ulozi data jedneho hraca pri jeho odpojeni
    /// </summary>
    /// <param name="player">odpajany HRAC</param>
    public static void SaveClientData(World.PlayerSave player)
    {
        if (world != null)
            world.SaveRewritePlayer(player);
        else
            world = new();
    }
    /// <summary>
    /// Ulozenie aktualneho sveta do binarneho suboru
    /// </summary>
    /// <param name="path">cesta a nazov suboru</param>
    /// <returns>PRAVDA ak prebehlo uspesne</returns>
    private static bool SaveWorldData(string path)
    {
        bool saved = false;
        FileStream stream = null;

        try {
            stream = new(path, FileMode.Create);
            BinaryFormatter formatter = new();

            World w = new();
            if (world != null)
                w.AddOfflinePlayers(world.players);

            formatter.Serialize(stream, w);
            saved = File.Exists(path);
            if (saved)
                world = w;
        } finally {
            stream.Close();
            Log($"World ({(saved ? "was" : "wasn't")}) saved: {world}");
        }

        return saved;
    }
    /// <summary>
    /// Nacitanie sveta zo suboru
    /// </summary>
    /// <param name="path">cesta a nazov suboru</param>
    /// <returns>PRAVDA ak prebehlo uspesne</returns>
    private static bool LoadWorldData(string path)
    {
        bool loaded = false;

        if (File.Exists(path))
        {
            FileStream stream = null;
            try {
                stream = new(path, FileMode.Open);
                BinaryFormatter formatter = new();
                World w = formatter.Deserialize(stream) as World;
                world = w;
                loaded = true;
            } finally {
                stream.Close();
                Log($"World ({(loaded ? "was" : "wasn't")}) loaded: {world}");                
            }
        }
        else
            Log("Savefile not found: " + path, MessageType.ERROR);

        return loaded;
    }
    /// <summary>
    /// Vytvori novy subor nastaveni podla aktualnych hodnot <br /> 
    /// ulozi ho vo formate .xml a prepise ten stary
    /// </summary>
    public static void RegeneradeSettings() // Called on ConnectToSever/SettingsClose
    {
        // Ziska hodnoty aktualneho nastavenia 
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
            Log("Settings regenerated:\n"+settings);
        }
    }
    /// <summary>
    /// Nacita udaje nastaveni zo .xml suboru
    /// </summary>
    public static void LoadSettings()
    {
        // Skontroluje ci subor nastaveni existuje
        if (File.Exists(SettingsPath))
        {
            // ak ANO pokusi sa ho nacitat
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(Settings));
                reader = new StreamReader(SettingsPath);

                // Nacitane hodnoty zo suboru nastavi ako aktuale
                settings ??= new();
                settings?.LoadSettings((Settings)serializer.Deserialize(reader));
            }
            finally
            {
                reader?.Close();
                Log("Settings loaded:\n"+settings);
            }
        }
    }
    /// <summary>
    /// Vrati cestu k ikone utoku podla typu utoku 
    /// </summary>
    /// <param name="type">typ utoku</param>
    /// <returns>CESTA k ikone</returns>
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
    /// <returns>CESTA k texture skillu</returns>
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

    /// <summary>
    /// Ziskavanie a zapisovanie blizsich informacii o stave hry
    /// </summary>
    /// <param name="message">sprava s udajmi</param>
    /// <param name="type">typ spravy</param>
    public static void Log(string message, MessageType type = MessageType.LOG)
    {
        // Zapise aktualny cas
        string log = $"[{DateTime.Now}] ";
        bool writeToFile = type != MessageType.LOG;
        if (writeToFile)
            log = "[RECORDED] " + log;
        log += message;

        // Zapise spravu do suboru
        if (writeToFile)
        {
            using StreamWriter sw = new (LogPath, true);
            sw.WriteLine(log);
            sw.Flush();
            sw.Close();
        }

        // Vypise spravu do konzoly v editore
        switch (type)
        {
            default:                    Debug.Log(log);         break;
            case MessageType.ERROR:     Debug.LogError(log);    break;
            case MessageType.WARNING:   Debug.LogWarning(log);  break;
        }
    }
    /// <summary>
    /// Typ zapisu v denniku ("LOG" sa do suboru nepise)
    /// </summary>
    public enum MessageType { LOG, RECORD, ERROR, WARNING }
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
            playerName = GameManager.UI.PlayerName;
            quality = GameManager.UI.Quality;
            audioS = GameManager.UI.Audios;
            online = GameManager.UI.Online;
            fullSc = GameManager.UI.FullSc;
        } catch (Exception ex) {
            FileManager.Log($"Setting Creation Error \nExeption: {ex.Message}\nData: {ex.Data}", FileManager.MessageType.WARNING);
        }
    }
    /// <summary>
    /// Nastavi hodnoty do statickych clenov menu
    /// </summary>
    /// <param name="settings"></param>
    public void LoadSettings(Settings settings)
    {
        // Nastavi hodnoty z 
        online = settings.online;
        fullSc = settings.fullSc;
        audioS = settings.audioS;
        quality = settings.quality;
        playerName = settings.playerName;
        lastConnection = settings.lastConnection;
        
        // Nastavi vlastnosti hry podla novych hodnot
        Connector.instance.codeText.text = lastConnection;
        GameManager.instance.PlayerName = playerName;
        GameManager.UI.Audios = audioS;
        GameManager.UI.Quality = quality;
        GameManager.UI.Online = online;
        GameManager.UI.FullSc = fullSc;
    }
    /// <summary>
    /// Sluzi ako moznost kontroly spravnosti ulozenia nastaveni
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
/// Drzi hodnoty potrebne pre bezproblemove nacitanie zo suboru
/// </summary>
[Serializable] public class World
{
    public string worldName;
    public List<ItemOnFoor> items;
    public List<PlayerSave> players;
    public List<EntitySave> entities;
    public BossSave boss;
    /// <summary>
    /// Vytvori prazdny save pre svet
    /// </summary>
    public World()
    {
        worldName = "";
        items = new ();
        players = new ();
        entities = new ();
        boss = null;
    }
    /// <summary>
    /// Ziska udaje o svete a zapise ich od premennych
    /// </summary>
    /// <exception cref="Ak nie je server tak zlyha"></exception>
    public World(string name)
    {
        worldName = name;
        if (Connector.instance.netMan.IsServer)
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
    /// Spaja data o hracoch zo stareho aj aktualneho ulozenia <br />
    /// Sluzi na uchovanie dat o hracoch, ktori niesu pripojeny
    /// </summary>
    /// <param name="oldPlayerList">Old Player list</param>
    public void AddOfflinePlayers(List<PlayerSave> oldPlayerList)
    {
        oldPlayerList.ForEach(p => {if (!players.Contains(p)) players.Add(p); } );
    }
    /// <summary>
    /// Prepise data len konkretnemu hracovi
    /// </summary>
    /// <param name="player"></param>
    public void SaveRewritePlayer(PlayerSave player)
    {
        int index = players.FindIndex(p => p.etName == player.etName);
        if (0 < index && index < players.Count)
            players[index] = player;
        else
            players.Add(player);

        FileManager.Log($"Player {player.etName} save {(0 < index ? "rewriten" : "added")} with values {player}", FileManager.MessageType.RECORD);
    }    
    /// <summary>
    /// Ziska udaje hraca na zaklade mena
    /// </summary>
    /// <param name="name">MENO hraca</param>
    /// <param name="player">vychadzajuce UDAJE hraca</param>
    /// <returns></returns>
    public bool TryGetPlayerSave(string name, out PlayerSave player)
    {
        player = null;
        player = players.Find(p => p.etName == name);
        FileManager.Log($"Player {name} requested save file: {(player != null ? player : "NOT FOUND")}", FileManager.MessageType.RECORD);
        return player != null;
    }
    /// <summary>
    /// Suhrny vypis o ulozenych dat v subore 
    /// </summary>
    /// <returns>SUHRN informacii</returns>
    public override string ToString()
    {
        string s = "";
        s += $"items.Count= {items.Count}, ";
        s += $"entities.Count= {entities.Count}, ";
        s += $"Boss: {boss}";
        s += $"players.Count= {players.Count}\n";
        s += "Mena Hracov: ";
        players.ForEach(p => s += p.etName + ", ");
        return s;
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

        public Vector2 Position => position.Vector;

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
            inventory = player.InventorySave;
            skillTree = player.SkillTreeSave;
        }
        /// <summary>
        /// Zrdi informacie o inventari hraca
        /// </summary>
        [Serializable] public class InventorySave
        {
            public string[] items;
            public string[] equiped;

            /// <summary>
            /// Vytvori objekt drziaci udaje o inventary hraca
            /// </summary>
            /// <param name="items">PREMETY v inventary</param>
            /// <param name="equiped">nosene PREDMETY</param>
            public InventorySave(string[] items, string[] equiped)
            {
                this.items = items;
                this.equiped = equiped;
            }

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

            public SkillTreeSave(Skill[] skills, string[] actives = null)
            {
                this.skills = skills;

                // ak su aktivyty "null" nastavi pole s 2-ma hodnotami ("")
                usingUtils = actives ?? new string[] { "", ""} ;
            }

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
        public bool isAlive;
        public BossSave(BosStats entity) : base(entity)
        {
            isAlive = entity.IsAlive.Value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            string s = base.ToString();
            s += $"IsAlive: {isAlive}";
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