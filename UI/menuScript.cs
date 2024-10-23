using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class menuScript : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializedDictionary("Name", "Button"), SerializeField]
    protected SerializedDictionary<string, Button> buttons = new();
    /*
        {"exit", null},
        {"solo", null},
        {"mult", null},
        {"sett", null},

        {"soloCont", null},
        {"soloLoad", null},
        {"soloCrea", null},

        {"multJoin", null},
        {"multLoad", null},
        {"multCrea", null},

        {"multJoinEnter", null}
    */
    [SerializedDictionary("Name", "InputField"), SerializeField]
    protected SerializedDictionary<string, TMP_InputField> inputFields = new();
     /*
        {"playerName", null},
        {"joinDestin", null},
    */
    [SerializedDictionary("Name", "InputField"), SerializeField]
    protected SerializedDictionary<string, TMP_InputField> textFields = new();
    /*
        TO DOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
    */
    void Start()
    {
        
    }
}
