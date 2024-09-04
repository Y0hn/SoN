using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject mainUI;
    [SerializeField] Animator uiAnimator;
    [SerializeField] GameObject conUI;
    [SerializeField] GameObject pauseUI; 
    [SerializeField] GameObject playerUI; 
    [SerializeField] GameObject invUI;
    [SerializeField] GameObject equipUI;
    [SerializeField] GameObject playerUIface;
    [SerializeField] GameObject playerUIhpBar;
    [SerializeField] GameObject playerUIxpBar;
    [SerializeField] GameObject deathScreen;
    [SerializeField] Button copy;
    [SerializeField] Button inventBtn;
    [SerializeField] Button equipBtn;
    [SerializeField] InputActionReference inputUIpause;
    [SerializeField] InputActionReference inputUIinventory;
    [SerializeField] InputActionReference inputUIequipment;
    
    public Inventory inventory;

    private bool paused = false, inv = false, equip = false;
    private string gameTag;
    private PlayerStats player;
    void Start()
    {
        // Events
        inventBtn.onClick.AddListener(() => OC_Inventory(new()));
        inputUIinventory.action.started += OC_Inventory;

        equipBtn.onClick.AddListener(() => OC_Equipment(new()));
        inputUIequipment.action.started += OC_Equipment;

        inputUIpause.action.started += OC_Pause;

        copy.onClick.AddListener(Copy);

        SetStartUI();
    }
    void SetStartUI()
    {
        deathScreen.SetActive(false);
        playerUI.SetActive(false);
        equipUI.SetActive(false);
        pauseUI.SetActive(false);
        invUI.SetActive(false);
        conUI.SetActive(true);
    }
    public void SetPlayerUI(bool lives = true)
    {
        // Death Screen ☠︎︎
        if (!lives)
            mainCam.transform.position = new
            (
                player.transform.position.x, player.transform.position.y, mainCam.transform.position.z
            );
        deathScreen.SetActive(!lives);
        mainCam.SetActive(!lives);

        playerUI.SetActive(lives);
        pauseUI.SetActive(false);
        equipUI.SetActive(lives);
        invUI.SetActive(lives);
        conUI.SetActive(false);
    }
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        SetPlayerUI();
    }
    void OC_Inventory(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) return;
        inv = !inv;
        uiAnimator.SetBool("inv-open", inv);
        TMP_Text tmp = inventBtn.GetComponentInChildren<TMP_Text>();
        if (inv) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Equipment(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) return;
        equip = !equip;
        uiAnimator.SetBool("equ-open", equip);
        TMP_Text tmp = equipBtn.GetComponentInChildren<TMP_Text>();
        if (equip) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (!player.IsAlive.Value) 
        {
            player.GetComponent<PlayerControler>().Fire(new());
            return;
        }
        //Debug.Log($"Pause {paused}");
        paused = !paused;
        pauseUI.SetActive(paused);
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public Slider GetBar(string bar)
    {
        switch (bar)
        {
            case "xp":      return playerUIxpBar.GetComponent<Slider>();
            case "hp":
            case "health":
            default:        return playerUIhpBar.GetComponent<Slider>();
        }
    }
    public string GetPlayerName()
    {
        switch (Random.Range(0, 5))
        {
            case 0: gameTag = "Toby"; break;
            case 1: gameTag = "Markuz"; break;
            case 2: gameTag = "Hugo"; break;
            case 3: gameTag = "xX_Legend_Xx"; break;
            case 4: gameTag = "Jerry"; break;
        }
        return gameTag;
    }
    public void Copy()
    {
        GUIUtility.systemCopyBuffer = connectionManager.codeText.text;
    }
}