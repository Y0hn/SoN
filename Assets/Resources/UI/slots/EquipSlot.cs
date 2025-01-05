using UnityEngine.UI;
using UnityEngine;

public class EquipmentSlot : ItemSlot
{
    /* Inhereted
    public bool empty = true;
    [SerializeField] Image icon;
    [SerializeField] Button button;
    [SerializeField] Image background;
    [SerializeField] Color defaultColor;
    public Item Item {}
    */
    [SerializeField] Image placeHolder;
    [SerializeField] public Equipment.Slot slot;
    static Color ghostC = new Color(1, 1, 1, 0.5f);
    bool isGhost;
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
