using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] 
public class Equipment : Item
{
    public Slot slot;
    [SerializeField] protected string sprite;
    /// <summary>
    /// Cesta k texture v hre
    /// </summary>
    public virtual string SpriteRef => sprite;
    public enum Slot
    {
        //Head, Torso, Legs,
        //Body,
        WeaponR, WeaponL, WeaponBoth/*, NoPreference*/
    }
    /// <summary>
    /// Ziska predmet na zaklade referencnej cesty
    /// Tato metoda ma byt "prepisana" (overwrite)
    /// </summary>
    /// <param name="referency">cesta</param>
    /// <returns>NOSITELNY PREDMET</returns>
    public new static Equipment GetItem (string referency)
    {
        return Resources.Load<Equipment>(referency);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string GetReferency => FileManager.ITEM_DEFAULT_PATH;
    /// <summary>
    /// Zisti ci je pouzitelny predmet zbran
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public static bool IsWeapon(Slot slot)  
    { 
        return 
            slot == Slot.WeaponL 
                || slot == 
            Slot.WeaponR;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="iS"></param>
    public override void Use(ItemSlot iS)
    {
        if (iS is EquipmentSlot)
        {
            Inventory.instance.UnEquip((EquipmentSlot)iS);
        }
        else
        {
            iS.Item = null;
            Inventory.instance.Equip(this);
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc/></returns>
    public override bool Equals(Item other)
    {
        bool eq = false;
        if (other is Equipment)
        {
            eq = base.Equals(other);
            var e = (Equipment)other;
            eq = slot == e.slot;
        }
        return eq;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        return 
            base.ToString() + "\n" +
            $"Slot: {slot}\n" + 
            $"Sprite Referency: \"{SpriteRef}\"";
    }
}
/// <summary>
/// Pouziva sa na redukovanie prichadzajuceho poskodenia
/// </summary>
[Serializable] public class Resistance
{
    /// <summary>
    /// Mnozstvo odporu pri utoku
    /// </summary>
    public float amount;
    /// <summary>
    /// Element, proti ktoremu je odolnost urcena
    /// </summary>
    [SerializeField] public Damage.Type defenceType;
    public Resistance (Damage.Type type, float amount)
    {
        defenceType = type;
        this.amount = amount;
    }
}