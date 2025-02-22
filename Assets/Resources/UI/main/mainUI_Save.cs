using UnityEngine;
using TMPro;
using UnityEngine.UI;
/// <summary>
/// <inheritdoc/>
/// pre nacitanie z vyberu ulozeni svetov
/// </summary>
public class MainUISave : MainUIButton 
{
    [SerializeField] TMP_Text saveName;
    [SerializeField] TMP_Text saveDate;
    [SerializeField] Button deleteButton;
    public void SetUp(ref World world)
    {
        name = world.worldName;
        saveName.text = world.worldName;
        saveDate.text = $"[{world.writeDate.Split(' ')[0]}]";
        button.interactable = !world.ended;
        AddListener(delegate { Menu.menu.PressLoad(name); });
        deleteButton.onClick.AddListener(DeleteSave);
    }
    void DeleteSave()
    {
        FileManager.DeleteWorld(name);
        Menu.menu.ReloadContinue();
        Destroy(gameObject);
    }
}