using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

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
    [SerializeField]    protected Transform attackPoint;
    public NetworkVariable<bool> IsAlive = new();
    // Set up TO RASE
    [SerializeField]    protected NetworkVariable<Attack> attack = new ();
    protected const float timeToDespawn = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Rezistances setup
            for (int i = 0; i < Enum.GetNames(typeof(Damage.Type)).Length; i++)
            { rezists.Add(new Rezistance()); };
            /*
                Add racial (default) rezistences
            */

            // Attack setup
            /*
                Add racial (default) attack 
            */

            hp.Value = maxHp.Value;

            IsAlive.Value = true;
        }

        // Health values
        hp.OnValueChanged += (int prevValue, int newValue) => HpUpdate();
        maxHp.OnValueChanged += (int prevValue, int newValue) => 
        {
            if (IsServer)
                hp.Value = maxHp.Value;
        };
        hpBar.value = hp.Value;

        attackPoint.position = new(attackPoint.position.x, attack.Value.range);
    }
    protected virtual void Update()
    {

    }
    protected virtual void HpUpdate()
    {
        float value = (float)hp.Value / (float)maxHp.Value;
        hpBar.value = value;
        //Debug.Log($"HP bar: [{hpBar.value}/{hpBar.maxValue}] Acsual hp: [{hp.Value}/{maxHp.Value}] => {value}");
    }
    [ServerRpc]
    public virtual void TakeDamageServerRpc(Damage damage, ulong clientId)
    {
        var playerDamaged = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>();
        //Debug.Log($"Server Rpc hitting client {clientId}");
        if (playerDamaged != null && playerDamaged.IsAlive.Value)
        {
            playerDamaged.TakeDamage(damage);
        }
        /* playerDamaged.TakenDamageClientRpc(damage, new ClientRpcParams 
        { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });*/
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
    }
    public virtual void MeleeAttack()
    {
        if (!IsOwner) return;
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attack.Value.range /* , layer mask */);
        foreach (var target in targets)
        {
            if (target.TryGetComponent(out NetworkObject netObj))
            {
                ulong ownID = netObj.OwnerClientId;
                if (OwnerClientId != ownID)
                {
                    // Debug.Log("Attack area collided with player");
                    TakeDamageServerRpc(attack.Value.damage, ownID);
                }
            }
        }
    }
    public virtual bool AttackTrigger()
    {
        return false;
    }
    protected virtual void Die()
    {
        IsAlive.Value = false;
        hpBar.gameObject.SetActive(false);
        // Play death animation
        Destroy(gameObject, timeToDespawn);
    }
    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        attackPoint.position = new(attackPoint.position.x, attack.Value.range);
        Gizmos.DrawWireSphere(attackPoint.position, attack.Value.range);
    }
}

[Serializable] public struct Rezistance : INetworkSerializable, IEquatable<Rezistance>
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

[Serializable] public struct Attack : INetworkSerializable
{
    public Damage damage;
    public float range;
    public float rate;
    public Type type;
    public Attack (Damage damage, float range, float rate, Type type)
    {
        this.damage = damage;
        this.range = range;
        this.rate = rate;
        this.type = type;
    }
    public enum Type
    {
        Melee, Ranged //, Magic
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref rate);
    }
}

[Serializable] public struct Damage : INetworkSerializable
{
    public Type type;
    public int amount;
    public Damage (Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
    public enum Type
    {
        // STANDARD
        bludgeoning, slashing, piercing, 
        // ELEMENTAL
        cold, fire, holy, lightning, dark, acid,
        //  OVER TIME
        poison    
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref amount);
    }
}
