using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
/// Spravuje hlavnu ponuku
/// </summary>
public class Menu : MonoBehaviour
{
    public static Menu menu;
    [SerializeField] Connector conn;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Toggle lanToggle;
    [SerializeField] Toggle fullScToggle;
    [SerializeField] AudioSource meneTheme;
    [SerializeField] AudioSource ui_sfx;
    [SerializeField] QualityScript quality;
    [SerializeField] AudioMixer[] audioMixers;   
    [SerializedDictionary("Name", "Button"), SerializeField]
    private SerializedDictionary<string, MainUIButton> buttons = new();
    /* OBSAH
        {"exit",            - },
        {"solo",            - },
        {"multi",           - },
        {"sett",            - },

        {"soloCont",        - },
        {"soloLoad",        - },
        {"soloCrea",        - },

        {"multiJoin",       - },
        {"multiLoad",       - },
        {"multiCreate",     - },
        {"multiStart",      - },

        {"joinMultiJoin",   - }
    */
    [SerializedDictionary("Name", "InputField"), SerializeField]
    private SerializedDictionary<string, InputFieldCheck> inputFields = new();
     /* OBSAH
        {"playerName",  - },
        {"ipCode",      - }
    */
    [SerializedDictionary("Name", "TextField"), SerializeField]
    private SerializedDictionary<string, TMP_Text> textFields = new();
    /* OBSAH
        {"mSolo",               - },
        {"sTitle",              - },
        {"sContinue",           - },
        {"sLoad",               - }, 
        {"sCreate",             - }, 

        {"mMulti",              - }, 
        {"muTitle",             - }, 
        {"muPlayerNameTitle",   - },
        {"muPlayerNamePlace",   - },
        {"UserNameError",       - },
        {"muCreate",            - }, 
        {"muJoin",              - },

        {"joTitle",             - }, 
        {"joServerTitle",       - }
        {"joServerPlace",       - },
        {"IPCodeError",         - }, 
        {"joJoin",              - }, 

        {"mSett",               - },
        {"setTitle",            - }, 

        {"Exit",                - },

        {"VERSION",             - },
        {"COMPANY",             - }
    */
    [SerializedDictionary("Name", "GameObject"), SerializeField]
    private SerializedDictionary<string, GameObject> uis = new();
    /*  OBSAH
    
        {"BG"}
        {"PT"}
        {"PV"}
        {"PC"}
        {"EXIT"}
        {"MenuBase"}
        {"Main"}
        {"SubSolo"}
        {"SubMulti"}
        {"SubMultiJoin"}
        {"SubMultiStart"}
        {"SubSett"}
        {"SubLoad"}
    */
    Stack<string> currentLayer;
    bool goesUP; string disable;
    string lastConnection;
    GameType choosenGame;

