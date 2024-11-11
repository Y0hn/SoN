using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] 
public class Weapon : Equipment
{
    public Attack attack;
    public override string GetReferency
    {
        get { return FileManager.WEAPONS_DEFAULT_PATH + "/" + name; }
    }
    public override void Use(ItemSlot iS)
    {
        base.Use(iS);
    }
    public override bool Equals(Item other)
    {
        bool eq = false;
        if (other is Weapon)
        {
            eq = base.Equals(other);
            var w = (Weapon)other;
            eq &= attack.Equals(w.attack);
        }
        return eq;
    }
}