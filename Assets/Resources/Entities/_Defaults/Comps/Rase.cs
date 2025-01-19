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
    public Attack attack = new(new Damage(Damage.Type.bludgeoning, 1), 1, 1, Attack.Type.MeleeSlash);
    public List<Resistance> resists = new();
}