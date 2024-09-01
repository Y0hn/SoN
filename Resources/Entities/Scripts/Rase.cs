using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using AYellowpaper.SerializedCollections;
using UnityEditor.Animations;

[CreateAssetMenu(fileName = "NewRase", menuName = "Entity/Rase"), Serializable] public class Rase : ScriptableObject, INetworkSerializable, IEquatable<Rase>
{
    public Attack attack = new(new Damage(Damage.Type.bludgeoning, 1), 1, 1, Attack.Type.Melee);
    // public GameObject gameObject;
    //public AnimatorOverrideController animator;
    public GameObject body;
    public Color color = new (1,1,1,1);
    public float speed = 1;
    public int maxHp = 100;
    [SerializedDictionary("Type", "rezist")]
    public SerializedDictionary<string, Rezistance> rezistances = new()
    {
        {Enum.GetName(typeof(Damage.Type), 0), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 1), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 2), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 3), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 4), new Rezistance()},

        {Enum.GetName(typeof(Damage.Type), 5), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 6), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 7), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 8), new Rezistance()},
        {Enum.GetName(typeof(Damage.Type), 9), new Rezistance()}
        // ...
    };
    public bool Equals(Rase other)
    {
        return false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref attack);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref maxHp);
    }
}