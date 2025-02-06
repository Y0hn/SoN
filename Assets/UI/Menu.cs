using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
/// <summary>
/// Spravuje hlavne menu
/// </summary>
public class MenuScript : MonoBehaviour
{
    int navLayer;
    [SerializeField] Connector conn;
    [SerializeField] Animator animator;
    [SerializeField] Toggle onlineToggle;
    [SerializeField] Toggle fullScToggle;
    [SerializeField] AudioSource meneTheme;
    //[SerializeField] AudioSource pageChange;
    [SerializeField] QualityScript quality;
    [SerializeField] AudioMixer[] audioMixers;
    public bool Online { get => onlineToggle.isOn;  set => onlineToggle.isOn = value;   }
    public bool FullSc { get => fullScToggle.isOn;  set => fullScToggle.isOn = value;   }
    public int Quality { get => quality.Q;          set => quality.Q = value;           }
    public string PlayerName { get => inputFields["playerName"].text; set => inputFields["playerName"].text = value; }
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
    [SerializedDictionary("Name", "Button"), SerializeField]
    private SerializedDictionary<string, Button> buttons = new();
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
    private SerializedDictionary<string, TMP_InputField> inputFields = new();
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
    
    /// <summary>
    /// Zavola sa na zaciatku
    /// </summary>
    void Start()
    {
        
        conn = Connector.instance;
        SubscribeToButtons();
        SetTextFields();
        SetUpUI();
        
        FileManager.LoadSettings();
        meneTheme.Play();
    }
    /// <summary>
    /// Nastavi aktivne UI elementy
    /// </summary>
    /// <param name="active"></param>
    public void SetUpUI(bool active = true)
    {
        uis["BG"].SetActive(active);
        uis["PT"].SetActive(active);
        uis["PV"].SetActive(active);
        uis["PC"].SetActive(active);
        uis["EXIT"].SetActive(active);
        uis["MenuBase"].SetActive(active);
        // if (active)
        uis["Main"].SetActive(true);
        uis["SubSolo"].SetActive(false);
        uis["SubMulti"].SetActive(false);
        uis["SubMultiJoin"].SetActive(false);
        uis["SubMultiStart"].SetActive(false);
        uis["SubSett"].SetActive(false);
        uis["SubLoad"].SetActive(false);
        
        MainMenuNav(0);
        if (!active)
            animator.SetTrigger("reset");

        fullScToggle.onValueChanged.AddListener(
            delegate (bool on) 
            { 
                Screen.fullScreen = on; 
            }
        );

        
    }
    /// <summary>
    /// Nastavi textove polia
    /// </summary>
    void SetTextFields()
    {
        textFields["TITLE"].text = Application.productName;
        textFields["VERSION"].text = "Version: " + Application.version;
        textFields["COMPANY"].text = Application.companyName;

        textFields["IPCodeError"].text = "";
        textFields["UserNameError"].text = "";
    }
    /// <summary>
    /// Prida posluchcov na stlacenie tlacidiel podla slovnika "buttons"
    /// </summary>
    void SubscribeToButtons()
    {
        /*foreach (var b in buttons)
        {
            b.Value.on;
        }*/
        buttons["exit"].    onClick.AddListener(delegate { Exit();});
        buttons["solo"].    onClick.AddListener(delegate { MainMenuNav(1); });
        buttons["multi"].   onClick.AddListener(delegate { MainMenuNav(2); });
        buttons["sett"].    onClick.AddListener(delegate { MainMenuNav(3); });

        buttons["soloCont"].    onClick.AddListener(delegate { SoloMenuNav(1); });
        buttons["soloLoad"].    onClick.AddListener(delegate { SoloMenuNav(2); });
        buttons["soloCreate"].  onClick.AddListener(delegate { SoloMenuNav(3); });

        buttons["multiStart"].  onClick.AddListener(delegate { MultiMenuNav(1); });
        buttons["multiJoin"].   onClick.AddListener(delegate { MultiMenuNav(2); });
        buttons["multiLoad"].   onClick.AddListener(delegate { MultiMenuNav(3); });
        buttons["multiCreate"]. onClick.AddListener(delegate { MultiMenuNav(4); });

        buttons["joinMultiJoin"].onClick.AddListener(delegate {MultiMenuNav(5); });
    }
    /// <summary>
    /// Odide z vnorenej navigacie alebo zavire hru
    /// </summary>
    void Exit()
    {
        if (navLayer > 0)
        {
            if (navLayer == 3/* || 2 == navLayer*/)
                FileManager.RegeneradeSettings();
            // Chod o vrstvu vyssie
            navLayer /= 10;
            //pageChange.Play();
            animator.SetInteger("layer", navLayer);
        }
        else
            Application.Quit();
    }
    /// <summary>
    /// Spravuje navigaciu v hlavnom menu
    /// </summary>
    /// <param name="to">VOLBA podla tlacitka</param>
    void MainMenuNav(int to)
    {
        navLayer = to;
        //pageChange.Play();
        animator.SetInteger("layer", to);
    }
    /// <summary>
    /// Spravuje navigaciu v prvom podmenu - v hre pre jedneho hraca
    /// </summary>
    /// <param name="choice">VOLBA podla tlacitka</param>
    void SoloMenuNav(int choice)
    {
        navLayer *= 10;
        navLayer += choice;
        animator.SetInteger("layer", navLayer);

        switch (choice)
        {
            case 1: meneTheme.Stop(); conn.CreateSolo(); break;  // Pokracovat v hre
            case 2: break;  // Nacitat zo subora hru
            case 3: meneTheme.Stop(); conn.CreateSolo(); break;  // Vytvorit novu hru

            default: Debug.LogWarning("Bad input [" + choice + "] on SoloNavigation!"); break;
        }
    }
    /// <summary>
    /// Spravuje navigaciu v druhom pod menu - v hre pre viac hracov <br />
    /// Taktiez overuje vstupy zadane pouzivatelom
    /// </summary>
    /// <param name="choice">VOLBA podla tlacitka</param>
    void MultiMenuNav(int choice)
    {
        bool proceed = false;
        
        switch (choice)
        {
            case 1: if (NameTagCheck()) proceed = true; break;  // Otvori menu pre vytvorenie hry pre viac hracov
            case 2: if (NameTagCheck()) proceed = true; break;  // Otvori menu pre pripojenie sa do hry pre viac hracov

            case 3: proceed = true; break;  // Otvori menu pre loadnutie hry pre viac hracov zo suboru

            case 4: StartConnection(Online); proceed = true; break; // Zapne hru ako Host pre viac hracov

            case 5: ConnectionCheck(); break;  // Pripoji sa do hry pre viac hracov

            default: Debug.LogWarning("Bad input [" + choice + "] on MultiNavigation!"); break;
        }
        if (proceed)
        {            
            navLayer *= 10;
            navLayer += choice;
            animator.SetInteger("layer", navLayer);
            //pageChange.Play();
        }
    }
    /// <summary>
    /// Overi spravnost mena hraca
    /// </summary>
    /// <returns>PRAVDA ak je spravne</returns>
    bool NameTagCheck()
    {
        bool check = false;

        string player = inputFields["playerName"].text.Trim();

        if (player == "")
        {
            textFields["UserNameError"].text = "Type your name";
        }
        else if (player.Length < 2)
        {
            textFields["UserNameError"].text = "Name must be longer";
        }
        else
        {
            textFields["UserNameError"].text = "";
            check = true;
        }

        if (check)
            FileManager.RegeneradeSettings();
        return check;
    }
    /// <summary>
    /// Spusti hru ako hostitel
    /// </summary>
    /// <param name="online">PRAVDA spusti online inak lokane</param>
    /// <param name="load">PRAVDA nacita hru zo suboru</param>
    void StartConnection(bool online, bool load = false)
    {
        meneTheme.Stop();
        FileManager.RegeneradeSettings();
        if (!load)
        {
            conn.StartConnection(online);
        }
        else
        {
            Debug.LogError("NONIMPLEMENTED EXEPTION");
        }
    }
    /// <summary>
    /// Pokusi sa pripojit do hry pre viac hracov
    /// </summary>
    /// <returns>PRAVDA ak je pokus uspasny</returns>
    bool ConnectionCheck()
    {
        meneTheme.Stop();
        FileManager.RegeneradeSettings();
        string ipCode = inputFields["ipCode"].text.Trim();
        bool check = conn.JoinConnection(ipCode, out string e);

        if (!check)
            textFields["IPCodeError"].text = e;

        return check;
    }
}
