using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class menuScript : MonoBehaviour
{
    int navLayer;
    [SerializeField] Animator animator;
    [SerializeField] Toggle onlineToggle;

    [SerializedDictionary("Name", "Button"), SerializeField]
    protected SerializedDictionary<string, Button> buttons = new();
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
    protected SerializedDictionary<string, TMP_InputField> inputFields = new();
     /* OBSAH
        {"playerName",  - },
        {"ipCode",      - }
    */
    
    [SerializedDictionary("Name", "TextField"), SerializeField]
    protected SerializedDictionary<string, TMP_Text> textFields = new();
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
    
    void Start()
    {
        SubscribeToButtons();
        SetTextFields();
        navLayer = 0;
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
        buttons["exit"].onClick.AddListener(Exit);
        buttons["solo"].onClick.AddListener(delegate { MainMenuNav(1); });
        buttons["multi"].onClick.AddListener(delegate { MainMenuNav(2); });
        buttons["sett"].onClick.AddListener(delegate { MainMenuNav(3); });

        buttons["soloCont"].onClick.AddListener(delegate { SoloMenuNav(1); });
        buttons["soloLoad"].onClick.AddListener(delegate { SoloMenuNav(2); });
        buttons["soloCreate"].onClick.AddListener(delegate { SoloMenuNav(3); });

        buttons["multiStart"].onClick.AddListener(delegate { MultiMenuNav(1); });
        buttons["multiJoin"].onClick.AddListener(delegate { MultiMenuNav(2); });
        buttons["multiLoad"].onClick.AddListener(delegate { MultiMenuNav(3); });
        buttons["multiCreate"].onClick.AddListener(delegate { MultiMenuNav(4); });

        buttons["joinMultiJoin"].onClick.AddListener(delegate { MultiMenuNav(5); });
    }
    void Exit()
    {
        if (navLayer > 0)
        {
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
            case 1: break;  // Pokracovat v hre
            case 2: break;  // Nacitat zo subora hru
            case 3: break;  // Vytvorit novu hru

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

            case 4: StartConnection(onlineToggle.isOn); proceed = true; break; // Zapne hru ako Host pre viac hracov

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
            ConnectionManager.instance.StartConnection(online);
        }
        else
        {
            Debug.LogError("NONIMPLEMENTED EXEPTION");
        }
    }
    bool ConnectionCheck()
    {
        string ipCode = inputFields["ipCode"].text.Trim();
        bool check = ConnectionManager.instance.JoinConnection(ipCode, out string e);

        if (!check)
            textFields["IPCodeError"].text = e;

        return check;
    }
}
