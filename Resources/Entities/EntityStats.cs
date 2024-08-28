using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EntityControler))]
public class EntityStats : NetworkBehaviour
{
    // Server Autoritative
    [SerializeField]    protected TMP_Text nameTag;
    [SerializeField]    protected NetworkList<Rezistance> rezists = new();
    [SerializeField]    protected NetworkVariable<int> maxHp = new();
                        protected NetworkVariable<int> hp = new();
    [SerializeField]    protected Slider hpBar;
    [SerializeField]    public NetworkVariable<int> speed = new();
    [SerializeField]    protected AttackField atField;
    // MOVE TO RASE
    [SerializeField]    protected Damage damage;
    public bool IsAlive { get; protected set; }
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
        atField.SetDamage(damage);
    }
    protected virtual void Update()
    {

    }
    protected void HpUpdate()
    {
        float value = hp.Value / maxHp.Value;
        hpBar.value = value;
        //Debug.Log($"HP bar: [{hpBar.value}/{hpBar.maxValue}] Acsual: [{hp.Value}/{maxHp.Value}] => {value}");
    }
    // Currently handeld by animator
    [ServerRpc]
    public virtual void TakeDamageServerRpc(Damage damage, ulong clientId)
    {
        var playerDamaged = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>();

        if (playerDamaged != null && playerDamaged.IsAlive)
        {
            playerDamaged.TakeDamage(damage);
        }

        playerDamaged.TakenDamageClientRpc(damage, new ClientRpcParams 
        { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
    }
    public virtual void TakeDamage(Damage damage)
    {
        if (IsServer)
        {
            Debug.Log("Server is doing damage !");
            int newDamage = rezists[(int)damage.type].GetDamage(damage.amount);
            hp.Value -= newDamage;
            //Debug.Log($"Entity {name} damaged by {damage.amount}, protection absorbed {damage.amount-newDamage} final damage is {newDamage} HP[{hp.Value}/{maxHp.Value}]");
            
            if (hp.Value <= 0)
                Die();
        }
    }/*
    [ServerRpc]
    protected virtual void TakeDamageServerRpc(Damage damage)
    {
        Debug.Log("Server Rtc Take-Damage !");
        TakeDamage(damage);
    }*/
    protected virtual void Die()
    {
        IsAlive = false;
        hpBar.gameObject.SetActive(false);
        Destroy(gameObject, timeToDespawn);
    }
}

[Serializable]
public struct Rezistance : INetworkSerializable, IEquatable<Rezistance>
{
    private  int rezAmount; // Amount value     (-∞ <=> ∞)
    // Stacks with avg
    private  float rezTil;  // Precentil value  (-1 <=> 1)
    // Stacks with +

    public Rezistance (int amount, float percetil)
    {
        rezAmount = amount;
        rezTil = percetil;            
    }
    public void ModRez(int amount)      { rezAmount += amount;  }
    public void ModRez(float percetil)  { rezTil += percetil;   }
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
        //return other.rezAmount == rezAmount && other.rezTil == rezTil;
        return false;
    }
}

[Serializable]
public struct Damage : INetworkSerializable
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
    public Type type;
    public int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref amount);
    }
}
