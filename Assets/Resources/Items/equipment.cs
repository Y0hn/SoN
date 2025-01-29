using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] 
public class Equipment : Item
{
    public Slot slot;
    [SerializeField] protected string sprite;
    public virtual string SpriteRef { get; }
    public enum Slot
    {
        //Head, Torso, Legs,
        //Body,
        WeaponR, WeaponL, WeaponBoth/*, NoPreference*/
    }
    /// <summary>
    /// Meni navratovu hodnotu povodnej metody
    /// </summary>
    /// <param name="referency"></param>
    /// <returns></returns>
    public new static Equipment GetItem (string referency)
    {
        return Resources.Load<Equipment>(referency);
    }
    public override string GetReferency
    {
        get { return FileManager.ITEM_DEFAULT_PATH; }
    }
    //public static bool IsArmor(Slot slot)   { return slot == Slot.Head || slot == Slot.Torso || slot == Slot.Legs || slot == Slot.Body; }
    public static bool IsWeapon(Slot slot)  
    { 
        return 
            slot == Slot.WeaponL 
                || slot == 
            Slot.WeaponR;
    }
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
    public float amount;
    [SerializeField] public Damage.Type defenceType;
    public Resistance (Damage.Type type, float amount)
    {
        defenceType = type;
        this.amount = amount;
    }
}