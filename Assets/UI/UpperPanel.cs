using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Spravuje horny panel hracskeho UI
/// </summary>
public class UpperPanel : MonoBehaviour
{
    [SerializedDictionary("Switches to", "Button"), SerializeField]    
    SerializedDictionary<SwitchTo, Button> buttons = new();
    private SwitchTo lastSwitch = SwitchTo.None;
    [SerializeField] GameObject skillTree;
    /// <summary>
    /// Nastavi prepinacie stlacidla a vynuluje stav
    /// </summary>
    void Start()
    {
        buttons[SwitchTo.Inventory].onClick.AddListener(delegate { SwitchBetween(SwitchTo.Inventory); });
        buttons[SwitchTo.SkillTree].onClick.AddListener(delegate { SwitchBetween(SwitchTo.SkillTree); });
        Reset();
    }
    /// <summary>
    /// Prepina medzi Inventarom a Stromom schopnosti 
    /// </summary>
    /// <param name="switchTo">prepnut DO</param>
    void SwitchBetween(SwitchTo switchTo)
    {
        if (lastSwitch != SwitchTo.None)
            buttons[lastSwitch].interactable = true;

        buttons[switchTo].interactable = false;
        skillTree.SetActive(switchTo == SwitchTo.SkillTree);
        lastSwitch = switchTo;
    }
    /// <summary>
    /// Vynuluje graficky stav 
    /// </summary>
    public void Reset()
    {
        SwitchBetween(SwitchTo.Inventory);
        buttons[SwitchTo.SkillTree].interactable = true;
    }
    /// <summary>
    /// Sluzi na urcenie aktuanej a pozadovanej obrazovky
    /// </summary>
    enum SwitchTo
    {
        None, Inventory, SkillTree
    }
}
