using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Urcuju nastavenia kvality v hlavnom menu
/// </summary>
public class ResolutionScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Button button;
    private static readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new(3840, 2160),  // 4K UHD
        new(2560, 1440),  // QHD (1440p)
        new(1920, 1080),  // Full HD (1080p)
        new(1600, 900),   // HD+ (900p)
        new(1280, 720)    // HD (720p)
    };
    /// <summary>
    /// Ziskava alebo nastavuje rozlisenie
    /// </summary>
    public int R 
    { 
        get => resolution; 
        set 
        { 
            resolution= value;
            Vector2Int res = resolutions[value];
            Screen.SetResolution(res.x, res.y, Menu.menu.ScreenMode);
            text.text = $"{res.x}x{res.y}";
        } 
    }
    int resolution = 2;
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
        int r = (R+1 < resolutions.Length) ? R+1 : 0;
        R = r;
    }
}
