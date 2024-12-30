using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor"), Serializable] 
public class Armor : Equipment
{
    public List<Resistance> resists = new();
    public new static Armor GetItem (string referency)
    {
        return Resources.Load<Armor>(referency);
    }
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
        return resists.FindAll(r => r.defenceType == type);
    }
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
}