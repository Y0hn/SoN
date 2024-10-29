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
        {"chatUI", null}
    */
    [SerializeField] Animator animatorGameUI;
    [SerializeField] Animator animatorMenuUI;
    [SerializeField] Button copy;
    [SerializeField] InputActionReference inputUIpause;
    [SerializeField] InputActionReference inputUIequipment;
    [SerializeField] InputActionReference submit;
    [SerializeField] TMP_InputField nameTag;
    [SerializeField] TMP_Text nameTagPlaceHolder;
    public Inventory inventory;
    public string PlayerName { get { return nameTag.text.Trim(); } }
    public bool playerLives;
    private bool paused = false;
    private PlayerStats player;
    void Start()
    {
        // Events
        inputUIpause.action.started += OC_Pause;
        submit.action.started += InputNameTag;

        copy.onClick.AddListener(Copy);

        SetStartUI();
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
    public void Copy() { GUIUtility.systemCopyBuffer = connectionManager.codeText.text; animatorGameUI.SetTrigger("copy"); }
    public void PlayerSpawned(PlayerStats plStats)
    {
        player = plStats;
        SetPlayerUI();
    }
#region INPUT open/close
    /*void OC_Pause() { OC_Pause(new()); }*/
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
    
#region UI
    /*    
         /$$   /$$ /$$$$$$
        | $$  | $$|_  $$_/
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        | $$  | $$  | $$  
        |  $$$$$$/ /$$$$$$
        \______/  |______/
    */
    void SetStartUI()
    {
        UIs["deathScreen"].SetActive(false);
        UIs["playerUI"].SetActive(false);
        //UIs["equipUI"].SetActive(false);
        UIs["pauseUI"].SetActive(false);
        UIs["invUI"].SetActive(false);

        UIs["chatUI"].SetActive(false);

        //UIs["conUI"].SetActive(false);
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
        //UIs["equipUI"].SetActive(lives);
        UIs["invUI"].SetActive(lives);
        
        UIs["pauseUI"].SetActive(false);
        UIs["chatUI"].SetActive(true);

        animatorMenuUI.enabled = false;
        UIs["menuUI"].SetActive(false);
        //UIs["conUI"].SetActive(false);

        playerLives = lives;
    }
    public void AnimateFace(float state)
    {
        animatorGameUI.SetFloat("state", state);
    }
    public void AnimateFace(string action)
    {
        animatorGameUI.SetTrigger(action);
    }
#endregion
}