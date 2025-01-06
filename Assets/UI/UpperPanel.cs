using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class UpperPanel : MonoBehaviour
{
    [SerializedDictionary("Switches to", "Button"), SerializeField]    
    SerializedDictionary<SwitchTo, Button> buttons = new();
    private SwitchTo lastSwitch = SwitchTo.None;
    [SerializeField] GameObject skillTree;
    void Start()
    {
        buttons[SwitchTo.Inventory].onClick.AddListener(delegate { SwitchBetween(SwitchTo.Inventory); });
        buttons[SwitchTo.SkillTree].onClick.AddListener(delegate { SwitchBetween(SwitchTo.SkillTree); });
        Reset();
    }
    void SwitchBetween(SwitchTo switchTo)
    {
        if (lastSwitch != SwitchTo.None)
            buttons[lastSwitch].interactable = true;

        buttons[switchTo].interactable = false;
        skillTree.SetActive(switchTo == SwitchTo.SkillTree);
        lastSwitch = switchTo;
    }
    public void Reset()
    {
        SwitchBetween(SwitchTo.Inventory);
        buttons[SwitchTo.SkillTree].interactable = true;
    }
    enum SwitchTo
    {
        None, Inventory, SkillTree
    }
}
