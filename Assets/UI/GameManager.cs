using AYellowpaper.SerializedCollections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.Netcode;
/// <summary>
/// Managing Game and PLayerUI - has 'instance'
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    void Awake()    { instance = this; }
    [SerializeField] Connector conn;
    [SerializeField] MenuScript menu;
    [SerializeField] MenuScript menuScript;
    [SerializeField] Animator animatorGameUI;
    public Inventory inventory;
    [HideInInspector] public bool playerLives;
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
    public bool PlayerAble          { get => !(paused || chatting || inventory.open); }
    public string PlayerName        { get { return inputFields["name"].text.Trim(); } set { inputFields["name"].text = value; } }
    public static MenuScript UI     { get => instance.menuScript; }
    public static bool IsServer     { get; private set; }
#pragma warning disable IDE0051 // Remove unused private members
    void Start()
    {
        IsServer = NetworkManager.Singleton.IsServer;
        uiPanels["mainCam"].SetActive(true);
        SetUpTextFields();
        SubscribeInput();
        SetGameUI();
        FileManager.LoadSettings();
    }
#pragma warning restore IDE0051 // Remove unused private members
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
    public Slider GetBar(string bar)
    {
        switch (bar)
        {
            case "xp":      return uiPanels["playerUIxpBar"].GetComponent<Slider>();
            case "hp":
            case "health":
            default:        return uiPanels["playerUIhpBar"].GetComponent<Slider>();
        }
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
        
        uiPanels["invUI"].SetActive(active);

        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;

        menu.SetUpUI(!active);
        uiPanels["mainCam"].SetActive(!active);
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
        uiPanels["invUI"].SetActive(lives);
        
        uiPanels["pauseUI"].SetActive(false);
        paused = false;
        uiPanels["chatUI"].SetActive(false);
        chatting = false;

        //uiPanels["menuUI"].SetActive(false);
        menu.SetUpUI(false);
        inventory.ReloadAttacks();

        playerLives = lives;
    }
    public void Copy()      { GUIUtility.systemCopyBuffer = conn.codeText.text; animatorGameUI.SetTrigger("copy"); }
    public void AnimateFace(float state)            { animatorGameUI.SetFloat("faceState", state);  }
    public void AnimateFace(string action)          { animatorGameUI.SetTrigger(action);            }
    public void AnimateUI(string name, float value) { animatorGameUI.SetFloat(name, value);         }
    public void AnimateUI(string name, bool value)  { animatorGameUI.SetBool(name,value);           }
    public void AnimateUI(string name)              { animatorGameUI.SetTrigger(name);              }
}