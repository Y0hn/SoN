using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class MenuScript : MonoBehaviour
{
    int navLayer;
    [SerializeField] Connector conn;
    [SerializeField] Animator animator;
    [SerializeField] Toggle onlineToggle;
    [SerializeField] AudioMixer[] audioMixers;
    public bool Online { get => onlineToggle.isOn; set => onlineToggle.isOn = value; }
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
    void Start()
    {
        conn = Connector.instance;
        SubscribeToButtons();
        SetTextFields();
        SetUpUI();
    }
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
    }
    void SetTextFields()
    {
        textFields["TITLE"].text = Application.productName;
        textFields["VERSION"].text = "Version: " + Application.version;
        textFields["COMPANY"].text = Application.companyName;

        textFields["IPCodeError"].text = "";
        textFields["UserNameError"].text = "";
    }
    void SubscribeToButtons()
    {
        buttons["exit"].    onClick.AddListener(Exit);
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

        buttons["joinMultiJoin"].onClick.AddListener(delegate { MultiMenuNav(5); });
    }
    void Exit()
    {
        if (navLayer > 0)
        {
            if (navLayer == 3)
                FileManager.RegeneradeSettings();
            // Chod o layer vyssie
            navLayer /= 10;
            animator.SetInteger("layer", navLayer);
        }
        else
            Application.Quit();
    }
    void MainMenuNav(int to)
    {
        navLayer = to;
        animator.SetInteger("layer", to);
    }
    void SoloMenuNav(int choice)
    {
        navLayer *= 10;
        navLayer += choice;
        animator.SetInteger("layer", navLayer);

        switch (choice)
        {
            case 1: conn.CreateSolo(); break;  // Pokracovat v hre
            case 2: break;  // Nacitat zo subora hru
            case 3: conn.CreateSolo(); break;  // Vytvorit novu hru

            default: Debug.LogWarning("Bad input [" + choice + "] on SoloNavigation!"); break;
        }
    }
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
        }
    }
    bool NameTagCheck()
    {
        bool check = false;

        string player = inputFields["playerName"].text.Trim();

        if (player == "")
            textFields["UserNameError"].text = "Type your name";
        else if (player.Length < 2)
            textFields["UserNameError"].text = "Name must be longer";
        else
        {
            textFields["UserNameError"].text = "";
            check = true;
        }          

        return check;
    }
    void StartConnection(bool online, bool load = false)
    {
        if (!load)
        {
            conn.StartConnection(online);
        }
        else
        {
            Debug.LogError("NONIMPLEMENTED EXEPTION");
        }
    }
    bool ConnectionCheck()
    {
        string ipCode = inputFields["ipCode"].text.Trim();
        bool check = conn.JoinConnection(ipCode, out string e);

        if (!check)
            textFields["IPCodeError"].text = e;

        return check;
    }
}
