using UnityEngine;
using System;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewRase", menuName = "Entity/Rase"), Serializable] public class Rase : ScriptableObject
{
    // public GameObject body;
    public byte level = 1;
    public float speed = 100;
    public int maxHp = 100;
    public float view = 5;
    public Weapon[] weapons;
    public List<Resistance> resists = new();
}