using Unity.Netcode;
using System;

/// <summary>
/// Drzi hodnoty poskodenia utoku
/// </summary>
[Serializable] public struct Damage : INetworkSerializable, IEquatable<Damage>
{
    public Type type;
    public int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
    /// <summary>
    /// Prida velkost poskodenia
    /// </summary>
    /// <param name="damage"></param>
    public void Add(Damage damage)
    {
        if (type == damage.type)
            amount += damage.amount;
    }
    [Serializable] public enum Type
    {
        None,
        FIST, BROOM,
        SWORD_SLASH, SWORD_TRUST, 
        BOW_SINLE, BOW_MULTI
    }
    /// <summary>
    /// Podla typu poskodenia vrati ci je na blizku
    /// </summary>
    public readonly bool Melee => type switch
    {
        Type.FIST or Type.BROOM or Type.SWORD_SLASH or Type.SWORD_TRUST => true,
        _ => false
    };
    /// <summary>
    /// Podla typu poskodenia vrati ci je na dialku
    /// </summary>
    public readonly bool Ranged => type switch
    {
        Type.BOW_SINLE or Type.BOW_MULTI => true,
        _ => false
    };

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
