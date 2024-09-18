using Unity.Netcode;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Money"), Serializable] public class Coin : Item
{
    public int amount;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref amount);
    }
}