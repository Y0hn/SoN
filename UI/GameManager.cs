using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
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
    private bool playerSpawned;
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

        playerSpawned = false;
        deathScreen.SetActive(playerSpawned);
        playerUI.SetActive(playerSpawned);
        equipUI.SetActive(playerSpawned);
        pauseUI.SetActive(playerSpawned);
        invUI.SetActive(playerSpawned);
        conUI.SetActive(true);
    }
    void OC_Inventory(InputAction.CallbackContext context)
    {
        if (!playerSpawned) return;
        inv = !inv;
        uiAnimator.SetBool("inv-open", inv);
        TMP_Text tmp = inventBtn.GetComponentInChildren<TMP_Text>();
        if (inv) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Equipment(InputAction.CallbackContext context)
    {
        if (!playerSpawned) return;
        equip = !equip;
        uiAnimator.SetBool("equ-open", equip);
        TMP_Text tmp = equipBtn.GetComponentInChildren<TMP_Text>();
        if (equip) tmp.text = ">";
        else tmp.text = "<";
    }
    void OC_Pause(InputAction.CallbackContext context)
    {
        if (!playerSpawned) return;
        //Debug.Log($"Pause {paused}");
        paused = !paused;
        pauseUI.SetActive(paused);
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void PlayerSpawned(PlayerStats plStats)
    {
        playerSpawned = true;
        player = plStats;
        player.IsAlive.OnValueChanged += (bool prev, bool newv) => { if(!newv) PD(); };
        mainCam.gameObject.SetActive(false);
        playerUI.SetActive(true);
        equipUI.SetActive(true);
        invUI.SetActive(true);
    }
    private void PD()
    {
        PlayerDied(player.transform.position);
    }
    public void PlayerDied(Vector2 pos)
    {
        mainCam.transform.position = new Vector3(pos.x, pos.y, mainCam.transform.position.z);
        mainCam.gameObject.SetActive(true);
        deathScreen.SetActive(true);
        playerUI.SetActive(false);
        equipUI.SetActive(false);
        invUI.SetActive(false);
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