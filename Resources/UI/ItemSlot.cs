using UnityEngine.UI;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public bool empty = true;
    [SerializeField] Image icon;
    [SerializeField] Image background;
    public Item Item
    {
        get { return Item; }
        private set 
        { 
            Item = value; 
            empty = Item == null;
            if (!empty)
                ItemUpdate(); 
        }
    }
    void ItemUpdate()
    {
        icon.sprite = Resources.Load<Sprite>(Item.iconRef);
        icon.color = Item.color;
        background.color = Item.rarity;
    }
    public void SetItem(Item item)
    {
        Item = item;
    }
}
