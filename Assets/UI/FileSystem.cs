using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Sluzi pre ziskavanie, nastavenie a ukladanie dat
/// </summary>
public static class FileManager
{
    #region File Paths
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

    // Vsetky subory svetov
    public static string[] Worlds       => Directory.GetFiles(WorldPath);

    /// <summary>
    /// Ziska poslede pripojenie pre pokracovanie solo hry
    /// </summary>
    public static string LastSavedWorld => settings.Solo ? settings.lastConnection.Split('-')[1] : "";

    /// <summary>
    /// K menu sveta prida cestu a priponu
    /// </summary>
    /// <param name="name">nazov SVETA (napr. world)</param>
    /// <returns>CESTU k svetu (napr. "/saves/world.sav")</returns>
    public static string NameToWorldPath(string name)      => WorldPath + name + ".sav";
    /// <summary>
    /// Z cesty ku svetu ziska jeho nazov
    /// </summary>
    /// <param name="path">CESTA ku suboru sveta (napr. "./saves/novysvet.sav")</param>
    /// <returns>NAZOV sveta (napr. novysvet)</returns>
    public static string WorldPathToName(string path)      => path.Split('/')[^1].Split('.')[0];
    #endregion

    /// <summary>
    /// Drzi udaje o aktualnom ulozeni sveta <br />
    /// Ulozene iba na Servery
    /// </summary>
    private static World world; public static World World => world;

    /// <summary>
    /// Drzi udaje o aktualnom nastaveni hry <br />
    /// Ulozene na kazdom klientovy lokalne
    /// </summary>
    private static Settings settings;

    /// <summary>
    /// Nastavi "FileManager" pri vstupe do hlavneho menu <br />
    /// Vymaze nacitany svet a Nacita ulozene nastavenia
    /// </summary>
    public static void Renew()
    {
        LoadSettings();
    }

