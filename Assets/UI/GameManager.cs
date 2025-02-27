using AYellowpaper.SerializedCollections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

/// <summary>
/// Managing Game and PLayerUI - has 'instance'
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Action GameQuit;
    [SerializeField] SkillPanel skillTree;
    [SerializeField] CopyField copy;
    [SerializeField] Button quit;
    [SerializeField] Menu menu;
    [SerializeField] Connector conn;
    [SerializeField] Animator anima;
    [SerializeField] UpperPanel uPl;
    [SerializeField] TMP_InputField chat;
    [SerializeField] AudioSource uiAudioSource;
    [SerializeField] AudioSource themeAudio;

    private bool paused;
    private bool chatting;
    private PlayerStats player;
    [SerializedDictionary("Name", "Field"), SerializeField]         SerializedDictionary<string, TMP_Text> textFields = new();      /*  OBSAH   
        {"maxHp",           -},
        {"curLevel",        -},
        {"pNmae",           -},
        {"versi",           -},
        {"compa",           -},
    */
    [SerializedDictionary("Name", "Objkt"), SerializeField]         SerializedDictionary<string, GameObject> uiPanels = new();          /*  OBSAH
        {"menuUI",          -},
        {"pauseUI",         -},
        {"chatUI",          -},
        {"invUI",           -},
        {"deathScreen",     -},

        {"playerUIface",    -},
        {"playerUIhpBar",   -},
        {"playerUIxpBar",   -},

        {"quitUI",          -}
    */
    [SerializedDictionary("Name", "input"), SerializeField]         SerializedDictionary<string, InputActionReference> inputs = new();  /*  OBSAH
        {"pause"},
        {"chat"},
        {"submit"},
        {"inventory"},
        {"equipment"},
        {"point"},
    */
    [HideInInspector] public bool playerLives = false;
    
    public event Action<Utility> UtilityUpdate;
    public Inventory inventory;
    public List<Damage.Type> defences = new();
    private List<Utility.Function> utilities = new();

#region Odkazovace
    private Vector2 RawMousePos => inputs["point"].action.ReadValue<Vector2>();
    /// <summary>
    /// Ziska polohu mysi relativnu voci stredu obrazovky
    /// </summary>
    /// <value>VEKTOR pozicie mysi</value>
    public Vector2 MousePos
    { 
        get 
        {
            Vector2 mouse = RawMousePos;
            Vector2 v= new(Screen.width/2, Screen.height/2);
            v= new(
                mouse.x - v.x, 
                mouse.y - v.y);
            return v;
        }
    }
    /// <summary>
    /// Ziska poziciu mysi voci lavemu dolnemu rohu obrazovky 
    /// </summary>
    public Vector2 CornerMousePos
    {
        get {
            Vector2 mouse = RawMousePos;
            //Vector2 modifier = new (1920f/Screen.width, 1080f/Screen.height);

            Vector2 v = new (Mathf.Clamp(mouse.x, 0, Screen.width), Mathf.Clamp(mouse.y, 0, Screen.height));
            //v *= modifier;

            FileManager.Log($"mouse [{mouse.x}, {mouse.y}] screen= [{Screen.width}, {Screen.height}] v [{v.x}, {v.y}]");
            return v;
        }
    }
    public Transform LocalDefence => uiPanels["playerDefence"].transform;
    public PlayerStats LocalPlayer  
    { 
        get => player; 
        set 
        {
            if (value != null)
            {
                SetMaxHp(value.MaxHP);
                skillTree.LevelUP(value.Level);
                textFields["curLevel"].text = value.Level.ToString();
            }

            player = value; 
        } 
    }
    public List<Transform> RemotePlayers= new();
    public SkillPanel SkillTree     { get => skillTree; }
    public bool PlayerAble          { get => !(paused || chatting || inventory.open); }
    public bool IsServer            { get { bool? b = conn.netMan?.IsServer; return b != null && b.Value; } }
    /// <summary>
    /// Vrati lokalny zobrazovac zivorov
    /// </summary>
    public Slider HpBar => uiPanels["playerUIhpBar"].GetComponent<Slider>();
    /// <summary>
    /// Vrati lokalny zobrazovac skusenosti
    /// </summary>
    public XpSliderScript XpBar => uiPanels["playerUIxpBar"].GetComponent<XpSliderScript>(); 

