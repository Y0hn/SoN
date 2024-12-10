using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] NetworkObject networkObject;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D coll;
    public Damage damage;
    private const float FORCE = 100;
    public override void OnNetworkSpawn()
    {
        // nevola sa pri spawne objktu
        Debug.Log("Projectile Lauched with attack: " + damage.amount);
        Vector2 force = new(0, FORCE);
        rb.AddForce(force);
    }

#pragma warning disable IDE0051 // Remove unused private members
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.TryGetComponent(out EntityStats et) /*&& et.TargetTeam != me*/)
        {
            et.TakeDamage(damage);
            networkObject.Despawn();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}
