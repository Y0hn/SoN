using UnityEngine.UI;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public bool empty = true;
    [SerializeField] Image icon;
    [SerializeField] Image background;
    [SerializeField] Color defaultColor;
    private Item item;
    public Item Item
    {
        get { return item; }
        private set 
        {
            item = value; 
            empty = Item == null;
            if (!empty)
                ItemUpdate(); 
        }
    }
    void ItemUpdate()
    {
        if (empty)
        {
            background.color = defaultColor;
            icon.sprite = null;
        }
        else
        {
            icon.sprite = Resources.Load<Sprite>(Item.iconRef);
            icon.color = Item.color;
            background.color = Item.rarity;
        }
        icon.enabled = !empty;
    }
    public void SetItem(Item item = null)
    {
        Item = item;
    }
}
