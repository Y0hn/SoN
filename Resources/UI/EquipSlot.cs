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
        if (!empty)
        {
            button.onClick.RemoveListener(Item.Use);
            button.onClick.AddListener(((Equipment)Item).Unequip);
        }
        placeHolder.gameObject.SetActive(!empty);
        button.interactable = !empty;
        icon.enabled = !empty;
    }
}
