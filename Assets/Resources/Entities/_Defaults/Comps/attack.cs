using Unity.Netcode;
using UnityEngine;
using System;
[Serializable] public struct Attack : INetworkSerializable, IEquatable<Attack>
{
    public Damage damage;
    public float range;
    public float rate;
    public Type type;
    public bool bothHanded;

    public readonly float AttackTime { get => 1/rate; }
    public readonly bool IsMelee    { get => MeleeAttack(type); }
    public readonly bool IsRanged   { get => RangedAttack(type); }
    public readonly bool IsSet      { get => range != 0 && rate != 0; }

    public Attack (Damage damage, float range, float rate, Type type, bool both = false)
    {
        this.bothHanded = both;
        this.damage = damage;
        this.range = range;
        this.rate = rate;
        this.type = type;
    }
    public Attack (Attack attack)
    {
        this.bothHanded =  attack.bothHanded;
        this.damage =  attack.damage;
        this.range = attack.range;
        this.rate =  attack.rate;
        this.type =  attack.type;

    }
    public enum Type
    {
        RaseUnnarmed, MeleeSlash, MeleeStab, BowSingle, BowMulti, BatSwing // Magi
    }
    public static bool MeleeAttack(Type t)
    {
        return Type.RaseUnnarmed == t || t == Type.MeleeSlash || Type.MeleeStab == t;
    }
    public static bool RangedAttack(Type t)
    {
        return Type.BowSingle == t || t == Type.BowMulti;
    }
    public void AddDamage(Damage damage)
    {
        damage.Add(damage);
    }
    public bool Equals (Attack other)
    {
        return
        other.type.Equals(type) &&
        other.rate.Equals(rate) &&
        other.range.Equals(range) && 
        other.damage.Equals(damage);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref bothHanded);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
        serializer.SerializeValue(ref type);
    }
    public override string ToString()
    {
        string s = "";

        s += $"Both handed: {bothHanded} | ";
        s += $"Damage: {damage} | ";
        s += $"Range: {range} tiles | ";
        s += $"Rate: {rate} ac/s | ";
        s += $"Time: {AttackTime} s | ";
        s += $"Attack Type: {Enum.GetName(typeof(Type), type)}";

        return s;
    }
}
