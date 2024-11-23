using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] 
public class Equipment : Item
{
    public Slot slot;
    public string spriteRef;
    public enum Slot
    {
        Head, Torso, Hands, Legs,
        WeaponL, WeaponR, 
        Body, 
        WeaponBoth, NoPreference
    }
    public override string GetReferency
    {
        get { return FileManager.ITEM_DEFAULT_PATH; }
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
}