#endregion
#region SetUp
    /// <summary>
    /// Spudti sa raz po nacitani objektu
    /// </summary>
    void Awake()
    {
        instance = this;
        skillTree.Awake();
        SetUpTextFields();
        SubscribeInput();
        SetGameUI();
    }
    /// <summary>
    /// Nastavi hodnoty pre textove polia <br />
    /// (moznost prekladu UI)
    /// </summary>
    void SetUpTextFields()
    {
        textFields["pName"].text = Application.productName;
        textFields["versi"].text = "Version: " + Application.version;
        textFields["compa"].text = Application.companyName;
    }
    /// <summary>
    /// Nastavi metody na pocuvanie vstupov
    /// </summary>
    void SubscribeInput()
    {
        // Stanovuje vstupy pre "input system"
        inputs["pause"].action.started += OC_Pause;
        inputs["chat"].action.started += OpenChat;
        inputs["submit"].action.started += SendMess;

        quit.onClick.AddListener(delegate { Quit(); });
    }

#region Nastavovace
    /// <summary>
    /// Zapne/Vypne UI hraca 
    /// </summary>
    /// <param name="lives"></param>
    public void SetPlayerUI(bool lives = true)
    {
        uiPanels["deathScreen"].SetActive(!lives);

        uiPanels["playerUI"].SetActive(lives);
        
        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;

        uPl.Reset();
        if (inventory.open)
            inventory.OC_Inventory();
        inventory.ReloadAttacks();

        playerLives = lives;
    }
    /// <summary>
    /// Meni maximalne zivoty v invenetari
    /// </summary>
    /// <param name="maxHp"></param>
    public void SetMaxHp(int maxHp)
    {
        textFields["maxHp"].text = maxHp.ToString();
    }
    /// <summary>
    /// Nastavi ui hry
    /// </summary>
    /// <param name="active"></param>
    void SetGameUI(bool active = false)
    {
        uiPanels["deathScreen"].SetActive(false);
        uiPanels["playerUI"].SetActive(active);
        uiPanels["titles"].SetActive(false);

        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;        
    }
#endregion
#endregion
#region Udalosti
    /// <summary>
    /// Bezi na servery ak zomrie nepriatel
    /// </summary>
    void EnemySpawner()
    {
        while (MapScript.map.SpawnEnemies)
            //FileManager.Log($"Enemy spawing {MapScript.npCouter} < {MAX_NPC_COUNT}");
            MapScript.map.SpawnEnemy();
    }
    /// <summary>
    /// Spusta sa po pripojeni a vzniku hraca pariaceho lokalnemu pocitacu
    /// </summary>
    /// <param name="plStats"></param>
    public void PlayerSpawned(PlayerStats plStats)
    {
        LocalPlayer = plStats;
        if (player == null) 
            return;
        SetPlayerUI();
        AnimateFace(player.HP);

        if (!IsServer)
            StartGame();
    }
    /// <summary>
    /// Po ziskani dalsej urovne
    /// </summary>
    /// <param name="level"></param>
    public void LevelUP()
    {
        if (skillTree != null)
        {
            skillTree.LevelUP(LocalPlayer.Level);
            textFields["curLevel"].text = LocalPlayer.Level.ToString();
        }
    }
    /// <summary>
    /// Prida odomknutu schopnost
    /// </summary>
    /// <param name="utility"></param>
    public void AddUtility(Utility utility)
    {
        utilities.Add(utility.function);
        UtilityUpdate?.Invoke(utility);
    }

    public void AddResist(Damage.Type condition)
    {
        if (!defences.Contains(condition))
            defences.Add(condition);
    }
    /// <summary>
    /// Odstrani schopnost
    /// </summary>
    /// <param name="utility"></param>
    public void RemUtility(Utility utility)
    {
        utilities.Remove(utility.function);
        UtilityUpdate?.Invoke(new Utility (utility.name, utility.function));
    }
    /// <summary>
    /// Zisti ci je povolena schopnost
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public bool IsUtilityEnabled (Utility.Function f)
    {
        return utilities.Contains(f);
    }
    public void BossKilled()
    {
        anima.SetTrigger("titles");

        if (IsServer)
        {
            FileManager.NoMorePlaingWorld();   
        }
        uiPanels["titles"].SetActive(true);
        Quit(false);
    }
    public void AddRemotePlayer(Transform rPlayer)
    {
        RemotePlayers.Add(rPlayer);
    }
    public void RemoveRemotePlayer(Transform rPlayer)
    {
        RemotePlayers.Remove(rPlayer);
    }
