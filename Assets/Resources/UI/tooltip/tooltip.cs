using TMPro;
using UnityEngine;
/// <summary>
/// Zobrazuje sa pri mysi 
/// </summary>
public class ToolTip : MonoBehaviour
{
    private static ToolTip toolTip;
    void Awake() => toolTip = this;

    [SerializeField] RectTransform parRect;
    [SerializeField] RectTransform bgRect;
    [SerializeField] TMP_Text tipText;
    [SerializeField] GameManager game;
    const float textPad = 5f;
    /// <summary>
    /// Zobrazi napis 
    /// </summary>
    /// <param name="tip">TEXT tipu</param>
    void ShowToolTip(string tip)
    {
        gameObject.SetActive(true);
        tipText.text = tip;

        Vector2 bgSize = new (tipText.preferredWidth + textPad*2, tipText.preferredHeight + textPad*2);
        bgRect.sizeDelta = bgSize;
    }
    /// <summary>
    /// Skryje objekt
    /// </summary>
    void HideToolTip()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Nasleduje mys
    /// </summary>
    void Update() => transform.position = game.CornerMousePos;

    /// <summary>
    /// Vypise tip na mysi
    /// </summary>
    /// <param name="tip">TEXT tipu</param>
    public static void ShowToolTip_Static(string tip)
    {
        toolTip.ShowToolTip(tip);
    }
    /// <summary>
    /// Skryje tip
    /// </summary>
    public static void HideToolTip_Static()
    {
        toolTip.HideToolTip();
    }


    /// <summary>
    /// Vrati informacie o schopnostiach
    /// </summary>
    /// <param name="skill"></param>
    public static string GetToolTipForSkill(Skill skill)
    {
        return skill.ToString();
    }
}