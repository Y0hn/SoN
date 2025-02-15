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
    [SerializeField] string releaseSound;

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
            shooter.PlaySoundRpc(releaseSound);
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
    /// <summary>
    /// Kontroluje ci bol casovac dosiahnuty a ci je platny
    /// </summary>
    /// <param name="timer">casovaca</param>
    /// <returns>PRAVDA ak Casovac je povoleny a uz uplynul</returns>
    bool TimerReached(float timer)
    {
        return 0 < timer && timer <= Time.time;
    }
    /// <summary>
    /// Otaca sa okolo strelca
    /// </summary>
    void RotateAroundPoint()
    {
        transform.SetPositionAndRotation(shooter.AttackPoint.position, shooter.AttackPoint.rotation);
        transform.Rotate(new Vector3(0,0,1), shooter.ViewAngle, Space.Self);
    }
    /// <summary>
    /// Projektil narazil na nieco
    /// </summary>
    /// <param name="other"></param>
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
    /// <summary>
    /// Uvodne nastavenie atributov projektilu
    /// </summary>
    /// <param name="attack">utok strelca</param>
    /// <param name="entityStats">strelec</param>
    public void SetUp(EntityStats entityStats)
    {
        Attack a = entityStats.Attack;

        delay = 0;
        graficDelay = 0;

        damage = a.damage;
        range = a.range;
        /*delay = a.AttackTime * 2/3;
        graficDelay = delay  * 1/3;*/

        shooter = entityStats;
        shooter.OnDeath += TryToDestoy;
        //Debug.Log($"Shoted projectile \nwith attack: {attack}\nwith shoot out delay: {delay}\ngrafical delay: {graficDelay}");
    }
    /// <summary>
    /// Zastavi strelu ak este neboa vystrelena
    /// </summary>
    public void StopAttack()
    {
        if (timers[0] != 0)
            TryToDestoy();
    }
    /// <summary>
    /// Znici projektil, bud iba lokalne alebo po celej sieti <br />
    /// Taktiez ho odboberie z akcie po smrti strelca
    /// </summary>
    void TryToDestoy()
    {
        shooter.OnDeath -= TryToDestoy;
        if      (networkObject.IsSpawned)
            networkObject.Despawn();
        else
            Destroy(gameObject);
    }
}
