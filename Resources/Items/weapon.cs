using Unity.Netcode;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] public class Weapon : Item
{
    public Attack attack;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref attack);
    }
}