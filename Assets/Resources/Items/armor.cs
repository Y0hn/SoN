using AYellowpaper.SerializedCollections;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Armor"), Serializable] 
public class Armor : Equipment
{
    [SerializedDictionary("DamageType","Value")]
    public Rezistance rezistance;
    public override string GetReferency
    {
        get { return FileManager.ARMORS_DEFAULT_PATH + "/" + name; }
    }
    public override void Use(ItemSlot iS)
    {
        base.Use(iS);
    }
    public override bool Equals(Item other)
    {
        bool eq = false;
        if (other is Armor)
        {
            eq = base.Equals(other);
            var a = (Armor)other;
            eq &= rezistance.Equals(a.rezistance);
        }
        return eq;
    }
}