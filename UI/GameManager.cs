using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    [SerializeField] GameObject playerUIface;
    [SerializeField] GameObject playerUIhpBar;
    [SerializeField] GameObject playerUIxpBar;
    [SerializeField] GameObject deathScreen;
    [SerializeField] Button copy;
    [SerializeField] Button inventBtn;
    [SerializeField] InputActionReference inputUIpause;
    [SerializeField] InputActionReference inputUIinventory;
    
    public Inventory inventory;

    private bool paused = false, inv = false;

    private string gameTag;
    void Start()
    {
        // Events
        inventBtn.onClick.AddListener(() => OC_Inventory(new()));
        inputUIinventory.action.started += OC_Inventory;
        copy.onClick.AddListener(Copy);
        inputUIpause.action.started += OC_Pause;

        deathScreen.SetActive(false);
        playerUI.SetActive(false);
        pauseUI.SetActive(false);
        invUI.SetActive(false);
        conUI.SetActive(true);
    }
    void OC_Inventory(InputAction.CallbackContext context)
    {
        inv = !inv;
        uiAnimator.SetBool("inv-open", inv);
    }
    void OC_Pause(InputAction.CallbackContext context)
    {
        //Debug.Log($"Pause {paused}");
        paused = !paused;
        pauseUI.SetActive(paused);
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void PlayerSpawned()
    {
        mainCam.gameObject.SetActive(false);
        playerUI.SetActive(true);
        invUI.SetActive(true);
    }
    public void PlayerDied(Vector2 pos)
    {
        mainCam.transform.position = new Vector3(pos.x, pos.y, mainCam.transform.position.z);
        mainCam.gameObject.SetActive(true);
        deathScreen.SetActive(true);
        playerUI.SetActive(false);
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