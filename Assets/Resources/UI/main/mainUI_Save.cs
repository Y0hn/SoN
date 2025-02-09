using UnityEngine;
using TMPro;
/// <summary>
/// <inheritdoc/>
/// pre nacitanie z vyberu ulozeni svetov
/// </summary>
public class MainUISave : MainUIButton 
{
    [SerializeField] TMP_Text saveName;
    [SerializeField] TMP_Text saveDate;

    public void SetUp(ref World world)
    {
        name = world.worldName;
        saveName.text = world.worldName;
        saveDate.text = $"[{world.writeDate.Split(' ')[0]}]";
        AddListener(delegate { _=Menu.menu.PressLoad(name); });
    }
}