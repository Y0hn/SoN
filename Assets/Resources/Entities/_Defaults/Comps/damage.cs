using Unity.Netcode;
using System;
[Serializable] public struct Damage : INetworkSerializable, IEquatable<Damage>
{
    public Type type;
    public int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
    public void Add(Damage damage)
    {
        if (type == damage.type)
            amount += damage.amount;
    }
    [Serializable] public enum Type
    {
        // STANDARD
        bludgeoning, slashing, piercing, 
        // ELEMENTAL
        cold, fire, holy, lightning, dark, acid,
        //  OVER TIME
        poison    
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref amount);
    }

    public bool Equals(Damage other)
    {
        return 
        type == other.type && 
        amount == other.amount;
    }
    public override string ToString()
    {
        string s = $"Type: {Enum.GetName(typeof(Type), type)} Amount: ";

        if (amount > 1)
            s += amount.ToString();
        else
            s += amount.ToString() + " %";

        return s;
    }
}
