using Unity.Netcode;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] public class Weapon : Item, IEquatable<Weapon>
{
    public Attack attack;
    public bool twoHanded = false;
    public string spriteRef;

    public bool Equals(Weapon other)
    {
        throw new NotImplementedException();
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref attack);
        serializer.SerializeValue(ref twoHanded);
    }
}