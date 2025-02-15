using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// Pouziva sa na utocenie
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] 
public class Weapon : Equipment
{
    public List<Attack> attack;
    public List<Sound> clips;   // pre kazdy utok jeden zvuk
    /// <summary>
    /// Ziska predmet na zaklade referencnej cesty
    /// Tato metoda ma byt "prepisana" (overwrite)
    /// </summary>
    /// <param name="referency">cesta</param>
    /// <returns>ZBRAN</returns>
    public new static Weapon GetItem (string referency)
    {
        return Resources.Load<Weapon>(referency);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string GetReferency
    {
        get 
        { 
            if (path != "")
                return path;
            return FileManager.WEAPONS_DEFAULT_PATH + "/" + name; 
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string SpriteRef => FileManager.WEAPONS_REF_DEFAULT_PATH + "/" + sprite;
    public override void Use(ItemSlot iS)
    {
        base.Use(iS);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="other"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override bool Equals(Item other)
    {
        bool eq = false;
        if (other is Weapon)
        {
            eq = base.Equals(other);
            var w = (Weapon)other;
            eq &= attack.Equals(w.attack);
        }
        return eq;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override string ToString()
    {
        string atts = "";
        attack.ForEach(att => atts += att.ToString() + "\n");
        return 
            base.ToString() + "\n" +
            $"Attacks: \n( {atts} \n)";
    }
}
