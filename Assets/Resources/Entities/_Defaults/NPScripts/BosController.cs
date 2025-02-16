using UnityEngine;

public class BosController : NPController
{
    /* ZDEDENE ATRIBUTY
     *  [SF] protected EntityStats stats;
     *  [SF] protected AIDestinationSetter destinationSetter;
     *  [SF] protected AIPath path;
     *  [SF] NPSensor sensor;
     *  protected Vector2 moveDir;
     *  protected bool attacking;
     *  protected const float MIN_MOVE_TOL = 0.1f;
     *  protected NextAction nextAction;
     *  protected float nextDecisionTimer = 0f;
     *  protected List<Transform> patrol = new();
     *  protected bool selfTarget;
     *
     *  public bool ForceDecision       { get; protected set; }
     *  protected Vector3 TargetPosition => sensor.ClosestTarget.position;    
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] protected GameObject canvas;
    protected new BosSensor sensor;

    protected bool dfTargeted;
    public new BosStats Stats => (BosStats)base.Stats;

    public override void OnNetworkSpawn()
    {
        SetDefaultTarget();
        if (IsServer)
        {
            sensor.targetChange += SetTarget;
            Stats.OnDeath += sensor.DisableSensor;
            path.endReachedDistance = Stats.AttackDistance;
        }
        base.OnNetworkSpawn();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Update()
    {
        /* base.Update(); *\
         * if (!IsServer || path == null || !Stats.IsAlive.Value) return;  
         * if (selfTarget && moveDir != Vector2.zero)
         * {
         *     viewDir = Vector2.zero;
         *     moveDir = Vector2.zero;
         *     attacking = false;   
         * }
         * else if (!selfTarget && attacking && path.reachedEndOfPath)
         * {
         *     if (!Stats.AboutToFire)
         *         TurnForTarget();
         *     Attack();
         * }
         * else if (!selfTarget)
         * {
         *     FollowTarget();
         * }
         * else
         *     SetTarget(defaultTarget);
         */
        
        if (!IsServer || path == null || !Stats.IsAlive.Value) return;
            
        if (selfTarget && moveDir != Vector2.zero)
        {
            viewDir = Vector2.zero;
            moveDir = Vector2.zero;
            attacking = false;   
        }
        else if (dfTargeted)
        {
            attacking = false;
            FollowTarget();
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
    /// <inheritdoc/> hlavnemu nepriatelovi
    /// </summary>
    /// <param name="t"></param>
    public override void SetTarget(Transform t)
    {
        if (t != null)
        {
            destinationSetter.target = t;
            selfTarget = false;
            dfTargeted = t == defaultTarget;
        }
        else
        {
            destinationSetter.target = transform;
            moveDir = Vector2.zero;
            selfTarget = true;
            dfTargeted = false;
        }

        canvas.SetActive(!(dfTargeted || selfTarget));
        attacking = false;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="t"></param>
    public override void SetDefaultTarget(string t = "")
    {
        defaultTarget = MapScript.map.BossSpawn;
        SetTarget(defaultTarget);
    }
    /// <summary>
    /// Nastavuje senzor
    /// </summary>
    /// <param name="s"></param>
    public void SetSensor(BosSensor s) 
    {
        Stats.Sensor = s;
        sensor = s;
    }
}