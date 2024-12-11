using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] NetworkObject networkObject;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D coll;
    public Damage damage;
    public float delay = 0f;
    public float range = 0f;
    public float graficDelay = 0f;
    private const float FORCE = 1000;
    Vector3 force;
    Vector2 startPos = Vector2.positiveInfinity;
    private float[] timer = { -1, -1};
    public override void OnNetworkSpawn()
    {
        //Debug.Log("Projectile Lauched with attack: " + damage.amount);
        coll.enabled = false;
        sprite.enabled = false;
        
        timer[0] = Time.time + delay;
        timer[1] = Time.time + graficDelay;
    }
    void Update()
    {
        if (timer[0] > 0 && Time.time >= timer[0])
        {
            coll.enabled = true;
            force = transform.up;
            force *= FORCE;
            startPos = transform.position;
            rb.AddForce(force);
            timer[0] = 0;
        }
        if (timer[1] > 0 && Time.time >= timer[1])
        {
            sprite.enabled = true;
        }

        if (coll.enabled && Vector2.Distance(startPos, transform.position) >= range)
        {
            networkObject.Despawn();
        }
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
