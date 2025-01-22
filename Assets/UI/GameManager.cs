using AYellowpaper.SerializedCollections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System;
/// <summary>
/// Managing Game and PLayerUI - has 'instance'
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    /*public static async Awaitable<GameManager> GetGameManager()
    {
        //await new WaitUntil(() => weaponSelected == true);
        while (instance != null);
        return instance;
    }*/
    void Awake()    { instance = this; }
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
    Dictionary<SkillTree.Utility.Function, bool> Utils = new();
    /*[SerializedDictionary("Utility", "Aquired")]public SerializedDictionary*/ 
    public event Action<UtilitySkill> UtilityUpdate;
    public Inventory inventory;
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
    public static MenuScript UI     { get => instance.menu; }
//#pragma warning disable IDE0051 // Remove unused private members
    void Start()
    {
        uiPanels["mainCam"].SetActive(true);
        SetUpTextFields();
        SubscribeInput();
        SetUpUtility();
        SetGameUI();
        
        FileManager.LoadSettings();
    }
    float timer = 0;
    void Update()
    {
        if (IsServer && Time.time > timer)
        {
            MapScript.map.SpawnEnemy();
            timer = Time.time + 1;
        }
    }
//#pragma warning restore IDE0051 // Remove unused private members
    void SetUpTextFields()
    {
        textFields["pName"].text = Application.productName;
        textFields["versi"].text = "Version: " + Application.version;
        textFields["compa"].text = Application.companyName;
    }
    void SubscribeInput()
    {
        // Stanovuje vstupy pre input system
        inputs["pause"].action.started += OC_Pause;
        inputs["chat"].action.started += OpenChat;
        inputs["submit"].action.started += SendMess;

        buttons["copy"].onClick.AddListener(Copy);
        buttons["quit"].onClick.AddListener(Quit);
    }
    void SetUpUtility()
    {
        foreach (SkillTree.Utility.Function uti in Enum.GetValues(typeof(SkillTree.Utility.Function)))
        {
            EnableUtility(new (uti));
        }
    }
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

        menu.SetUpUI(!active);
        uiPanels["mainCam"].SetActive(!active);
    }
    
    public void LevelUP(byte level)
    {
        skillTree.LevelUP(level);
        Debug.Log("Player leveled UP to " + level);
    }
    public void EnableUtility(UtilitySkill utility)
    {
        if (Utils.ContainsKey(utility.function))
            Utils[utility.function] = utility.aquired;
        else
            Utils.Add(utility.function, utility.aquired);

        UtilityUpdate?.Invoke(utility);
    }
    public bool IsUtilityEnabled (SkillTree.Utility.Function f)
    {
        bool b = false;
        if (!Utils.ContainsKey(f))
            Utils.Add(f, b);
        return Utils[f];
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

        //uiPanels["menuUI"].SetActive(false);
        uPl.Reset();
        menu.SetUpUI(false);
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