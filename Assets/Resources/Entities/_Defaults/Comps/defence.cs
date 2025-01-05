using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable] public class Defence
{
    List<Resistance> resistances;
    public Defence()
    {
        resistances = new();
    }
    public Defence(Resistance res)
    {
        resistances = new();
        Add(res);
    }
    public Defence(List<Resistance> resists)
    {
        resistances = new();
        resists.ForEach(res => Add(res));
    }
    public bool Add     (Resistance r)
    {
        if (!resistances.Contains(r))
        {
            resistances.Add(r);
            return true;
        }
        return false;
    }
    public bool Remove  (Resistance a)
    {
        if (resistances.Contains(a))
        {
            resistances.Remove(a);
            return true;
        }
        return false;
    }
    
    public int CalculateDMG(Damage damage, Resistance additionalRezists = null, bool clamp = true)
    {
        List<Resistance> list = resistances.FindAll(r => r.defenceType == damage.type);
        if (additionalRezists != null)
            list.Add(additionalRezists);

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
    public Class CallculateDC()
    {
        if (resistances.Count > 0)
        {
            float total = 0;
            int count = 0;

            foreach (Resistance r in resistances)
            {
                total += (r.amount > 1) ? r.amount : r.amount * 100;
                count++;
            }
            total /= count; 
            
            if      (total < 40)
                return Class.Small;
            else if (total < 70)
                return Class.Medium;
            else if (count < 5)
                return Class.Dedicated;
            else
                return Class.Heavy;
        }
        else
            return Class.None;
    }
    public enum Class  { None, Small, Medium, Heavy, Dedicated }
}
