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
    [SerializeField] Button copy;
    [SerializeField] InputActionReference inputUI;
    [SerializeField] InputActionReference inputPlayer;
    private bool paused;
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
    public void Copy()
    {
        GUIUtility.systemCopyBuffer = connectionManager.codeText.text;
    }
}
