using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] NetworkObject networkObject;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D coll;
    [SerializeField] Transform line;
    [SerializeField] SpriteRenderer lineSpr;

    EntityStats shooter;
    Damage damage;
    float graficDelay;
    float delay;
    float range;

    private Vector2 startPos = Vector2.positiveInfinity;
    private Vector3 force;
    private float[] timers;
    private float RangeLimit => range*2;
    public float FireTime => delay;
    const float FORCE = 1000;

    public override void OnNetworkSpawn()
    {
        //Debug.Log("Projectile Lauched with attack: " + damage.amount);
        coll.enabled = false;
        sprite.enabled = false;
        //lineSpr.enabled = false;
        
        timers = new float[3];
        timers[0] = Time.time + delay;
        timers[1] = Time.time + graficDelay;
        timers[2] = Time.time + graficDelay*0.5f;
    }
#pragma warning disable IDE0051 // Remove unused private members
    void Update()
    {
        if      (TimerReached(timers[2]))   // vykreslenie drahy projektilu
        {
            line.localScale = new (line.localScale.x,RangeLimit);
            lineSpr.enabled = true;
            timers[2] = 0;
        }
        else if (TimerReached(timers[1]))  // vykreslenie textury
        {
            sprite.enabled = true;
            timers[1] = 0;
        }
        else if (TimerReached(timers[0]))  // vystrelenie projektilu
        {
            transform.SetParent(null);
            coll.enabled = true;
            force = transform.up;
            force *= FORCE;
            startPos = transform.position;
            rb.AddForce(force);
            timers[0] = 0;
        }
        else if (coll.enabled)              // limitovanie letu projektilu
        {
            float distance = Vector2.Distance(startPos, transform.position);
            if (distance >= RangeLimit)
                TryToDestoy();
            line.localScale = new (line.localScale.x,distance);
        }
    }
    void FixedUpdate()
    {
        // Toci sa okolo strelca podla toho kam mieri
        if (0 < timers[0] && shooter.ViewAngle != transform.rotation.z)
        {
            RotateAroundPoint();
        }
    }
    bool TimerReached(float timer)
    {
        return 0 < timer && timer <= Time.time;
    }
    void RotateAroundPoint()
    {
        transform.SetPositionAndRotation(shooter.AttackPoint.position, shooter.AttackPoint.rotation);
        transform.Rotate(new Vector3(0,0,1), shooter.ViewAngle, Space.Self);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.TryGetComponent(out EntityStats et) && et != shooter)
        {
            if (et.TakeDamage(damage))
                shooter.KilledEnemy(et);
            TryToDestoy();
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
        shooter.OnDeath += TryToDestoy;
        //Debug.Log($"Shoted projectile \nwith attack: {attack}\nwith shoot out delay: {delay}\ngrafical delay: {graficDelay}");
    }
    public void StopAttack()
    {
        if (timers[0] != 0)
            TryToDestoy();
    }
    void TryToDestoy()
    {
        shooter.OnDeath -= TryToDestoy;
        if      (/*!IsServer && */networkObject.IsSpawned)
            networkObject.Despawn();
        else //if (!networkObject.IsSpawned)
            Destroy(gameObject);
    }
}
