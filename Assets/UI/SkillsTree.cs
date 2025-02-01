using System.Collections.Generic;
using UnityEngine;
//using Unity.Netcode;
/// <summary>
/// Sluzi na drzanie ziskanych schopnosti pre hraca na servery <br />
/// Ovplyvnuje vypocty hodnot pri boji a pod.
/// </summary>
public class SkillTree
{
#pragma warning disable IDE0044 // Add readonly modifier
    List<Skill> skills;
    PlayerStats player;
    Dictionary<Damage.Type, Modifier> offence;
    Dictionary<Damage.Type, float> offRate;
#pragma warning restore IDE0044 // Add readonly modifier

    public SkillTree ()
    {
        skills = new ();
        offence = new ();
        offRate = new ();
    }
    public SkillTree (PlayerStats _player)
    {
        skills = new ();
        offence = new ();
        offRate = new ();
        player = _player;
    }
    /// <summary>
    /// Pridanie schopnosti
    /// </summary>
    /// <param name="skill"></param>
    public void Add(Skill skill)
    {
        skills.Add(skill);
        string debug = $"For player {player.name} skill [{skill.name}] has been added as ";

        if (skill is ModDamage mD)
        {
            Damage.Type type = mD.condition;
            if (mD.damage)
            {
                debug += "Attack";
                if (mD.isSpeed)
                {
                    if (offRate.ContainsKey(type))
                        offRate[type] *= mD.amount;
                    else
                        offRate.Add(type, mD.amount);
                    debug += "Rate";
                }
                else
                {
                    if (offence.ContainsKey(type))
                        offence[type].Add(new (mD.amount, mD.isPercentyl));
                    else if (mD.isPercentyl)
                        offence.Add(type, new (mD.amount, mD.isPercentyl));
                    debug += "Damage";
                }
            }
            else
            {
                player.Defence.Add(new (type, mD.amount));
                debug += "Defence";
            }
            debug += " Modifier";
        }        
        else if (skill is ModSkill mS)
        {
            if (mS.isSpeed)
                player.TerrainChangeRpc(mS.amount);
            else
                player.AddMaxHealthRpc(mS.amount);

            debug += mS.isSpeed ? "Speed" : "Health";
            debug += " Modifier";
        }
        else if (skill is Utility ut)
        {
            player.UnlockUtilityRpc(ut);
            debug += "Utility";
        }
        else
            debug += "UNRECOGNIZED";

        Debug.Log(debug + $"\n{skill}");
    }
    /// <summary>
    /// Zikanie zmeneneho utoku podla ziskanych schopnosti
    /// </summary>
    /// <param name="baseAttack"></param>
    /// <returns></returns>
    public Attack ModAttack (Attack baseAttack)
    {
        // Zmeni utoku poskodenie
        Damage.Type dt = baseAttack.damage.type;
        float damageA = baseAttack.damage.amount;
        if (offence.ContainsKey(dt))
            damageA = offence[dt].ModifyValue(damageA);
        baseAttack.damage.amount = Mathf.RoundToInt(damageA);

        // Zmeni utkoku rychlost
        if (offRate.ContainsKey(dt))
            baseAttack.rate *= offRate[dt];

        return baseAttack;
    }
    /// <summary>
    /// Zostrucnenie zmeny
    /// </summary>
    private class Modifier
    {
        public float amount;
        public float percentyl;

        public Modifier(float a = 0, float p = 1)
        {
            amount = a;
            percentyl = p;
        }
        public Modifier(float ap, bool percent)
        {
            amount = percent ? 0 : ap;
            percentyl = percent ? ap : 1;
        }
        public void Add(Modifier mod)
        {
            this.amount += mod.amount;
            this.percentyl *= mod.percentyl;
        }
        public float ModifyValue(float value)
        {
            value *= percentyl;
            value -= amount;
            return value;
        }
    }
}