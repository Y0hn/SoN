using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Projectile : NetworkBehaviour
{
    [SerializeField] NetworkObject networkObject;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D coll;
    [SerializeField] Transform line;

    EntityStats shooter;
    Damage damage;
    float graficDelay;
    float delay;
    float range;

    private Vector2 startPos = Vector2.positiveInfinity;
    private Vector3 force;
    private float[] timer = { -1, -1};
    private float RangeLimit => range*2;
    const float FORCE = 1000;

    public override void OnNetworkSpawn()
    {
        //Debug.Log("Projectile Lauched with attack: " + damage.amount);
        coll.enabled = false;
        sprite.enabled = false;
        
        timer[0] = Time.time + delay;
        timer[1] = Time.time + graficDelay;
        line.gameObject.SetActive(false);
    }
#pragma warning disable IDE0051 // Remove unused private members
    void Update()
    {
        if (timer[0] > 0 && Time.time >= timer[0])  // vystrelenie projektilu
        {
            transform.SetParent(null);
            coll.enabled = true;
            force = transform.up;
            force *= FORCE;
            startPos = transform.position;
            rb.AddForce(force);
            timer[0] = 0;
        }
        else if (timer[1] > 0 && Time.time >= timer[1])  // vykreslenie textury
        {
            line.localScale = new (line.localScale.x,RangeLimit);
            sprite.enabled = true;
            timer[1] = 0;
        }
        else if (coll.enabled)  // range limit
        {
            float distance = Vector2.Distance(startPos, transform.position);
            if (distance >= RangeLimit)
                Stop();
            line.localScale = new (line.localScale.x,distance);
        }
    }
    void FixedUpdate()
    {
        // Toci sa okolo strelca podla toho kam mieri
        if (timer[0] > 0 && shooter.ViewAngle != transform.rotation.z)
        {
            RotateAroundPoint();
        }
    }
    void RotateAroundPoint()
    {
        transform.position = shooter.AttackPoint.position;
        transform.rotation = shooter.AttackPoint.rotation;
        transform.Rotate(new Vector3(0,0,1), shooter.ViewAngle, Space.Self);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.TryGetComponent(out EntityStats et) && et != shooter)
        {
            if (et.TakeDamage(damage))
                shooter.KilledEnemy(et);
            networkObject.Despawn();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    public void SetUp(Attack attack, EntityStats entityStats)
    {
        delay = attack.AttackTime   * 2/3;
        graficDelay = delay         * 1/3;
        damage = attack.damage;
        range = attack.range;
        shooter = entityStats;

        Debug.Log($"Shoted projectile \nwith attack: {attack}");
    }
    public void Stop()
    {
        networkObject.Despawn();
    }
}
