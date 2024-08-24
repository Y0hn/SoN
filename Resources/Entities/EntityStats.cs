using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EntityStats : NetworkBehaviour
{
    // Server Autoritative
    [SerializeField] protected TMP_Text nameTag;
    protected NetworkList<Rezistance> rezists = new();
    [SerializeField] protected NetworkVariable<int> maxHp = new();
    protected NetworkVariable<int> hp = new();
    [SerializeField] protected Slider hpBar;
    [SerializeField] public float speed;

    protected const float timeToDespawn = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Rezistances setup
            for (int i = 0; i < Enum.GetNames(typeof(Damage.Type)).Length; i++)
            { rezists.Add(new Rezistance()); };

            hp.Value = maxHp.Value;
        }

        // Health values
        hp.OnValueChanged += (int prevValue, int newValue) => HpUpdate();
        maxHp.OnValueChanged += (int prevValue, int newValue) => 
        {
            if (IsServer)
                hp.Value = maxHp.Value;
        };
        hpBar.value = hp.Value;
    }
    protected virtual void Update()
    {

    }
    protected void HpUpdate()
    {
        float value = (float)hp.Value / (float)maxHp.Value;
        hpBar.value = value;
        //Debug.Log($"HP bar: [{hpBar.value}/{hpBar.maxValue}] Acsual: [{hp.Value}/{maxHp.Value}] => {value}");
    }
    public virtual void TakeDamage(Damage damage)
    {
        if (IsServer)
        {
            int newDamage = rezists[(int)damage.type].GetDamage(damage.amount);
            hp.Value -= newDamage;
            //Debug.Log($"Entity {name} damaged by {damage.amount}, protection absorbed {damage.amount-newDamage} final damage is {newDamage} HP[{hp.Value}/{maxHp.Value}]");
        }
        if (hp.Value <= 0)
            Die();
    }
    public virtual void Die()
    {
        hpBar.gameObject.SetActive(false);
        Destroy(gameObject, timeToDespawn);
    }
}
public struct Rezistance : INetworkSerializable, IEquatable<Rezistance>
{
    private  int rezAmount; // Amount value     (-∞ <=> ∞)
    private  float rezTil;  // Precentil value  (-1 <=> 1)
    // Stacks with +

    public Rezistance (int amount = 0, float percetil = 0)
    {
        rezAmount = amount;
        rezTil = percetil;            
    }
    public void ModRez(int amount) { rezAmount += amount;  }
    public void ModRez(float percetil) { rezTil += percetil;   }
    public int GetDamage(int damage)
    {
        return (int)Mathf.Round(damage * (1 - rezTil) - rezAmount);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref rezAmount);
        serializer.SerializeValue(ref rezTil);
    }
    public bool Equals(Rezistance other)
    {
        throw new NotImplementedException();
    }
}
public readonly struct Damage
{
    public enum Type
    {
        // STANDARD
        bludgeoning, slashing, piercing, 
        // ELEMENTAL
        cold, fire, holy, lightning, dark, acid,
        //  OVER TIME
        poison    
    }
    public readonly Type type;
    public readonly int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
