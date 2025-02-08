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
    void Awake() => instance = this;
    public static GameManager instance;
    public static MenuScript UI         { get => instance.menu; }    
    [SerializeField] SkillPanel skillTree;
    [SerializeField] MenuScript menu;
    [SerializeField] Connector conn;
    [SerializeField] Animator anima;
    [SerializeField] UpperPanel uPl;
    private bool paused;
    private bool chatting;
    private PlayerStats player;
    [SerializedDictionary("Name", "buttn"), SerializeField]         SerializedDictionary<string, Button> buttons = new();           /*  OBSAH
        {"copy"},
        {"quit"},
    */
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
    [SerializedDictionary("Name", "Field"), SerializeField]         SerializedDictionary<string, TMP_InputField> inputFields = new();   /*  OBSAH   
        {"name"},
        {"chat"},
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
    public PlayerStats LocalPlayer  { get => player; } 
    public SkillPanel SkillTree     { get => skillTree; }
    public bool PlayerAble          { get => !(paused || chatting || inventory.open); }
    public string PlayerName        { get { return inputFields["name"].text.Trim(); } set { inputFields["name"].text = value; } }
    public bool IsServer            { get { bool? b = conn.netMan?.IsServer; return b != null && b.Value; } }
    void Start()
    {
        uiPanels["mainCam"].SetActive(true);
        SetUpTextFields();
        SubscribeInput();
        SetGameUI();
    }
    void Update()
    {
        if (IsServer && MapScript.map != null && NPStats.NPCount < MAX_NPC_COUNT)
            MapScript.map.SpawnEnemy();
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
        // Stanovuje vstupy pre input system
        inputs["pause"].action.started += OC_Pause;
        inputs["chat"].action.started += OpenChat;
        inputs["submit"].action.started += SendMess;

        buttons["copy"].onClick.AddListener(Copy);
        buttons["quit"].onClick.AddListener(Quit);
    }
    /// <summary>
    /// Otvori/Zavrie pauzove menu
    /// </summary>
    /// <param name="context"></param>
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (PlayerAble)
        {
            paused = !paused;
            uiPanels["pauseUI"].SetActive(paused);
        }
        else if (chatting)
        {
            chatting = false;
            uiPanels["chatUI"].SetActive(chatting);
        }
        else if (inventory.open)
        {
            inventory.OC_Inventory();
        }
        else if (player.IsAlive.Value && paused) 
        {
            paused = !paused;
            uiPanels["pauseUI"].SetActive(paused);
        }
        else
            player.GetComponent<PlayerController>().Fire(new());
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
            inputFields["chat"].Select();
            inputFields["chat"].ActivateInputField();
        }
    }
    void SendMess(InputAction.CallbackContext context)
    {
        if (chatting)
        {
            chatting = false;
            uiPanels["chatUI"].SetActive(chatting);
            string mess = inputFields["chat"].text.Trim()/*.Substring(0, 64)*/;
            if (inputFields["chat"].text.Trim() == "") return;
            player.message.Value = mess;
            inputFields["chat"].text = "";
        }
    }
    void Quit()
    {
        SetGameUI(false);
        UI.gameObject.SetActive(true);
        conn.Quit(player.OwnerClientId);
    }
    public Slider GetHpBar()
    {
        return uiPanels["playerUIhpBar"].GetComponent<Slider>();
    }
    public XpSliderScript GetXpBar()
    {
        return uiPanels["playerUIxpBar"].GetComponent<XpSliderScript>();
    } 
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        if (player == null) return;
        SetPlayerUI();
        AnimateFace(player.HP);
    }
    void SetGameUI(bool active = false)
    {
        uiPanels["deathScreen"].SetActive(false);
        uiPanels["playerUI"].SetActive(active);

        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;
        
        uiPanels["mainCam"].SetActive(!active);
    }
    
    public void LevelUP(byte level)
    {
        skillTree.LevelUP(level);
        //Debug.Log("Player leveled UP to " + level);
    }
    public void AddUtility(Utility utility)
    {
        utilities.Add(utility.function);
        UtilityUpdate?.Invoke(utility);
    }
    public void RemUtility(Utility utility)
    {
        utilities.Remove(utility.function);
        UtilityUpdate?.Invoke(new Utility (utility.name, utility.function));
    }
    public bool IsUtilityEnabled (Utility.Function f)
    {
        return utilities.Contains(f);
    }
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
        inventory.ReloadAttacks();

        playerLives = lives;
    }
    public void Copy()
    { 
        GUIUtility.systemCopyBuffer = conn.codeText.text; 
        anima.SetTrigger("copy"); 
    }
    

    // ANIMATION //
    public void AnimateFace(float state)            => anima.SetFloat("faceState", Mathf.Floor(state*10)/10f);
    public void AnimateFace(string action)          => anima.SetTrigger(action);   
    public void AnimateUI(string name, float value) => anima.SetFloat(name, value);
    public void AnimateUI(string name, bool value)  => anima.SetBool(name,value);  
    public void AnimateUI(string name)              => anima.SetTrigger(name);     
}