using UnityEngine;
using Pathfinding;
/// <summary>
/// <inheritdoc/> Serverom
/// </summary>
public class NPController : EntityController
{
    /* ZDEDENE ATRIBUTY
     *  [SF] protected EntityStats stats;
     *  protected Vector2 moveDir;
     *  protected bool attacking;
     *  protected const float MIN_MOVE_TOL = 0.1f;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] protected AIDestinationSetter destinationSetter;
    [SerializeField] protected AIPath path;
    [SerializeField] protected NPSensor sensor;
    protected Transform defaultTarget;
    protected bool selfTarget;

    protected new NPStats Stats => (NPStats)base.Stats;
    public string DefaultTarget => defaultTarget.name;
    public bool ForceDecision       { get; protected set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Stats.OnHit += delegate { ForceDecision = true; };
        if (IsServer && sensor != null)
        {
            sensor.targetChange += SetTarget;
            Stats.OnDeath += sensor.DisableSensor;
            path.endReachedDistance = Stats.AttackDistance;
            
            if (Stats.HasSwapons)
            {
                Stats.OnHit += delegate { path.endReachedDistance = Stats.AttackDistance; };  
            }
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Update()
    {
        if (!IsServer || path == null || !Stats.IsAlive.Value) return;
            
        if (selfTarget && moveDir != Vector2.zero)
        {
            viewDir = Vector2.zero;
            moveDir = Vector2.zero;
            attacking = false;   
        }
        else if (!selfTarget && attacking && path.reachedEndOfPath)
        {
            if (!Stats.AboutToFire)
                TurnForTarget();
            Attack();
        }
        else if (!selfTarget)
        {
            FollowTarget();
        }
        else
            SetTarget(defaultTarget);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Attack()
    {
        if (moveDir != Vector2.zero) moveDir = Vector2.zero;
        base.Attack();
    }
    /// <summary>
    /// Otaca telo NPC graficky pri utoku
    /// </summary>
    protected virtual void TurnForTarget()
    {
        if (sensor.ClosestTarget == null) return;
        viewDir = sensor.ClosestTarget.position - transform.position;
        viewDir = viewDir.normalized;
    }
    /// <summary>
    /// Nastavuje parametre pre animovanie chodze charakteru pri utoku
    /// </summary>
    protected virtual void FollowTarget()
    {
        if (viewDir != Vector2.zero) viewDir = Vector2.zero;
        // ziskava chceny smer pohybu
        Vector2 move = new (path.desiredVelocity.x*100, path.desiredVelocity.y*100);

        moveDir = move.normalized;

        if (!attacking) attacking = true; 
    }
    /// <summary>
    /// Manualne nastavuje ciel AI navigacie
    /// </summary>
    /// <param name="t"></param>
    public virtual void SetTarget(Transform t)
    {
        if (t != null)
        {
            destinationSetter.target = t;
            selfTarget = false;
        }
        else
        {
            destinationSetter.target = transform;
            moveDir = Vector2.zero;
            selfTarget = true;
        }
        attacking = false;
    }
    /// <summary>
    /// Nastavuje ciel pri objaveni charakteru
    /// </summary>
    /// <param name="t"></param>
    public virtual void SetDefaultTarget(Transform t)
    {
        //FileManager.Log("Default target setting to " + t.name);
        defaultTarget = t;
        SetTarget(t);
    }
    /// <summary>
    /// Nastavuje ciel pri nacitani zo suboru charakteru
    /// </summary>
    /// <param name="t"></param>
    public virtual void SetDefaultTarget(string t)
    {
        //Debug.Log("Default target setted to " + t.name);
        defaultTarget = MapScript.map.RequestDefaultTarget(t);
        SetTarget(defaultTarget);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void AnimateMovement()
    {
        base.AnimateMovement();
        
        if (viewDir != Vector2.zero)
        {
            Stats.Animator.SetFloat("horizontal", viewDir.x);
            Stats.Animator.SetFloat("vertical", viewDir.y);
        }
    }
}
