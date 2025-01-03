using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
[Serializable] public class SkillTree
{
    List<Skill> skills;

    Dictionary<Damage.Type, float> offence;
    Dictionary<Damage.Type, float> defence;

    public SkillTree ()
    {
        skills = new ();
        defence = new ();
        offence = new ();
    }
    public void Add(Skill skill)
    {
        skills.Add(skill);

        if (skill is Health h)
        {
            // change max health
        }
        else if (skill is Combat c)
        {
            // urci ci je offensivny
            if      (0 < c.amount)
            {
                if (offence.ContainsKey(c.condition))
                    offence[c.condition] += c.amount;
                else
                    offence.Add(c.condition, c.amount);
            }
            // ak je defenzivny urci ci je percentilny
            else if (-1 <= c.amount)
            {
                if (defence.ContainsKey(c.condition) && defence[c.condition] < c.amount)
                    defence[c.condition] += c.amount;
                else
                    defence.Add(c.condition, c.amount);
            }
            else // if (c.amount < -1)  // je pocetny defenzivny
            {
                if (defence.ContainsKey(c.condition))
                    defence[c.condition] += c.amount;
                else
                    defence.Add(c.condition, c.amount);                
            }
        }
    }
    // NoNeedForRemovalYet

    public Damage GetDamage(Damage.Type type)           
    {
        if (defence.ContainsKey(type))
        {
            return new Damage (type, (int)offence[type]); 
        }
        return new ();
    }
    public Armor.Resistance GetResist(Damage.Type type) 
    { 
        if (defence.ContainsKey(type))
        {
            return new Armor.Resistance(type, Mathf.Abs(defence[type]));
        }
        return null;
    }
    [Serializable] public class Skill : INetworkSerializable
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
    [Serializable] public class Health : Skill
    {
        public int amount;
        public Health ()
        {
            amount = 0;
        }
        public Health (string n, int a) : base (n)
        {
            amount = a;
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref amount);
        }
    }
    [Serializable] public class Combat : Skill
    {
        /// <summary>
        /// + ak offence
        /// - ak defence 
        /// </summary>
        public float amount;   
        // |amount| > 1 => pocetny 
        // |amount| < 1 => percentny    (offence nieje nikdy v percentach)
        public Damage.Type condition;
        public Combat ()
        {
            amount = 0;
            condition = Damage.Type.bludgeoning;
        }
        public Combat (string n, float a, Damage.Type c) : base (n)
        {
            amount = a;
            condition = c;
        }
        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref amount);
        }
    }/*
    [Serializable] public class ModArmor : Combat
    {
        
    }*/
    [Serializable] public class ModAttack : Combat
    {
        public bool rate;
        public ModAttack ()
        {
            rate = false;
        }
        public ModAttack (string n, float a, Damage.Type c, bool r) : base (n, a, c)
        {
            rate = r;
        }
        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref rate);
        }
    }
    [Serializable] public class Utility : Skill
    {
        public Function function;

        public Utility ()
        {

        }
        public Utility (string n, Function f) : base (n)
        {
            function = f;
        }
        public enum Function
        {
            None, 
            ViewOwnHP, ViewOwnMeleeAttack, ViewOwnRangedAttack,
            ViewOthersHP, ViewOthersMeleeAttack, ViewOthersRangedAttack,
            NightVision, 
            VisionSizeIncrease, 
            ViewCorruptionAndBecomeImune
        }
    }
}