using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item"), Serializable] 
public abstract class Item : ScriptableObject, IEquatable<Item>
{
    public string title = "null";
    public string description;
    public string iconRef = "Items/textures";
    public Color color = Color.white;
    public Color rarity = Color.white;
    public static Item GetItem (string referency)
    {
        return Resources.Load<Item>(referency);
    }
    public virtual string GetReferency { get { return FileManager.ITEM_DEFAULT_PATH + "/" + name; } }
    public virtual void Use()
    {
        // usage of Item here
    }
    public virtual void Use(ItemSlot iS)
    {
        // usage of Item here
        Debug.LogWarning("This is NOT SUPOSSED TO HAPPEN !!");
    }
    public virtual bool Equals(Item other)
    {
        return 
        name == other.name && 
        rarity == other.rarity &&
        iconRef == other.iconRef && 
        description == other.description;
    }
}