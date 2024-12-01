using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] 
public class Weapon : Equipment
{
    public List<Attack> attack;
    public override string GetReferency
    {
        get { return FileManager.WEAPONS_DEFAULT_PATH + "/" + name; }
    }
    public override string SpriteRef => FileManager.WEAPONS_REF_DEFAULT_PATH + "/" + sprite;
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
    public Class CallculateWC()
    {
        List<float> vals = new();

        for (int i = 0; i < vals.Count; i++)
            vals.Add(attack[i].rate * attack[i].damage.amount);
        
        int index = vals.IndexOf(vals.Max());   // zsika najefektiktivnejsi utok zbrane
        vals.Clear();
        float r = attack[index].rate;

        switch (attack[index].type)
        {
            case Attack.Type.BowSingle:
            case Attack.Type.BowMulti:
                return Class.Ranged;
            default:
                if      (r > 2f)
                    return Class.Light;
                else if (r > 1f)
                    return Class.Medium;
                else
                    return Class.Heavy;
        }
    }
    public enum Class { Light, Medium, Heavy, Ranged, AoE }
}