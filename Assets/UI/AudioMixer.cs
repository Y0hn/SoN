using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioMixer : MonoBehaviour
{
    [SerializeField] AudioMixerGroup mixerG;    // -80 <=> 20 dB
    [SerializeField] TMP_Text title;
    [SerializeField] Slider slider;
    /// <summary>
    /// Ziskanie a nastavenie hodnoty posuvaca
    /// </summary>
    /// <value></value>
    public float SliderValue 
    { 
        get => slider.value; 
        set 
        {
            slider.value = value;
            ChangeMixer(slider.value);
        }
    }
    
    void Start()
    {
        slider.onValueChanged.AddListener(ChangeMixer);
        title.text = mixerG.name;
        ChangeMixer(SliderValue);
    }
    /// <summary>
    /// Nastavenie zmenenej hodnoty posovaca do audio mixeru 
    /// </summary>
    /// <param name="value"></param>
    void ChangeMixer(float value)
    {
        value = Mathf.Log10(value)*20;
        mixerG.audioMixer.SetFloat(mixerG.name, value);
        FileManager.Log($"Audio volume of {mixerG.name} setted to {value}");
    }
}
