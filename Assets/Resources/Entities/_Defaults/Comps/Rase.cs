using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// Drzi informacie, na zaklade ktorych sa vypnaju informacie o charakteroch
/// </summary>
[CreateAssetMenu(fileName = "NewRase", menuName = "Entity/Rase"), Serializable] public class Rase : ScriptableObject
{
    // public GameObject body;
    public byte level = 1;
    public float speed = 100;
    public int maxHp = 100;
    public float view = 5;
    public Weapon[] weapons;
    public NPStats.WeaponChange[] swapons;
    public List<Resistance> resists = new();
}