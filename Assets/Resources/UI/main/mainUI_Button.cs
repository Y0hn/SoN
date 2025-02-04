using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainUIButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text text;
    [SerializeField] AudioSource source;

    public string Text 
    {
        get => text.text;
        set => text.text = value;
    }

    void Awake()
    {
        //Text = name;
        button.onClick.AddListener(OnClick);
        FileManager.Log($"{name} awoken");
    }
    void OnClick()
    {
        FileManager.Log($"{name} clicked");
        source.Play();
    }
}
