using UnityEngine.UI;
using UnityEngine;
using TMPro;

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
        label.text = c[0];
        text.text = c[1];
        gameObject.SetActive(!c[0].Contains("solo"));
    }
}