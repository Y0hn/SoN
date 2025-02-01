using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Urcuju nastavenia kvality v hlavnom menu
/// </summary>
public class QualityScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Button button;
    /// <summary>
    /// Ziskava alebo nastavuje kvalitu
    /// </summary>
    public int Q 
    { 
        get => quality; 
        set { quality=value; SetQuality(quality); } 
    }
    int quality = 0;
    /// <summary>
    /// Spusit sa na zaciatku
    /// </summary>
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    /// <summary>
    /// Prepne do dalsej moznosti
    /// </summary>
    void OnButtonClick()
    {
        int q = (Q+1 < QualitySettings.count) ? Q+1 : 0;
        Q = q;
    }
    /// <summary>
    /// Nastavuje kvalitu
    /// </summary>
    /// <param name="q"></param>
    void SetQuality(int q)
    {
        //Debug.Log($"Quality set to [{q} / {quals.Length}]");
        text.text = QualitySettings.names[q];
        QualitySettings.SetQualityLevel(q);
    }
}
