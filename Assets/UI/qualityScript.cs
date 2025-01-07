using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QualityScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Button button;
    public int Q 
    { 
        get => quality; 
        set { quality=value; SetQuality(quality); } 
    }
    int quality = 0;
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    void OnButtonClick()
    {
        int q = (Q+1 < QualitySettings.count) ? Q+1 : 0;
        Q = q;
    }
    void SetQuality(int q)
    {
        //Debug.Log($"Quality set to [{q} / {quals.Length}]");
        text.text = QualitySettings.names[q];
        QualitySettings.SetQualityLevel(q);
    }
}
