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
    }
}
