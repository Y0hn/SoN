using AYellowpaper.SerializedCollections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
/// <summary>
/// Managing Game and PLayerUI - has 'instance'
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    void Awake()    { instance = this; }
    [SerializeField] ConnectionManager connectionManager;

    [SerializedDictionary("Name", "GameObject"), SerializeField]
    protected SerializedDictionary<string, GameObject> UIs = new();
    /*  OBSAH
        {"mainCam",         -},
        {"gameUI",          -},
        {"pauseUI",         -},
        {"playerUI",        -},

        {"invUI",           -},
        {"playerUIface",    -},
        {"playerUIhpBar",   -},
        {"playerUIxpBar",   -},
        {"deathScreen",     -},
        {"chatUI",          -}
    */

    [SerializedDictionary("Name", "input"), SerializeField]
    private SerializedDictionary<string, InputActionReference> inputs = new();

    [SerializedDictionary("Name", "inputField"), SerializeField]
    private SerializedDictionary<string, TMP_InputField> inputFields = new();

    [SerializeField] Animator animatorGameUI;
    [SerializeField] Button copy;
    public Inventory inventory;
    public string PlayerName    { get { return inputFields["name"].text.Trim(); } }
    public bool playerLives;
    public bool PlayerAble      { get { return !(paused || chatting); } }
    private bool paused;
    public bool chatting        { get; set; }
    private PlayerStats player;
    
    void Start()
    {
        SubscribeInput();
        SetStartUI();
    }
    void SubscribeInput()
    {
        inputs["pause"].action.started += OC_Pause;
        inputs["chat"].action.started += OpenChat;
        inputs["submit"].action.started += SendMess;

        copy.onClick.AddListener(Copy);
    }
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (player.IsAlive.Value) 
        {
            paused = !paused;
            UIs["pauseUI"].SetActive(paused);
        }
        else
            player.GetComponent<PlayerController>().Fire(new());
    }
    void OpenChat(InputAction.CallbackContext context)
    {
        chatting = true;
        UIs["chatUI"].SetActive(true);
        inputFields["chat"].Select();
        inputFields["chat"].ActivateInputField();
    }
    void SendMess(InputAction.CallbackContext context)
    {
        if (chatting)
        {
            chatting = false;
            UIs["chatUI"].SetActive(chatting);
            string mess = inputFields["chat"].text.Trim()/*.Substring(0, 64)*/;
            if (inputFields["chat"].text.Trim() == "") return;
            player.SendMessageServerRpc(mess);
            inputFields["chat"].text = "";
        }
    }

    public Slider GetBar(string bar)
    {
        switch (bar)
        {
            case "xp":      return UIs["playerUIxpBar"].GetComponent<Slider>();
            case "hp":
            case "health":
            default:        return UIs["playerUIhpBar"].GetComponent<Slider>();
        }
    }
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        SetPlayerUI();
    }
    void SetStartUI()
    {
        UIs["deathScreen"].SetActive(false);
        UIs["playerUI"].SetActive(false);
        
        UIs["invUI"].SetActive(false);

        UIs["pauseUI"].SetActive(false);
        paused = false;
        UIs["chatUI"].SetActive(false);
        chatting = false;

        UIs["menuUI"].SetActive(true);
    }
    public void SetPlayerUI(bool lives = true)
    {
        if (!lives)
        //  Death Screen ☠︎︎
            UIs["mainCam"].transform.position = new Vector3
            (
                player.transform.position.x, 
                player.transform.position.y, 
                UIs["mainCam"].transform.position.z
            );

        UIs["deathScreen"].SetActive(!lives);
        UIs["mainCam"].SetActive(!lives);

        UIs["playerUI"].SetActive(lives);
        UIs["invUI"].SetActive(lives);
        
        UIs["pauseUI"].SetActive(false);
        paused = false;
        UIs["chatUI"].SetActive(false);
        chatting = false;

        UIs["menuUI"].SetActive(false);
        UIs["gameUI"].SetActive(true);

        playerLives = lives;
    }

    public void Copy() { GUIUtility.systemCopyBuffer = connectionManager.codeText.text; animatorGameUI.SetTrigger("copy"); }
    public void AnimateFace(float state)    { animatorGameUI.SetFloat("state", state);  }
    public void AnimateFace(string action)  { animatorGameUI.SetTrigger(action);        }
}