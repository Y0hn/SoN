using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor"), Serializable] 
public class Armor : Equipment
{
    public List<Resistance> rezists = new();
    
    public override string GetReferency
    {
        get { return FileManager.ARMORS_DEFAULT_PATH + "/" + name; }
    }
    public override void Use(ItemSlot iS)
    {
        base.Use(iS);
    }
    public List<Resistance> GetElemental(Damage.Type type)
    {
        return rezists.FindAll(r => r.defenceType == type);
    }
    [Serializable] public class Resistance
    {
        public float amount;
        [SerializeField] public Damage.Type defenceType;
    }
}