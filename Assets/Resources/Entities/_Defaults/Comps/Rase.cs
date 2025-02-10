using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// Drzi informacie, na zaklade ktorych sa vypnaju informacie o charakteroch
/// </summary>
[CreateAssetMenu(fileName = "NewRase", menuName = "Entity/Rase"), Serializable] public class Rase : ScriptableObject
{
    // urcuje odmenu za zabitie
    public byte level = 1;

    // urcuje rychlost chodze
    public float speed = 100;

    // urcuje pocet zivotov
    public int maxHp = 100;

    // urcuje velkost zorneho pola
    public float view = 5;

    // pole so zbranami pre charakter
    public Weapon[] weapons = new Weapon[1];

    // pole so zmenami zbrani () podla poctu zivotov
    public WeaponChange[] swapons = new WeaponChange[0];

    // List ochran proti poskodeniu
    public List<Resistance> resists = new();
}