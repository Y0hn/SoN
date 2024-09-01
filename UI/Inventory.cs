using System.Collections.Generic;
using UnityEngine;
using System;
public class Inventory : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}

#region Items
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item"), Serializable] public class Item : ScriptableObject
{
    public new string name;
    public string description;
    public Sprite icon;
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] public class Equipment : Item
{
    public Rezistance rezistance;
    public Slot slot;
    public enum Slot
    {
        Head, Torso, Hands, Legs,
        Body
    }
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] public class Weapon : Item
{
    public Attack attack;
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Money"), Serializable] public class Coin : Item
{
    public int amount;
}
#endregion