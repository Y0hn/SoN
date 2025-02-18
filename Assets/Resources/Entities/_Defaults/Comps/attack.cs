using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;
[Serializable] public struct Attack : INetworkSerializable, IEquatable<Attack>
{
    public Damage damage;
    public float range;
    public float rate;
    public bool bothHanded;

    public readonly float AttackTime=> 1/rate;
    public readonly bool IsMelee    => damage.Melee; 
    public readonly bool IsRanged   => damage.Ranged; 
    public readonly bool IsSet      => range != 0 && rate != 0; 

    public Attack (Damage damage, float range, float rate, bool both = false)
    {
        this.bothHanded = both;
        this.damage = damage;
        this.range = range;
        this.rate = rate;
    }
    public Attack (Attack attack)
    {
        this.bothHanded =  attack.bothHanded;
        this.damage =  attack.damage;
        this.range = attack.range;
        this.rate =  attack.rate;

    }
    /// <summary>
    /// Utoku prida poskodenie
    /// </summary>
    /// <param name="damage">pridane poskodenie</param>
    public void AddDamage(Damage damage)
    {
        damage.Add(damage);
    }
    /// <summary>
    /// Utok zautoci NaBlizku / NaDialku
    /// </summary>
    /// <param name="self">utocnik</param>
    /// <returns></returns>
    public List<EntityStats> Trigger(EntityStats self)
    {
        List<EntityStats> etS = new();

        if (IsMelee)
            MeleeTrigger(ref self, ref etS);
        else // if ranged
            RangedTrigger(ref self);

        return etS;
    }
	/// <summary>
    /// Získa vśetky entity okrem seba, ktoré sú v dosahu (beźí na servery) a zautoci na ne
    /// </summary>
    /// <returns>vráti pole získaných entít</returns>
    private static void MeleeTrigger(ref EntityStats self, ref List<EntityStats> list)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(self.AttackPoint.position, self.Attack.range /*, layer mask */);
        foreach (Collider2D target in targets)
            if (target.TryGetComponent(out EntityStats stats))
                if (stats != self)
                    list.Add(stats);
    }
	/// <summary>
	/// Z strelnej zbrane získa projektil, ktorý vytvori a nastaví mu hodnoty
	/// </summary>
    private static void RangedTrigger(ref EntityStats self)
    {
        Ranged r = (Ranged)self.EquipedWeapon;
        GameObject p = Ranged.Projectile(self.Attack.damage.type);
        Transform par = self.transform;
        
        p = GameObject.Instantiate(p, par.position, self.Rotation);
        Projectile proj = p.GetComponent<Projectile>();
        proj.SetUp(self);
        NetworkObject netP = p.GetComponent<NetworkObject>();
        netP.Spawn(true);
        netP.TrySetParent(par);

        if      (self is PlayerStats plS)
            plS.Projectile = proj;
        else if (self is NPStats npS)
            npS.SetAboutToFireTime(proj);
    }
    /// <summary>
    /// Porovnavac
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals (Attack other)
    {
        return
        other.rate.Equals(rate) &&
        other.range.Equals(range) && 
        other.damage.Equals(damage);
    }
    /// <summary>
    /// Posielanie po sieti
    /// </summary>
    /// <param name="serializer"></param>
    /// <typeparam name="T"></typeparam>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref bothHanded);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
    }
    /// <summary>
    /// Vypis
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string s = "";

        s += $"Both handed: {bothHanded} | ";
        s += $"Damage: {damage} | ";
        s += $"Range: {range} tiles | ";
        s += $"Rate: {rate} ac/s | ";
        s += $"Time: {AttackTime} s | ";

        return s;
    }
}
