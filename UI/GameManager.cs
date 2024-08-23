using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    void Awake()
    { instance = this; }
    public static GameManager instance;
    [SerializeField] ConnectionManager connectionManager;
    [SerializeField] GameObject conUI;
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject playerUI;
    [SerializeField] GameObject playerUIface;
    [SerializeField] GameObject playerUIhpBar;
    [SerializeField] Button copy;
    [SerializeField] InputActionReference inputUI;
    [SerializeField] InputActionReference inputPlayer;
    private bool paused;
    private string gameTag;
    void Start()
    {
        inputUI.action.performed += Pause;
        copy.onClick.AddListener(Copy);
        playerUI.SetActive(false);
        pauseUI.SetActive(false);
        conUI.SetActive(true);
        paused = false;
    }
    void Pause(InputAction.CallbackContext context)
    {
        Debug.Log($"Pause {paused}");
        paused = !paused;
        pauseUI.SetActive(paused);
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }
    public void PlayerSpawned()
    {
        playerUI.SetActive(true);
    }
    public Slider GetPlayerHpBar()
    {
        return playerUIhpBar.GetComponent<Slider>();
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