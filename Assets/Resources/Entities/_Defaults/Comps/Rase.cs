using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections.Generic;
using System;
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

    [SerializedDictionary("name","sound")] 
    public SerializedDictionary<string, Sound> sounds = new()
    {
        { "grassStep1", new(null, 0.05f) },
        { "grassStep2", new(null, 0.05f) },
        { "grassStep3", new(null, 0.05f) },

        { "stoneStep1", new(null, 0.5f ) },
        { "stoneStep2", new(null, 0.5f ) },
        { "stoneStep3", new(null, 0.5f ) },

        { "onHitted1",  new(null, 0.5f ) },
        { "onHitted2",  new(null, 0.5f ) },
        { "onHitted3",  new(null, 0.5f ) },
        
        { "onDeath",    new(null, 1) },
    };
}