using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class menuScript : MonoBehaviour
{
    int navLayer;
    [SerializeField] Animator animator;

    [SerializedDictionary("Name", "Button"), SerializeField]
    protected SerializedDictionary<string, Button> buttons = new();
    /* OBSAH
        {"exit",        #EDITOR_SETTED_VALUE },
        {"solo",        #EDITOR_SETTED_VALUE },
        {"multi",        #EDITOR_SETTED_VALUE },
        {"sett",        #EDITOR_SETTED_VALUE },

        {"soloCont",    #EDITOR_SETTED_VALUE },
        {"soloLoad",    #EDITOR_SETTED_VALUE },
        {"soloCrea",    #EDITOR_SETTED_VALUE },

        {"multiJoin",    #EDITOR_SETTED_VALUE },
        {"multiLoad",    #EDITOR_SETTED_VALUE },
        {"multiCrea",    #EDITOR_SETTED_VALUE },

        {"joinMultiJoin",  #EDITOR_SETTED_VALUE }
    */
    [SerializedDictionary("Name", "InputField"), SerializeField]
    protected SerializedDictionary<string, TMP_InputField> inputFields = new();
     /* OBSAH
        {"playerName",  #EDITOR_SETTED_VALUE },
        {"joinDestin",  #EDITOR_SETTED_VALUE },
    */
    [SerializedDictionary("Name", "TextField"), SerializeField]
    protected SerializedDictionary<string, TMP_Text> textFields = new();
    /* OBSAH
        {"mSolo",               #EDITOR_SETTED_VALUE },
        {"sTitle",              #EDITOR_SETTED_VALUE },
        {"sContinue",           #EDITOR_SETTED_VALUE },
        {"sLoad",               #EDITOR_SETTED_VALUE }, 
        {"sCreate",             #EDITOR_SETTED_VALUE }, 

        {"mMulti",              #EDITOR_SETTED_VALUE }, 
        {"muTitle",             #EDITOR_SETTED_VALUE }, 
        {"muPlayerNameTitle",   #EDITOR_SETTED_VALUE },
        {"muPlayerNamePlace",   #EDITOR_SETTED_VALUE },
        {"UserNameError",       #EDITOR_SETTED_VALUE },
        {"muCreate",            #EDITOR_SETTED_VALUE }, 
        {"muJoin",              #EDITOR_SETTED_VALUE },

        {"joTitle",             #EDITOR_SETTED_VALUE }, 
        {"joServerTitle",       #EDITOR_SETTED_VALUE }
        {"joServerPlace",       #EDITOR_SETTED_VALUE },
        {"IPCodeError",         #EDITOR_SETTED_VALUE }, 
        {"joJoin",              #EDITOR_SETTED_VALUE }, 

        {"mSett",               #EDITOR_SETTED_VALUE },
        {"setTitle",            #EDITOR_SETTED_VALUE }, 

        {"Exit",                #EDITOR_SETTED_VALUE },

        {"VERSION",             #EDITOR_SETTED_VALUE },
        {"COMPANY",             #EDITOR_SETTED_VALUE }
    */
    void Start()
    {
        SubscribeToButtons();
        MenuLanguageSet();
        navLayer = 0;
    }
    void MenuLanguageSet()
    {
        // TO DOO WITH TEXT_FIELDS
    }
    void SubscribeToButtons()
    {
        buttons["exit"].onClick.AddListener(Exit);
        buttons["solo"].onClick.AddListener(delegate { MainMenuNav(1); });
        buttons["multi"].onClick.AddListener(delegate { MainMenuNav(2); });
        buttons["sett"].onClick.AddListener(delegate { MainMenuNav(3); });
        buttons["soloCont"].onClick.AddListener(delegate { SoloMenuNav(1); });
        buttons["soloLoad"].onClick.AddListener(delegate { SoloMenuNav(2); });
        buttons["soloCrea"].onClick.AddListener(delegate { SoloMenuNav(3); });
        //buttons["multiLoad"].onClick.AddListener(delegate { MultiMenuNav(2); });
        buttons["multiJoin"].onClick.AddListener(delegate { MultiMenuNav(2); });
        buttons["multiCrea"].onClick.AddListener(delegate { MultiMenuNav(3); });
        buttons["joinMultiJoin"].onClick.AddListener(delegate { MultiMenuNav(4); });
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
        {
            Application.Quit();
        }
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
        navLayer *= 10;
        navLayer += choice;
        animator.SetInteger("layer", navLayer);
        
        switch (choice)
        {
            case 1: break;  // Vytvorit multiplayer hru
            case 2: break;  // Pripojit sa do cudzej hry
            case 3: break;  // Zahajit hru
            case 4: break;  // Vstupit do hry

            default: Debug.LogWarning("Bad input [" + choice + "] on MultiNavigation!"); break;
        }
    }
}
