using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Drzi vsetky obranny a vypocitava ich ucinok
/// </summary>
[Serializable] public class Defence
{
    List<Resistance> resistances;
    /// <summary>
    /// Vznik na cisto
    /// </summary>
    public Defence()
    {
        resistances = new();
    }
    /// <summary>
    /// Vznik z jednej obrany
    /// </summary>
    /// <param name="res"></param>
    public Defence(Resistance res)
    {
        resistances = new();
        Add(res);
    }
    /// <summary>
    /// Vznik z listu obran
    /// </summary>
    /// <param name="resists"></param>
    public Defence(List<Resistance> resists)
    {
        resistances = new();
        resists.ForEach(res => Add(res));
    }
    /// <summary>
    /// Prida obranu
    /// </summary>
    /// <param name="r"></param>
    /// <returns>PRAVDA ak uspesne</returns>
    public bool Add     (Resistance r)
    {
        if (!resistances.Contains(r))
        {
            resistances.Add(r);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Odsrani konkretnu ochranu 
    /// </summary>
    /// <param name="a"></param>
    /// <returns>PRAVDA ak uspesne</returns>
    public bool Remove  (Resistance a)
    {
        if (resistances.Contains(a))
        {
            resistances.Remove(a);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Vypocita poskodenie na zaklade predchadzajucej hodnoty a hodnty obrany proti danemu elementu
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clamp"></param>
    /// <returns>vysledne p</returns>
    public int CalculateDMG(Damage damage, bool clamp = true)
    {
        List<Resistance> list = resistances.FindAll(r => r.defenceType == damage.type);

        float sum = 0f, per = 0f;
        list.ForEach(r=>
        {   if (r.amount < 1)
                per = (per < r.amount && r.amount < 1) ? r.amount: per;     // scita Pocetne rezisty
            else
                sum += (r.amount > 1) ? r.amount : 0f;                      // najvacsi percentualne Rezisty
        });

        per *= damage.amount;  // nastavi ciselnu vysku

        int recieved = Mathf.RoundToInt(damage.amount - (sum + per));
        if (clamp)
            recieved = Math.Clamp(recieved, 0, int.MaxValue);
        return recieved;
    }
}
