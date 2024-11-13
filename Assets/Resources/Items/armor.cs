using AYellowpaper.SerializedCollections;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor"), Serializable] 
public class Armor : Equipment
{

    [SerializedDictionary("Name", "GameObject")]
    public SerializedDictionary<Damage.Type, Rezistance> rezists = new();
    
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
        bool eq = base.Equals(other);
        try
        {
            Armor a = (Armor)other;
            eq &= other is Armor;
            eq &= rezists.Count == a.rezists.Count;
            foreach (Damage.Type key in rezists.Keys)
                eq &= rezists[key].Equals(a.rezists[key]);
        } catch (Exception e) {
            Debug.Log("Equals error " + e.Message);
            eq = false;
        }
        return eq;
    }
}