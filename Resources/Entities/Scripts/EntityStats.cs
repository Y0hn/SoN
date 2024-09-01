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
    [SerializeField]    public NetworkVariable<float> speed = new();
    [SerializeField]    protected GameObject body;
    [SerializeField]    protected Transform attackPoint;
                        public NetworkVariable<bool> IsAlive = new();
    [SerializeField]    protected Animator animator;
    [SerializeField]    protected Rase rase;
                        protected NetworkVariable<Attack> attack = new ();
                        protected const float timeToDespawn = 0f;

    public override void OnNetworkSpawn()
    {
        RaseSetUp();

        if (IsServer)
            IsAlive.Value = true;

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
    protected virtual void RaseSetUp()
    {
        if (body != null)
            Destroy(body);
        body = Instantiate(rase.body, transform.position, Quaternion.identity, transform);
        body.name = "Body";

        animator.Rebind();
        animator.Update(0);

        GetComponent<EntityControler>().animate = animator;

        attackPoint = body.transform.GetChild(0);
        
        if (IsServer)
        {
            attack.Value = rase.attack;

            maxHp.Value = rase.maxHp;
            hp.Value = maxHp.Value;

            speed.Value = rase.speed;

            for (int i = 0; i < rase.rezistances.Count; i++)
                rezists.Add(rase.rezistances[Enum.GetName(typeof(Damage.Type), i)]);    
        }
    }
    protected virtual void Update()
    {

    }
    protected virtual void HpUpdate()
    {
        float value = (float)hp.Value / (float)maxHp.Value;
        hpBar.value = value;
    }
    [ServerRpc]
    public virtual void TakeDamageServerRpc(Damage damage, ulong clientId, ulong senderId)
    {
        var playerDamaged = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>();
        //Debug.Log($"Server Rpc hitting client {clientId}");
        if (playerDamaged != null && playerDamaged.IsAlive.Value)
        {
            playerDamaged.TakeDamage(damage/*, senderId*/);
        }
    }
    public virtual void TakeDamage(Damage damage)
    {
        if (IsServer)
        {
            Debug.Log("Server is doing damage !");
            int newDamage = rezists[(int)damage.type].GetDamage(damage.amount);
            hp.Value -= newDamage;
            
            if (hp.Value <= 0)
                Die();
        }
        else
            Debug.Log("Take damage on client");
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
                    TakeDamageServerRpc(attack.Value.damage, ownID, OwnerClientId);
                    Debug.Log($"{OwnerClientId} attacking player {ownID}");
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
        //Debug.Log("Killed by " + killer);
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
    [field: SerializeField] private  int rezAmount; // Amount value     (-∞ <=> ∞)
    // Stacks with avg
    [field: SerializeField] private  float rezTil;  // Precentil value  (-1 <=> 1)
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
