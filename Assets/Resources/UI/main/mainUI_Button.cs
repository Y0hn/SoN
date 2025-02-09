using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
//using TMPro;
/// <summary>
/// Sluzi pre rychlu odozvu k stlaceniu UI tlacida
/// </summary>
public class MainUIButton : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected AudioClip clip;
    //[SerializeField] TMP_Text text;
    AudioSource source;

    /*public string Text 
    {
        get => text.text;
        set => text.text = value;
    }*/
    public bool Interactable 
    {
        get => button.interactable;
        set => button.interactable = value;
    }

    protected virtual void Awake()
    {
        AddListener(OnClick);
        FileManager.Log($"{name} awoken");
    }
    protected virtual void Start()
    {
        
    }
    protected virtual void OnClick()
    {
        source.PlayOneShot(clip);
        FileManager.Log($"{name} clicked");
    }
    public virtual void AddListener(UnityAction call)
    {
        button.onClick.AddListener(call);
    }
    public virtual void SetAudioSource(AudioSource s)
    {
        source = s;
    }
}
