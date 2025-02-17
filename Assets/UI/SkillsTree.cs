using System.Collections.Generic;
using UnityEngine;
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
    string[] uSkils;
#pragma warning restore IDE0044 // Add readonly modifier
    
    /// <summary>
    /// Ziska Udaje o stromu schopnosti pre ulozenie do suboru
    /// </summary>
    /// <returns></returns>
    public World.PlayerSave.SkillTreeSave SkillTreeSave => new (skills.ToArray(), uSkils);

    /// <summary>
    /// Vytvorenie cisto noveho stromu schopnosti
    /// </summary>
    public SkillTree ()
    {
        skills = new ();
        offence = new ();
        offRate = new ();
        uSkils = new string[2];
    }
    /// <summary>
    /// Vytvorenie noveho stromu schopnosti pre konkretneho hraca
    /// </summary>
    /// <param name="_player"></param>
    public SkillTree (PlayerStats _player)
    {
        skills = new ();
        offence = new ();
        offRate = new ();
        uSkils = new string[2];
        player = _player;
    }
    /// <summary>
    /// Pridanie schopnosti do listu aktivnych schopnosti
    /// </summary>
    /// <param name="skill">pridadvana SCHOPNOST</param>
    public void Add(Skill skill)
    {
        // prida schopnost do listu schopnosti
        skills.Add(skill);

        string debug = $"For player {player.name} skill [{skill.name}] has been added as ";

        if (skill is ModDamage mD)      // ak schopnost meni pocet prichadajuceho/odchadzajuceho poskodenia
        {
            Damage.Type type = mD.condition;
            if (mD.damage)
            {
                debug += "Attack";
                if (mD.isSpeed)
                {
                    if (offRate.ContainsKey(type))
                        offRate[type] *= Mathf.Abs(mD.amount);
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
        else if (skill is ModSkill mS)  // ak schopnost meni vlastnost charaktera
        {
            if (mS.isSpeed)
            {
                // Prida hracovi rychlost
                player.TerrainChangeRpc(Mathf.Abs(mS.amount));
                debug += "Speed";
            }
            else
            {
                // Prida hracovi maximalne zivoty
                player.AddMaxHealthRpc(mS.amount);
                debug += "Health";
            }
            
            debug += " Modifier";
        }
        else if (skill is Utility ut)   // ak je schopnost specialna
        {
            // Prida odomknutu schopnost
            player.UnlockUtilityRpc(ut);
            debug += "Utility";
        }
        else                            // inak nepatri medzi schopnosti
            debug += "UNRECOGNIZED";

        // Zapise zaznam o pridani
        FileManager.Log(debug + $"\n{skill}", FileLogType.RECORD);
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
        /// <summary>
        /// Vytvara menic hodnoty podla parametrov 
        /// </summary>
        /// <param name="ap">HODNOTA zmeny</param>
        /// <param name="percent">PRAVDA ak je percentualna</param>
        public Modifier(float ap, bool percent)
        {
            amount = percent ? 0 : ap;
            percentyl = percent ? ap : 1;
        }
        /// <summary>
        /// Pridava hodnotu menenia
        /// </summary>
        /// <param name="mod"></param>
        public void Add(Modifier mod)
        {
            this.amount += mod.amount;
            this.percentyl = Mathf.Max(percentyl, mod.percentyl);
        }
        /// <summary>
        /// Zmenenie hodnoty o zmenu
        /// </summary>
        /// <param name="value">povodna HODNOTA</param>
        /// <returns>zmenena HODNOTA</returns>
        public float ModifyValue(float value)
        {
            value *= percentyl;
            value -= amount;
            return value;
        }
    }
}