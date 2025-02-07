using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Spravuje hlavne menu
/// </summary>
public class MenuScript : MonoBehaviour
{
    [SerializeField] Connector conn;
    [SerializeField] new Camera camera;
    [SerializeField] Animator animator;
    [SerializeField] Toggle onlineToggle;
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
    
    const float ANIMATION_DURATION = 15f/60f + 25f/60f;
    Stack<string> currentLayer;  
    float timer;  

    public string PlayerName { get => inputFields["playerName"].text; set => inputFields["playerName"].text = value; }
    public bool Online { get => onlineToggle.isOn;  set => onlineToggle.isOn = value;   }
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
        ResetUI();
        
        FileManager.LoadSettings();        
    }
    void Start() 
    {
        conn ??= Connector.instance;
        meneTheme.Play();        
    }
    /// <summary>
    /// Sluzi pre animovanie "farebneho" prechodu
    /// </summary>
    void FixedUpdate()
    {
        if (0 < timer && timer < Time.time)
        {
            uis[currentLayer.Peek()].SetActive(true);
            timer = -1;
        }
    }
    /// <summary>
    /// Resetuje nastavenie UI elementov, cize zapne vsetky zakladne a vypne vsetky podradene ponuky
    /// </summary>
    public void ResetUI()
    {
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
        timer = -1;
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

        textFields["IPCodeError"].text = "";
        textFields["UserNameError"].text = "";
    }
    /// <summary>
    /// Prida posluchcov na stlacenie tlacidiel podla slovnika "buttons"
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
            // Chod o vrstvu vyssie
            uis[currentLayer.Pop()].SetActive(false);
            animator.SetTrigger("change");
            timer = Time.time + ANIMATION_DURATION;
        }
        else
            // Zavri hru
            Application.Quit();
    }
    /// <summary>
    /// Spravuje navigaciu v hlavnom menu
    /// </summary>
    /// <param name="to">VOLBA podla tlacitka</param>
    void MenuNavigation(sbyte to, sbyte from = 0)
    {
        int layer = 10*from + to;
        uis[currentLayer.Peek()].SetActive(false);
        switch (layer)
        {
            // HLAVNE menu
            case 1: currentLayer.Push("SubSolo"); break;
            case 2: currentLayer.Push("SubMulti"); break;
            case 3: currentLayer.Push("SubSett"); break;

            // PODPONUKA pre JEDNEHO hraca                          (localhost)
            case 11: /* Zapne hru na localhoste */ conn.CreateSolo(); break;
            case 12: /* Nacita zo subora hru    */  break; 
            case 13: /* Vytvorit novu hru       */ conn.CreateSolo(); break;

            // PODPONUKA pre VIAC hracov
            case 21: 
                if(NameTagCheck()) 
                    currentLayer.Push("SubMultiStart");
                break;  // vnori sa do ponuky pre server
            case 22: 
                if(NameTagCheck()) 
                    currentLayer.Push("SubMultiJoin"); 
                break;  // vnori sa do ponuky pre klienta

            // PODPONUKA pre ZALOZENIE hry pre VIAC hracov          (pre server)
            case 211: /* Nacita zo subora hru   */  break;
            case 212: /* Vytvorit novu hru      */  StartConnection(Online); break;

            // PODPONUKA pre PRIPOJENIE sa ho hry pre VIAC hracov   (pre klienta)
            case 221: 
                /* Pripoji sa do uz existujucej hry */
                ConnectionCheck();
                break;

            default: 
                FileManager.Log("Bad layer [" + layer + "] on MenuNavigation!", FileManager.MessageType.WARNING); 
                break;
        }

        if (layer < 0)
        {
            meneTheme.Stop();

            gameObject.SetActive(false);
            ResetUI();
            FileManager.Log($"Game Started => MenuReset => MenuTheme Stoped", FileManager.MessageType.RECORD);
        }

        FileManager.Log($"Navigation set to {currentLayer}");
        animator.SetTrigger("change");
        timer = Time.time + ANIMATION_DURATION;
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