    [HideInInspector] public string playerName;
    public string PlayerName 
    { 
        get => playerName;
        set
        {
            inputFields["playerN1"].Text = value;
            inputFields["playerN2"].Text = value;
            playerName = value;
        }
    }
    public bool OnlineGame { get => !lanToggle.isOn;  set => lanToggle.isOn = !value;   }
    public bool FullSc { get => fullScToggle.isOn;  set => fullScToggle.isOn = value;   }
    public int Quality { get => quality.Q;          set => quality.Q = value;           }
    public float[] Audios 
    {
        get 
        { 
            float[] f = new float[audioMixers.Length];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = audioMixers[i].SliderValue;
            }
            return f;
        }
        set
        {
            for (int i = 0; i < value.Length; i++)
            {
                audioMixers[i].SliderValue = value[i];
            }
        }
    }

    /// <summary>
    /// Zavola sa na zaciatku
    /// </summary>
    void Awake()
    {   
        SubscribeToButtons();
        ResetTextFields();
        menu = this;
    }
    /// <summary>
    /// Zavolane pri prvom povoleni objektu
    /// </summary>
    void Start() 
    {
        conn = Connector.instance;
    }
    /// <summary>
    /// Zapne sa pri povoleni objektu
    /// </summary>
    void OnEnable()
    {
        FileManager.Renew();
        meneTheme.Play();
        ResetUI();        
    }
    /// <summary>
    /// Sluzi pre stridanie ponuk - spusta nanimator
    /// </summary>
    void SwitchActiveMenu()
    {
        string last = goesUP ? currentLayer.Pop() : disable;

        uis[last].SetActive(false);
        uis[currentLayer.Peek()].SetActive(true);
        goesUP = false;
    }
    /// <summary>
    /// Resetuje nastavenie UI elementov, cize zapne vsetky zakladne a vypne vsetky podradene ponuky
    /// </summary>
    public void ResetUI()
    {
        cam.gameObject.SetActive(true);
        
        uis["BG"].SetActive(true);
        uis["PT"].SetActive(true);
        uis["PV"].SetActive(true);
        uis["PC"].SetActive(true);
        uis["EXIT"].SetActive(true);
        uis["MenuBase"].SetActive(true);              

        // zapne hlavnu ponuku
        uis["Main"].SetActive(true);
        currentLayer = new();
        currentLayer.Push("Main");

        // Vypne vsetky "sub" ponuky
        foreach(string key in uis.Keys.ToList().FindAll(k => k.Contains("Sub")))
            uis[key].SetActive(false);
        goesUP = false;
    }
    /// <summary>
    /// Vypne objekt hlavneho menu po postupnom ubudani pozadia
    /// </summary>
    public void TiggerHideUI() => animator.SetTrigger("change");
    /// <summary>
    /// Pri zapnuti hry vypne menu
    /// </summary>
    void HideUI()
    {
        meneTheme.Stop();
        animator.Rebind();
        gameObject.SetActive(false);
        FileManager.RegeneradeSettings();
        FileManager.Log($"Game Started => Menu Disabled => MenuTheme Stoped", FileLogType.RECORD);
    }
    /// <summary>
    /// Resetuje textove polia, <br />
    /// cize nastavy nazov hry, verziu a nazov firmy podla udajov v unity <br />
    /// zaroven resetuje chybove hlacenia v ramci textovych vystupovs
    /// </summary>
    void ResetTextFields()
    {
        textFields["TITLE"].text = Application.productName;
        textFields["VERSION"].text = "Version: " + Application.version;
        textFields["COMPANY"].text = Application.companyName;
    }
    /// <summary>
    /// Prida pocuvajuce metody na stlacenie tlacidiel podla slovnika "buttons"
    /// </summary>
    void SubscribeToButtons()
    {
        foreach (var b in buttons)
            b.Value.SetAudioSource(ui_sfx);

        buttons["exit"].AddListener(delegate { Exit();});
        buttons["solo"].AddListener(delegate { MenuNavigation(1); });
        buttons["multi"].AddListener(delegate { MenuNavigation(2); });
        buttons["sett"].AddListener(delegate { MenuNavigation(3); });

        buttons["soloCont"].AddListener(delegate { MenuNavigation(1, 1); });
        buttons["soloLoad"].AddListener(delegate { MenuNavigation(2, 1); });
        buttons["soloCreate"].AddListener(delegate { MenuNavigation(3, 1); });

        buttons["multiStart"].AddListener(delegate { MenuNavigation(1, 2); });
        buttons["multiJoin"].AddListener(delegate { MenuNavigation(2, 2); });

        buttons["multiLoad"].AddListener(delegate { MenuNavigation(1, 21); });
        buttons["multiCreate"].AddListener(delegate { MenuNavigation(2, 21); });

        buttons["joinMultiJoin"].AddListener(delegate {MenuNavigation(1, 22); });

        buttons["createWorld"].AddListener(delegate {MenuNavigation(0);});


        
        fullScToggle.onValueChanged.AddListener(
            delegate (bool on) 
            { 
                Screen.fullScreen = on; 
            }
        );        
    }
    /// <summary>
    /// Odide z vnorenej navigacie alebo zavire hru
    /// </summary>
    void Exit()
    {
        if (1 < currentLayer.Count)
        {
            // Posun sa na predchadzajucu vrstvu -> vypni sucasnu
            goesUP = true;
            animator.SetTrigger("change");

            // Ak odchadza z nastaveni ulozi ich
            if (currentLayer.Peek() == "SubSett")
                FileManager.RegeneradeSettings();
        }
        else
        {
            // Zavri hru
            Application.Quit();
            FileManager.Log("GAME CLOSED", FileLogType.RECORD);
        }
    }
    /// <summary>
    /// Spravuje navigaciu v hlavnom menu
    /// </summary>
    /// <param name="to">VOLBA podla tlacitka</param>
    void MenuNavigation(sbyte to, sbyte from = 0)
    {
        int layer = 10*from + to;
        disable = currentLayer.Peek();
        switch (layer)
        {
            // HLAVNE menu
            case 1: currentLayer.Push("SubSolo"); choosenGame = GameType.Solo; break;
            case 2: currentLayer.Push("SubMulti"); break;
            case 3: currentLayer.Push("SubSett"); break;

            // PODPONUKA pre JEDNEHO hraca                          (localhost)
            case 11: /* Pokracuje v poslednom svete */ 
                _ = FileManager.StartWorld(inputFields["worldName"].Text, choosenGame);
                layer= -1; 
                break;
            case 12: /* Nacita zo subora hru    */ currentLayer.Push("SubLoad"); break; 
            case 13: /* Vytvorit novu hru       */ currentLayer.Push("SubCreate"); break;

            // PODPONUKA pre VIAC hracov
            case 21: currentLayer.Push("SubMultiStart"); break;  // vnori sa do ponuky pre server
            case 22: currentLayer.Push("SubMultiJoin");  break;  // vnori sa do ponuky pre klienta

            // PODPONUKA pre ZALOZENIE hry pre VIAC hracov          (pre server)
            case 211: /* Nacitat zo subora hru   */ 
            case 212: /* Vytvorit novu hru      */ 
                choosenGame = !OnlineGame ? GameType.Online : GameType.Local;
                string next = layer == 211 ? "SubLoad" : "SubCreate";
                if (inputFields["playerN2"].Check) 
                    currentLayer.Push(next);
                else 
                    layer= 0;
                break;

            /// PODPONUKA pre VYTVORENIE sveta
            case 0: /* Vytvori svet */
                if (inputFields["worldName"].Check)
                {
                    _ = FileManager.StartWorld(inputFields["worldName"].Text, choosenGame);
                    layer= -1;
                }
                else
                    layer= 0;
                break;

            // PODPONUKA pre PRIPOJENIE sa ho hry pre VIAC hracov   (pre klienta)
            case 221: /* Pripoji sa do uz existujucej hry */
                if (inputFields["playerN1"].Check && inputFields["ipCode"].Check)
                {
                    _ = Connector.instance.JoinServer(inputFields["ipCode"].Text);
                    layer= -1;
                }
                else
                    layer= 0;
                break;

            case -1: 
                FileManager.Log("Hiding UI");
                break;
            default: 
                FileManager.Log("Bad layer [" + layer + "] on MenuNavigation!", FileLogType.WARNING); 
                break;
        }

        // v niektorych pripadoch v prepinaci je "layer= -1;" to znamena ze hrac chce vstupit do hry,
        // v inych je "layer= 0" to znamena ze sa nema prepinat ponuka lebo nebola spnena podmienka 
        // cize hlavne menu sa vypne
        if (layer < 0)
        {
            FileManager.Log($"Navigation is hiding");
            animator.SetTrigger("game");
        }
        else if (0 < layer)
        {
            FileManager.Log($"Navigation set to {currentLayer.Peek()}");
            animator.SetTrigger("change");
        }
    }
    /// <summary>
    /// Po stlaceni tlacidla pre nacitanie konkretneho suboru hry
    /// </summary>
    /// <param name="worldName"></param>
    public void PressLoad(string worldName)
    {
        _ = FileManager.StartWorld(worldName);
        MenuNavigation(-1);
    }
    /// <summary>
    /// Sluzi pre nacitanie hodnot z nastaveni
    /// </summary>
    /// <param name="settings">hodnoty NASTAVENI</param>
    public void LoadSettings(Settings settings)
    {
        Audios = settings.audioS;
        Quality= settings.quality;
        OnlineGame=settings.Online;
        FullSc = settings.fullSc;
        PlayerName = settings.playerName;
        lastConnection = settings.lastConnection;
        if (lastConnection != "")
        {
            bool solo = lastConnection.Contains("solo-");
            buttons["soloCont"].Interactable = solo;
            if (solo)
                inputFields["ipCode"].Text = lastConnection;
        }
    }
}
