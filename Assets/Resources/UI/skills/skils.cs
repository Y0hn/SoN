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
    /// <summary>
    /// Sluzi na posielanie objektov po sieti
    /// </summary>
    /// <typeparam name="T">Typova hodnota</typeparam>
    /// <param name="serializer">samodopnane</param>
    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
    }
    /// <summary>
    /// Porovnanie dvoch schonosti
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Skill other)
    {
        return this.name == other.name;
    }
    /// <summary>
    /// Vypis vlastnosti schopnosti
    /// </summary>
    /// <returns>vypis PARAMETROV</returns>
    public override string ToString()
    {
        string s = base.ToString();
        s += $" >>> ID: {name}";
        return s;
    }
}
/// <summary>
/// Schopnost meni hodnotu nejakeho parametra <br />
/// (Tu su vsetky Skilly pre upravu zivotov)
/// </summary>
[Serializable] public class ModSkill : Skill
{
    public float amount;    // ak zaporne tak sa jedna o rychlost inak zivoty
    /// <summary>
    /// PRAVDA ak (amount * 100 % 100 != 0)
    /// </summary>
    public bool isPercentyl => amount * 100 % 100 != 0;
    /// <summary>
    /// PRAVDA ak (amount < 0)
    /// </summary>
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
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        string s = base.ToString();
        s += $" => Amount: {amount} {(isPercentyl ? "%" : "+")} {(isSpeed ? "time" : "")}";
        return s;
    }
}
/// <summary>
/// Schopnost meni hodnotu len ak je spnena podmienka - rovnaky typ damage <br />
/// (Tu su schopnosti co menia obranu alebo utok)
/// </summary>
[Serializable] public class ModDamage : ModSkill
{
    public bool damage;
    public Damage.Type condition;
    public ModDamage ()
    {
        condition = Damage.Type.None;
        damage = false;
    }
    /// <summary>
    /// Vytvori novu obranu alebo utok podla parametrov
    /// </summary>
    /// <param name="n">IDENTIFIKATOR</param>
    /// <param name="a">MNOZTSTVO</param>
    /// <param name="c">PODMIENKA uplatnenia</param>
    /// <param name="r">PRAVDA ak urcuje rychlost</param>
    /// <param name="d">PRAVDA ak je utok</param>
    public ModDamage (string n, float a, Damage.Type c, bool r, bool d = false) : base (n, a, r)
    {
        condition = c;
        damage = d;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc/></typeparam>
    /// <param name="serializer"><inheritdoc/></param>
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref condition);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        string s = base.ToString();
        s += $", Damage: {damage}, Element: {Enum.GetName(typeof(Damage.Type), condition)}";
        return s;
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
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc/></typeparam>
    /// <param name="serializer"><inheritdoc/></param>
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref aquired);
        serializer.SerializeValue(ref function);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        string s = base.ToString();
        s += $" => Function: {Enum.GetName(typeof(Function), function)}, Aquired= {aquired}";
        return s;
    }
    /// <summary>
    /// Urcuje typ specialnej schopnosti teda to co odomkyna
    /// </summary>
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