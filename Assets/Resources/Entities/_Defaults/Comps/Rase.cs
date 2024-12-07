using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewRase", menuName = "Entity/Rase"), Serializable] public class Rase : ScriptableObject
{
    // public GameObject body;
    public float speed = 1;
    public int maxHp = 100;
    public float view = 5;
    public Attack attack = new(new Damage(Damage.Type.bludgeoning, 1), 1, 1, Attack.Type.MeleeSlash);
    public Armor naturalArmor;
}