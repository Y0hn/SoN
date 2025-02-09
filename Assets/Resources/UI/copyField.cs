using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CopyField : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text text;
    [SerializeField] Button button;

    void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    void OnButtonClick()
    {
        GUIUtility.systemCopyBuffer = text.text; 
        animator.SetTrigger("copy"); 
    }
    public void SetUp(string connection)
    {
        string[] c = connection.Split('-');
        
        if (c[0] == "solo")
            gameObject.SetActive(false);
        else
        {
            label.text = c[0].FirstCharacterToUpper();
            label.text+= $" {(c[1].Contains('.') ? "address" : "code")}";
            label.text+= ": ";
            text.text = c[1];
            gameObject.SetActive(!c[0].Contains("solo"));
        }
    }
}