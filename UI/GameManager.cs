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
    void Awake()
    { instance = this; }
    public static GameManager instance;
    [SerializeField] ConnectionManager connectionManager;

    [SerializedDictionary("Name", "GameObject"), SerializeField]
    protected SerializedDictionary<string, GameObject> UIs = new();
    /*
        {"mainCam", null},
        {"mainUI", null},
        {"conUI", null},
        {"pauseUI", null},
        {"playerUI", null},

        {"invUI", null},
        {"playerUIface", null},
        {"playerUIhpBar", null},
        {"playerUIxpBar", null},
        {"deathScreen", null},
    */
    [SerializeField] Animator animatorGameUI;
    [SerializeField] Animator animatorMenuUI;
    [SerializeField] Button copy;
    [SerializeField] Button inventBtn;
    [SerializeField] Button equipBtn;
    [SerializeField] InputActionReference inputUIpause;
    [SerializeField] InputActionReference inputUIinventory;
    [SerializeField] InputActionReference inputUIequipment;
    [SerializeField] InputActionReference submit;
    [SerializeField] TMP_InputField nameTag;
    [SerializeField] TMP_Text nameTagPlaceHolder;
    public Inventory inventory;

    private bool paused = false, inv = false, equip = false, inMenu = true;
    private PlayerStats player;
    void Start()
    {
        // Events
        inventBtn.onClick.AddListener(() => OC_Inventory(new()));
        inputUIinventory.action.started += OC_Inventory;

        equipBtn.onClick.AddListener(() => OC_Equipment(new()));
        inputUIequipment.action.started += OC_Equipment;

        inputUIpause.action.started += OC_Pause;
        submit.action.started += InputNameTag;

        copy.onClick.AddListener(Copy);

        SetStartUI();
    }
    /*    
          _____     __   ___        _____                                 __     _                   
          \_   \ /\ \ \ / _ \ /\ /\/__   \   ___   _ __    ___  _ __     / /___ | |  ___   ___   ___ 
           / /\//  \/ // /_)// / \ \ / /\/  / _ \ | '_ \  / _ \| '_ \   / // __|| | / _ \ / __| / _ \
        /\/ /_ / /\  // ___/ \ \_/ // /    | (_) || |_) ||  __/| | | | / /| (__ | || (_) |\__ \|  __/
        \____/ \_\ \/ \/      \___/ \/      \___/ | .__/  \___||_| |_|/_/  \___||_| \___/ |___/ \___|
                                                  |_|                                                                                                                  
     */
#region INPUT open/close
    void OC_Inventory(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) return;
        inv = !inv;
        animatorGameUI.SetBool("inv-open", inv);
        TMP_Text tmp = inventBtn.GetComponentInChildren<TMP_Text>();
        if (inv) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Equipment(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) return;
        equip = !equip;
        animatorGameUI.SetBool("equ-open", equip);
        TMP_Text tmp = equipBtn.GetComponentInChildren<TMP_Text>();
        if (equip) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) 
        {
            player.GetComponent<PlayerController>().Fire(new());
            return;
        }
        //Debug.Log($"Pause {paused}");
        paused = !paused;
        UIs["pauseUI"].SetActive(paused);
    }
    void InputNameTag(InputAction.CallbackContext context)
    {
        if (nameTag.text.Length > 1)
        {
            animatorMenuUI.SetBool("next", true);
        }
        else
        {
            nameTag.text = "";
            nameTagPlaceHolder.text = "Name must be longer";
        }
    }
#endregion
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
    public string GetPlayerName()
    {
        return nameTag.text;
    }
    public void Copy() { GUIUtility.systemCopyBuffer = connectionManager.codeText.text; animatorGameUI.SetTrigger("copy"); }
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        SetPlayerUI();
    }
    /*    
         /$$   /$$ /$$$$$$
        | $$  | $$|_  $$_/
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        |  $$$$$$/ /$$$$$$
         \______/ |______/
    */
    void SetStartUI()
    {
        UIs["deathScreen"].SetActive(false);
        UIs["playerUI"].SetActive(false);
        UIs["equipUI"].SetActive(false);
        UIs["pauseUI"].SetActive(false);
        UIs["invUI"].SetActive(false);

        UIs["conUI"].SetActive(false);
        UIs["menuUI"].SetActive(true);
    }
    public void SetPlayerUI(bool lives = true)
    {
        // Death Screen ☠︎︎
        if (!lives)
            UIs["mainCam"].transform.position = new
            (
                player.transform.position.x, player.transform.position.y, UIs["mainCam"].transform.position.z
            );
        UIs["deathScreen"].SetActive(!lives);
        UIs["mainCam"].SetActive(!lives);

        UIs["playerUI"].SetActive(lives);
        UIs["equipUI"].SetActive(lives);
        UIs["invUI"].SetActive(lives);

        UIs["pauseUI"].SetActive(false);

        animatorMenuUI.enabled = false;
        UIs["menuUI"].SetActive(false);
        UIs["conUI"].SetActive(false);
    }
    public void AnimateFace(float state)
    {
        animatorGameUI.SetFloat("state", state);
    }
    public void AnimateFace(string action)
    {
        animatorGameUI.SetTrigger(action);
    }
}