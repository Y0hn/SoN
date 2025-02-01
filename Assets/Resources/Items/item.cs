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
    [SerializeField] protected string path = "";
    /// <summary>
    /// Ziska predmet na zaklade referencnej cesty
    /// Tato metoda ma byt "prepisana" (overwrite/new)
    /// </summary>
    /// <param name="referency">cesta</param>
    /// <returns>PREDMET</returns>
    public static Item GetItem (string referency)
    {
        return Resources.Load<Item>(referency);
    }
    /// <summary>
    /// Ziskava cestu k aktualnemu predmetu
    /// </summary>
    public virtual string GetReferency 
    { 
        get 
        { 
            if (path != "")
                return path;
            else
                return FileManager.ITEM_DEFAULT_PATH + "/" + name; 
        } 
    }
    /// <summary>
    /// Bude obsahovat to co sa stane po/pri pouziti predmetu 
    /// </summary>
    public virtual void Use()
    {
        // tu bude pouzitie predmetu
    }
    /// <summary>
    /// pouzitie predmetu ako nositelneho predmetu
    /// </summary>
    /// <param name="iS"></param>
    public virtual void Use(ItemSlot iS)
    {
        Debug.LogWarning("This is NOT SUPOSSED TO HAPPEN !!");
    }
    /// <summary>
    /// Sluzi na porovnavanie predmetov
    /// Dolezite pri tvorbe listov
    /// </summary>
    /// <param name="other">Druhy predmet</param>
    /// <returns>PRAVDA ak su rovnake</returns>
    public virtual bool Equals(Item other)
    {
        return 
        name == other.name && 
        rarity == other.rarity &&
        iconRef == other.iconRef && 
        description == other.description;
    }
    /// <summary>
    /// Sluzi na rychle ziskanie vypisu vlastnosti predmetu
    /// </summary>
    /// <returns>vypis PARAMETROV</returns>
    public override string ToString()
    {
        return 
            $"Item [{title}]\n" +
            $"desc: {description}\n" +
            $"IconColor: {color}\n" +
            $"Rarity color: {rarity}\n" +
            $"Icon Referency: \"{iconRef}\"";
    }
}