    #region WorldLoader
    /// <summary>
    /// Ziska vsetky ulozene svety
    /// </summary>
    /// <returns>pole udajo SVETOV</returns>
    public static World[] GetSavedWorlds()
    {
        // Ak priecinok neexistuje vytvori ho
        if (!Directory.Exists(WorldPath))
            Directory.CreateDirectory(WorldPath);

        // Ziska vsetky cesty ulozenych svetom
        string[] wrs = Worlds;
        // Nastavi velkost pola svetov
        World[] worlds= new World[wrs.Length];

        // Ziska udaje kazdeho sveta
        for (int i = 0; i < wrs.Length; i++) 
            worlds[i] = ReadWorld(wrs[i]);

        return worlds;
    }
    /// <summary>
    /// Ziska svet na ceste
    /// </summary>
    /// <param name="path">CESTA a nazov sveta</param>
    /// <returns>SVET na ceste</returns>
    public static World ReadWorld(string path)
    {
        FileStream stream= null;
        World w= null;
        try {
            BinaryFormatter formatter = new();
            stream = new(path, FileMode.Open);
            w = formatter.Deserialize(stream) as World;
        } finally {
            stream?.Close();
        }
        return w;
    }
    /// <summary>
    /// Zapise do udaje sveta binarneho suboru
    /// </summary>
    /// <param name="path">CESTA na zapis</param>
    /// <param name="world">SVET s udajmi</param>
    public static void WriteWorld(string path, ref World world)
    {
        FileStream stream= null;
        try {
            BinaryFormatter formatter = new();
            stream = new(path, FileMode.Create);
            formatter.Serialize(stream, world);
        } finally {
            stream?.Close();
        }
    }
    /// <summary>
    /// Vymaze subor ulozeneho sveta podla jeho nazvu
    /// </summary>
    /// <param name="name">NAZOV sveta</param>
    public static void DeleteWorld(string name)
    {
        // odstrani svet
        File.Delete(NameToWorldPath(name));
        Log("Svet bol odstraneny: " + name, FileLogType.RECORD);
    }
    /// <summary>
    /// Spustanie sveta
    /// </summary>
    /// <param name="name"></param>
    public static async Task StartWorld(string name, GameType type = GameType.Online) 
    {        
        if (!Directory.Exists(WorldPath)) 
            Directory.CreateDirectory(WorldPath);
            
        Log($"World {name} is loading", FileLogType.WARNING);
        string path = NameToWorldPath(name);

        if (File.Exists(path))
        {
            world = ReadWorld(path);
        }
        else
        {
            world = new();
            world.worldName = name;
        }

        Log($"World {name} is in process of loading", FileLogType.WARNING);

        if (type == GameType.Solo)
        {
            Connector.instance.StartSolo();
        }
        else
        {
            bool online = type == GameType.Online;
            await Connector.instance.StartServer(online);
        }

        GameManager.instance.StartGame();
        Log($"World {world.worldName} has been opened", FileLogType.WARNING);
    }
    public static void EndWorld()
    {
        string path = NameToWorldPath(world.worldName);
        
        // Ziska aktualne udaje o svete
        World w = new(world.worldName);

        // Prida udaje nepripojenych hracov
        w.AddOfflinePlayers(world.players);
        
        // Zapise do suboru
        WriteWorld(path, ref w);
        Log($"World {w.worldName} has been closed");
    }
    /// <summary>
    /// Ulozi data jedneho hraca pri jeho odpojeni
    /// </summary>
    /// <param name="player">odpajany HRAC</param>
    public static void SaveClientData(World.PlayerSave player)
    {
        world.SaveRewritePlayer(player);
    }
    #endregion
    #region SettingsLoader
    /// <summary>
    /// Vytvori novy subor nastaveni podla aktualnych hodnot <br /> 
    /// ulozi ho vo formate .xml a prepise ten stary
    /// </summary>
    public static void RegeneradeSettings() // Called on ConnectToSever/SettingsClose
    {
        // Ziska hodnoty aktualneho nastavenia 
        settings ??= new();
        settings.RegeneradeSettings();

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
                settings = serializer.Deserialize(reader) as Settings;
                Menu.menu.LoadSettings(settings);
            }
            finally
            {
                reader?.Close();
                Log("Settings loaded:\n"+settings);
            }
        }
        else
            Log("Settings not found go to setting to gegenerate", FileLogType.WARNING);
    }
    #endregion
    #region References
    /// <summary>
    /// Vrati cestu k ikone utoku podla typu utoku 
    /// </summary>
    /// <param name="type">typ utoku</param>
    /// <returns>CESTA k ikone</returns>
    public static string GetAttackRefferency(Damage.Type type)
    {
        string refer = ATTACKS_ICONS_PATH + "/";
        switch (type)
        {
            case Damage.Type.SWORD_SLASH:
                refer += "sword-slash";
                break;
            case Damage.Type.SWORD_TRUST:
                refer += "sword-thrust";
                break;
            case Damage.Type.FIST:
                refer += "fist";
                break;
            case Damage.Type.BOW_SINLE:
                refer += "bow-single";
                break;
            case Damage.Type.BOW_MULTI:
                refer += "bow-triple";
                break;
            case Damage.Type.POLE:
                refer += "bat-swing";
                break;
        }
        //Debug.Log("Returning at ref on: " + refer);
        return refer;
    }
    public static string GetDamageReff(Damage.Type type)
    {
        string refer = ATTACKS_ICONS_PATH + "/";
        switch (type)
        {
            case Damage.Type.SWORD_SLASH:   refer += "sword-slash";
                break;
            case Damage.Type.SWORD_TRUST:   refer += "sword-thrust";
                break;
            case Damage.Type.FIST:          refer += "fist";
                break;
            case Damage.Type.BOW_SINLE:     refer += "bow-single";
                break;
            case Damage.Type.BOW_MULTI:     refer += "bow-triple";
                break;
            case Damage.Type.POLE:          refer += "bat-swing";
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

    #endregion
    #region LOG
    /// <summary>
    /// Ziskavanie a zapisovanie blizsich informacii o stave hry
    /// </summary>
    /// <param name="message">sprava s udajmi</param>
    /// <param name="type">typ spravy</param>
    public static void Log(string message, FileLogType type = FileLogType.LOG)
    {
        // Zapise aktualny cas
        string log = $"[{DateTime.Now}] ";
        bool writeToFile = type != FileLogType.LOG;
        log += type == FileLogType.ERROR || FileLogType.WARNING == type ? $"[{Enum.GetName(typeof(FileLogType), type)}]" : "";

        log += message;

        // Zapise spravu do suboru
        if (writeToFile)
        {
            using StreamWriter sw = new (LogPath, true);
            sw.WriteLine(log);
            sw.Flush();
            sw.Close();
            log = "[RECORDED] " + log;
        }

        // Vypise spravu do konzoly v editore
        switch (type)
        {
            default:                    Debug.Log(log+" [RECORDED]");         break;
            case FileLogType.ERROR:     Debug.LogError(log);    break;
            case FileLogType.WARNING:   Debug.LogWarning(log);  break;
        }
    }
}
/// <summary>
/// Typ zapisu v denniku ("LOG" sa do suboru nepise)
/// </summary>
public enum FileLogType { LOG, RECORD, ERROR, WARNING }
#endregion
#region SETTINGS
/// <summary>
/// Drzi informacie o poslednej konfiguracii nastaveni
/// </summary>
[Serializable] public class Settings
{
    public bool fullSc;
    public int quality;
    public string playerName;
    public float[] audioS;
    
    /// <summary>
    /// Multifukcne pole textu sluziace ako posledne
    /// </summary>
    public string lastConnection;
    public bool Solo => lastConnection.Contains("solo-");
    public bool Server => lastConnection.Contains("server-");
    public bool Online => lastConnection.Count(o => o == '.') == 3;
    public bool Client => !(Solo || Server);

    /// <summary>
    /// Ziska si hodnoty zo statickych clenov menu
    /// </summary>
    public Settings()
    {
        try {
            lastConnection = Connector.instance.GetConnection();
            playerName = Menu.menu.PlayerName;
            quality = Menu.menu.Quality;
            audioS = Menu.menu.Audios;
            fullSc = Menu.menu.FullSc;
        } catch (Exception ex) {
            FileManager.Log($"Setting Creation Error \nExeption: {ex.Message}\nData: {ex.Data}", FileLogType.WARNING);
        }
    }
    public void RegeneradeSettings()
    {
        if (ReNewSettings(new()))
            ApplySettings();
    }
    /// <summary>
    /// Nastavi hodnoty do statickych clenov menu
    /// </summary>
    /// <param name="settings"></param>
    private bool ReNewSettings(Settings settings)
    {
        bool changed = !Equals(settings);

        fullSc = settings.fullSc;
        audioS = settings.audioS;
        quality = settings.quality;
        playerName = settings.playerName;     

        if (changed && settings.lastConnection != "")
            lastConnection = settings.lastConnection;
        
        return changed;
    }
    private void ApplySettings()
    {
        // Nastavi vlastnosti hry podla novych hodnot
        Menu.menu.LoadSettings(this);
    }
    /// <summary>
    /// Pororvnanie hodnot
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool Equals(Settings other)
    {
        bool eq = true;

        eq &= fullSc == other.fullSc;
        eq &= audioS == other.audioS;
        eq &= quality == other.quality;
        eq &= playerName == other.playerName;

        if (other.lastConnection != "")
            eq &= lastConnection == other.lastConnection;
        
        return eq;
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
            $"Last connection= {lastConnection}\n"+
            $"Quality setting= {quality}\n"+
            $"Fullscreen= {fullSc}\n"+
            $"Auidos list: {auL}";
    }
}
#endregion
#region WORLD
/// <summary>
/// Drzi hodnoty potrebne pre bezproblemove nacitanie zo suboru
/// </summary>
public enum GameType { Online, Local, Solo }
[Serializable] public class World
{
    public bool singlePlayer;
    public string worldName;
    public string writeDate;
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
        writeDate = DateTime.Now.ToString();
        items = new ();
        players = new ();
        entities = new ();
        singlePlayer = false;
        boss = null;
    }
    /// <summary>
    /// Ziska udaje o svete a zapise ich od premennych
    /// </summary>
    /// <exception cref="Ak nie je server tak zlyha"></exception>
    public World(string name)
    {
        worldName = name;
        singlePlayer = Connector.instance.Solo;
        writeDate = DateTime.Now.ToString();
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

        FileManager.Log($"Player {player.etName} save {(0 < index ? "rewriten" : "added")} with values {player}", FileLogType.RECORD);
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
        FileManager.Log($"Player {name} requested save file: {(player != null ? player : "NOT FOUND")}", FileLogType.RECORD);
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
        public WeaponIndex weapon;
        public string etName;
        public float hp;

        public Vector2 Position => position.Vector;

        public EntitySave(EntityStats entity)
        {
            position = new(entity.transform.position.x,entity.transform.position.y);
            weapon = entity.WeaponPrameter;
            etName = entity.transform.name;
            hp = entity.HP;

            if (entity is NPStats nps && entity is not BosStats)
                etName += "-" + nps.GetComponent<NPController>().DefaultTarget;
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
        public uint xp;
        public int maxHp;
        public byte level;
        public PlayerSave(PlayerStats player) : base(player)
        {
            inventory = player.InventorySave;
            skillTree = player.SkillTreeSave;
            level = player.Level;
            maxHp = player.MaxHP;
            xp = player.xp.Value.value;
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
                return $"items.Lenght= {items.Length}";
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
            s += $", level= {level}";
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
#endregion