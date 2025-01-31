using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Drzi nositelny predmet v inventari
/// </summary>
public class EquipmentSlot : ItemSlot
{
    /* ZDEDENE ATRIBUTY
     * public bool empty = true;
     * [SF] Image icon;
     * [SF] Button button;
     * [SF] Image background;
     * [SF] Color defaultColor;
     * public Item Item {}
     *  *  *  *  *  *  *  *  *  */
    [SerializeField] Image placeHolder;
    [SerializeField] public Equipment.Slot slot;
    static Color ghostC = new Color(1, 1, 1, 0.5f);
    bool isGhost;

    /// <summary>
    /// Zmena predmentu
    /// </summary>
    protected override void ItemUpdate()
    {
        base.ItemUpdate();
        if (!empty)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener( delegate { Item.Use(this);});
        }
        placeHolder.gameObject.SetActive(empty);
        button.interactable = !empty;
        icon.enabled = !empty;
        isGhost = false;
    }
    /// <summary>
    /// Nastavenie priesvitnosti ikony
    /// </summary>
    /// <param name="isTransparent"></param>
    public void SetTransparent(bool isTransparent)
    {
        if      (!isGhost && isTransparent)
        {
            icon.color *= ghostC;
            isGhost = true;
        }
        else if (isGhost && !isTransparent)
        {
            icon.color = Item.color;
            isGhost = false;
        }
        // inak sa nemusi menit
    }
}
