using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Drzi predmet v inventary
/// </summary>
public class ItemSlot : MonoBehaviour
{
    public bool empty = true;
    [SerializeField] protected Image icon;
    [SerializeField] protected Button button;
    [SerializeField] protected Button remove;
    [SerializeField] protected Image background;
    [SerializeField] protected Color defaultColor;
    private Item item;
    /// <summary>
    /// Drzany predmet
    /// </summary>
    /// <value></value>
    public Item Item
    {
        get { return item; }
        set 
        {
            item = value; 
            empty = Item == null;
            ItemUpdate();
        }
    }
    /// <summary>
    /// Zmena drzaneho predmetu
    /// </summary>
    protected virtual void ItemUpdate()
    {
        if (empty)
        {
            button.onClick.RemoveAllListeners();

            if (remove != null)
            {
                remove.onClick.RemoveAllListeners();
                remove.gameObject.SetActive(false);
            }

            background.color = defaultColor;
            icon.sprite = null;
        }
        else
        {
            icon.sprite = Resources.Load<Sprite>(Item.iconRef);
            button.onClick.AddListener( delegate { Item.Use(this); });
            string s =Item.GetReferency; 

            if (remove != null)
            {
                remove.onClick.AddListener( delegate { GameManager.instance.inventory.Remove(s); });
                remove.gameObject.SetActive(true);
            }
            
            background.color = Item.rarity;
            icon.color = Item.color;
        }

        button.interactable = !empty;
        icon.enabled = !empty;
    }
}
