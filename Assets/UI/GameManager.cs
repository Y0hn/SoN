using AYellowpaper.SerializedCollections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
//using Random = UnityEngine.Random;

/// <summary>
/// Managing Game and PLayerUI - has 'instance'
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;  
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
    [SerializedDictionary("Name", "Field"), SerializeField]         SerializedDictionary<string, TMP_Text> textFields = new();      /*  OBSAH   */
    [SerializedDictionary("Name", "Objkt"), SerializeField]         SerializedDictionary<string, GameObject> uiPanels = new();          /*  OBSAH
        {"mainCam",         -},
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
    [HideInInspector] public bool playerLives;
    /*[SerializedDictionary("Utility", "Aquired")]public SerializedDictionary*/ 
    
    public event Action<Utility> UtilityUpdate;
    public Inventory inventory;
    private List<Utility.Function> utilities = new();
    private const byte MAX_NPC_COUNT = 25;
    /// <summary>
    /// Ziska polohu mysi relativnu voci stredu obrazovky
    /// </summary>
    /// <value>VEKTOR pozicie mysi</value>
    public Vector2 MousePos
    { 
        get 
        {
            Vector2 mouse = inputs["point"].action.ReadValue<Vector2>();
            Vector2 v= new(Screen.width/2, Screen.height/2);
            v= new(
                mouse.x - v.x, 
                mouse.y - v.y);
            return v;
        }
    }
    public PlayerStats LocalPlayer  { get => player; set => player = value; } 
    public SkillPanel SkillTree     { get => skillTree; }
    public bool PlayerAble          { get => !(paused || chatting || inventory.open); }
    public bool IsServer            { get { bool? b = conn.netMan?.IsServer; return b != null && b.Value; } }
    /// <summary>
    /// Spudti sa raz po nacitani objektu
    /// </summary>
    void Awake()
    {
        instance = this;
        //uiPanels["mainCam"].SetActive(true);
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

        quit.onClick.AddListener(Quit);
    }
    /// <summary>
    /// Otvori/Zavrie pauzove menu
    /// </summary>
    /// <param name="context"></param>
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (PlayerAble)     // ak sa hrac moze pohybovat
        {
            paused = !paused;
            uiPanels["pauseUI"].SetActive(paused);
        }
        else if (chatting)  // ak ma hrac otvoreny cet
        {
            chatting = false;
            uiPanels["chatUI"].SetActive(chatting);
        }
        else if (inventory.open)    // ak je otvoreny inventar
        {
            inventory.OC_Inventory();
        }
        else if (player.IsAlive.Value && paused)    // ak hrac zije a hra je pozastavena
        {
            paused = !paused;
            uiPanels["pauseUI"].SetActive(paused);
        }
        else    // inak (hrac je mrtvy) 
            player.ReviveRpc();
    }
    /// <summary>
    /// Otvori pole na 
    /// </summary>
    /// <param name="context"></param>
    void OpenChat(InputAction.CallbackContext context)
    {
        if (PlayerAble)
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
        if (chatting && player != null)
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

            // Ak su ulzene nejake data nepriatelov
            if (0 < FileManager.World.entities.Count)
                foreach (var entity in FileManager.World.entities)
                    MapScript.map.SpawnFromSave(entity);
            else
            {
                MapScript.npCouter = 0;
                NPStats.npcDied.Invoke();
            }
                
            FileManager.Log("Bos setuping");

            // Ak je svet bez hlavneho nepriatela tak ho vytvori
            if (FileManager.World.boss == null)
                MapScript.map.SpawnBoss();
            // Inak ho nacita zo suboru
            else
                MapScript.map.SpawnFromSave(FileManager.World.boss);
        } 
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
    /// <summary>
    /// Odide z hry -> do hlavneho menu
    /// </summary>
    public void Quit()
    {
        if (IsServer)
            NPStats.npcDied -= EnemySpawner;

        themeAudio.Stop();
        inventory.Clear();
        Menu.menu.gameObject.SetActive(true);
        conn.Quit(player.OwnerClientId);
    }
    /// <summary>
    /// Bezi na servery ak zomrie nepriatel
    /// </summary>
    void EnemySpawner()
    {
        while (MapScript.npCouter < MAX_NPC_COUNT)
            //FileManager.Log($"Enemy spawing {MapScript.npCouter} < {MAX_NPC_COUNT}");
            MapScript.map.SpawnEnemy();
    }
    /// <summary>
    /// Vrati lokalny zobrazovac zivorov
    /// </summary>
    /// <returns>POSUVNIK zivotov</returns>
    public Slider GetHpBar()
    {
        return uiPanels["playerUIhpBar"].GetComponent<Slider>();
    }
    /// <summary>
    /// Vrati lokalny zobrazovac skusenosti
    /// </summary>
    /// <returns>POSUVNIK skosenosti</returns>
    public XpSliderScript GetXpBar()
    {
        return uiPanels["playerUIxpBar"].GetComponent<XpSliderScript>();
    } 
    /// <summary>
    /// Spusta sa po pripojeni a vzniku hraca pariaceho lokalnemu pocitacu
    /// </summary>
    /// <param name="plStats"></param>
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        if (player == null) return;
        SetPlayerUI();
        AnimateFace(player.HP);

        if (!IsServer)
            StartGame();
    }
    /// <summary>
    /// Nastavi 
    /// </summary>
    /// <param name="active"></param>
    void SetGameUI(bool active = false)
    {
        uiPanels["deathScreen"].SetActive(false);
        uiPanels["playerUI"].SetActive(active);

        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;
        
        //uiPanels["mainCam"].SetActive(!active);
    }
    /// <summary>
    /// Po ziskani dalsej urovne
    /// </summary>
    /// <param name="level"></param>
    public void LevelUP(byte level)
    {
        skillTree.LevelUP(level);
        //Debug.Log("Player leveled UP to " + level);
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
    /// <summary>
    /// Zapne/Vypne UI hraca 
    /// </summary>
    /// <param name="lives"></param>
    public void SetPlayerUI(bool lives = true)
    {
        if (!lives)
        //  Death Screen ☠︎︎
            uiPanels["mainCam"].transform.position = new Vector3
            (
                player.transform.position.x, 
                player.transform.position.y, 
                uiPanels["mainCam"].transform.position.z
            );

        uiPanels["deathScreen"].SetActive(!lives);
        uiPanels["mainCam"].SetActive(!lives);

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

    // ANIMACIE POUZIVATELSKEHO ROZHRANIA //
    public void AnimateFace(float state)            => anima.SetFloat("faceState", Mathf.Floor(state*10)/10f);
    public void AnimateFace(string action)          => anima.SetTrigger(action);   
    public void AnimateUI(string name, float value) => anima.SetFloat(name, value);
    public void AnimateUI(string name, bool value)  => anima.SetBool(name,value);  
    public void AnimateUI(string name)              => anima.SetTrigger(name);
}