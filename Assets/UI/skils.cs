using System;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Abstrakna trieda z, ktorej su zdedene vsekty schopnosti
/// </summary>
[Serializable] public abstract class Skill : INetworkSerializable
{
    public string name;
    public Skill ()
    {
        name = "";
    }
    public Skill (string n)
    {
        name = n;
    }
    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
    }
}
/// <summary>
/// Schopnost meni hodnotu nejakeho parametra <br />
/// (Tu su vsetky Skilly pre upravu zivotov)
/// </summary>
[Serializable] public class ModSkill : Skill
{
    public float amount;    // ak zaporne tak sa jedna o rychlost inak zivoty
    public bool isPercentyl => amount * 100 % 100 != 0;
    public bool isSpeed => amount < 0;
    public ModSkill ()
    {
        amount = 0;
    }
    public ModSkill (string n, float a, bool s = false) : base (n)
    {
        if (s)
            a *= -1;
        amount = a;
    }
    public virtual float ModifyValue(float value)
    {
        float a = Mathf.Abs(amount);
        return isPercentyl ? (1+a) * value : a + value;
    }
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref amount);
    }
}
/// <summary>
/// Schopnost meni hodnotu len ak je spnena podmienka - rovnaky typ damage <br />
/// (Tu su schopnosti co menia obranu)
/// </summary>
[Serializable] public class ModDamage : ModSkill
{
    public bool damage;
    public Damage.Type condition;
    public ModDamage ()
    {
        condition = Damage.Type.None;
    }
    public ModDamage (string n, float a, Damage.Type c, bool d = false) : base (n, a)
    {
        condition = c;
        damage = d;
    }
    public ModDamage (string n, float a, Damage.Type c, bool r, bool d = false) : base (n, a, r)
    {
        condition = c;
        damage = d;
    }
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref condition);
    }
}
/// <summary>
/// Schopnost odomkyna skytu funkciu <br />
/// (Sem patria len specialne schopnosti)
/// </summary>
[Serializable] public class Utility : Skill
{
    public bool aquired;
    public Function function;
    public Utility ()
    {
        aquired = false;
        function = Function.None;
    }
    public Utility (string n, Function f, bool a = false) : base (n)
    {
        function = f;
        aquired = a;
    }
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref aquired);
        serializer.SerializeValue(ref function);
    }
    public enum Function
    {
        None, 
        ViewOwnHP, ViewOwnMeleeAttack, ViewOwnRangedAttack,
        ViewOthersHP, ViewOthersMeleeAttack, ViewOthersRangedAttack,
        ViewOthersDefence, 
        VisionSizeIncrease, 
        ViewCorruptionAndBecomeImune
    }
}