#region UI_Vstupy
    /// <summary>
    /// Otvori/Zavrie pauzove menu
    /// </summary>
    /// <param name="context"></param>
    void OC_Pause(InputAction.CallbackContext context)
    {
        // ak je menu este otvorene
        if (menu.gameObject.activeSelf)
        {
            menu.Exit();
        }
        // ak sa hrac moze pohybovat
        else if (PlayerAble)     
        {
            Pause();
        }
        // ak ma hrac otvoreny cet
        else if (chatting)  
        {
            chatting = false;
            uiPanels["chatUI"].SetActive(chatting);
        }
        // ak je otvoreny inventar
        else if (inventory.open)    
        {
            inventory.OC_Inventory();
        }
        // ak hrac zije a hra je pozastavena
        else if (player.IsAlive.Value && paused)    
        {
            Pause();
        }
        // inak (hrac je mrtvy) 
        else    
            player.ReviveRpc();
    }
    /// <summary>
    /// Zobrazi ponuku pauzy <br />
    /// V hre pre jedneho hraca zastavi cas
    /// </summary>
    public void Pause()
    {
        paused = !paused;
        uiPanels["pauseUI"].SetActive(paused);
        
        // ak je solo pozastavi hru
        if (conn.Solo)
            Time.timeScale = paused ? 0 : 1;
    }
    /// <summary>
    /// Otvori pole na 
    /// </summary>
    /// <param name="context"></param>
    void OpenChat(InputAction.CallbackContext context)
    {
        if (!menu.gameObject.activeSelf && PlayerAble)
        {
            chatting = true;
            uiPanels["chatUI"].SetActive(chatting);
            chat.Select();
            chat.ActivateInputField();
        }
    }
    /// <summary>
    /// Odosle spravu do cetu
    /// </summary>
    /// <param name="context"></param>
    void SendMess(InputAction.CallbackContext context)
    {
        if (!menu.gameObject.activeSelf && chatting && player != null)
        {
            chatting = false;
            uiPanels["chatUI"].SetActive(chatting);
            string mess = chat.text.Trim()/*.Substring(0, 64)*/;
            if (chat.text.Trim() == "") return;
            player.message.Value = mess;
            chat.text = "";
        }
    }
    /// <summary>
    /// Odide z hry -> do hlavneho menu
    /// </summary>
    public void Quit(bool gameQuit = true)
    {
        if (IsServer)
            NPStats.npcDied -= EnemySpawner;

        if (gameQuit)
            QuitUI();

        themeAudio.Stop();
        Time.timeScale = 1;
        RemotePlayers.Clear();
        conn.Quit(player.OwnerClientId);
    }
    public void QuitUI()
    {
        GameQuit.Invoke();
    }
#endregion
#endregion
#region Spustenie Hry
    /// <summary>
    /// Nastavi zapnutie hry
    /// </summary>
    public void StartGame()
    {
        // Nastavi obsah kopirovacieho pola
        copy.SetUp(conn.GetConnection());

        if (IsServer)
        {
            FileManager.Log("NPCs setuping");
            // pre istovu iba ak je null v pripade viacnasobneho spustania hry
            NPStats.npcDied += EnemySpawner;

            MapScript.npCouter = 0;
            // Ak su ulzene nejake data nepriatelov
            if (0 < FileManager.World.entities.Count)
                foreach (var entity in FileManager.World.entities)
                    MapScript.map.SpawnFromSave(entity);
            else
                NPStats.npcDied.Invoke();
                
            FileManager.Log("Bos setuping");

            // Ak je svet bez hlavneho nepriatela tak ho vytvori
            if (FileManager.World.boss == null)
                MapScript.map.SpawnBoss();
            // Inak ho nacita zo suboru
            else// if (!FileManager.World.ended)
                MapScript.map.SpawnFromSave(FileManager.World.boss);
        }

        // vymaze listy
        utilities.Clear();
        defences.Clear();

        // Vymaze objekty obrany
        int n = LocalDefence.childCount-1;
        for (int i = n; 0 <= i; i--)
            Destroy(LocalDefence.GetChild(i).gameObject);

        menu.TiggerHideUI();
        FileManager.Log("Game started");
    }
    /// <summary>
    /// 
    /// </summary>
    public void GameStarted()
    {
        themeAudio.Play();
        SetGameUI(true);
    }
#endregion
#region Animacie Tvare

    // ANIMACIE POUZIVATELSKEHO ROZHRANIA //
    public void AnimateFace(float state)            => anima.SetFloat("faceState", Mathf.Floor(state*10)/10f);
    public void AnimateFace(string action)          => anima.SetTrigger(action);   
    public void AnimateUI(string name, float value) => anima.SetFloat(name, value);
    public void AnimateUI(string name, bool value)  => anima.SetBool(name,value);  
    public void AnimateUI(string name)              => anima.SetTrigger(name);

#endregion
}