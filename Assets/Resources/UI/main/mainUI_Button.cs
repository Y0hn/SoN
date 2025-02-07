using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
//using TMPro;
/// <summary>
/// Sluzi pre rychlu odozvu k stlaceniu UI tlacida
/// </summary>
public class MainUIButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] AudioClip clip;
    //[SerializeField] TMP_Text text;
    AudioSource source;

    /*public string Text 
    {
        get => text.text;
        set => text.text = value;
    }*/

    void Awake()
    {
        AddListener(OnClick);
        FileManager.Log($"{name} awoken");
    }
    void Start()
    {
        
    }
    void OnClick()
    {
        FileManager.Log($"{name} clicked");
        source.PlayOneShot(clip);
    }
    public void AddListener(UnityAction call)
    {
        button.onClick.AddListener(call);
    }
    public void SetAudioSource(AudioSource s)
    {
        source = s;
    }
}
