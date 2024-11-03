using Unity.Netcode;
using UnityEngine;
using System;[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] public class Equipment : Item
{
    public Rezistance rezistance;
    public Slot slot;
    public enum Slot
    {
        Head, Torso, Hands, Legs,
        Body
    }
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref rezistance);
        serializer.SerializeValue(ref slot);
    }
    public override void Use()
    {
        Inventory.instance.Equip(this);
    }
    public virtual void Unequip()
    {
        Inventory.instance.Unequip(this);
    }
}