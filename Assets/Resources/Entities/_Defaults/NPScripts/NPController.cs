using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public class NPController : EntityController
{
    /* ZDEDENE ATRIBUTY
     *  [SF] protected EntityStats stats;
     *  protected Vector2 moveDir;
     *  protected bool attacking;
     *  protected const float MIN_MOVE_TOL = 0.1f;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] AIDestinationSetter destinationSetter;
    [SerializeField] AIPath path;
    [SerializeField] NPSensor sensor;
    protected NextAction nextAction;
    protected float nextDecisionTimer = 0f;
    protected List<Transform> patrol = new();
    protected bool selfTarget;

    protected new NPStats Stats => (NPStats)base.Stats;
    public bool ForceDecision       { get; protected set; }
    protected Vector3 TargetPosition => sensor.ClosestTarget.position;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ((NPStats)stats).OnHit += delegate { ForceDecision = true; };
        if (IsServer)
        {
            sensor.targetChange += SetTarget;
            stats.OnDeath += sensor.DisableSensor;
            path.endReachedDistance = ((NPStats)stats).AttackDistance;
        }
    }
    protected override void Update()
    {
        if (!IsServer || path == null || !stats.IsAlive.Value) return;

        /*if (nextDecisionTimer < Time.time || ForceDecision)
        {

            //DecideNextMove();
        }*/
            
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
    }
    protected override void Attack()
    {
        if (moveDir != Vector2.zero) moveDir = Vector2.zero;
        base.Attack();
    }
    protected virtual void TurnForTarget()
    {
        viewDir = TargetPosition - transform.position;
        viewDir = viewDir.normalized;
        // float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    protected virtual void FollowTarget()
    {
        if (viewDir != Vector2.zero) viewDir = Vector2.zero;
        Vector2 move = new (path.desiredVelocity.x*100, path.desiredVelocity.y*100);
        moveDir = move.normalized;
        if (!attacking) attacking = true; 
    }
    public virtual void SetTarget(Transform t)
    {
        if (t != null)
        {
            destinationSetter.target = sensor.ClosestTarget;
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
    protected override void AnimateMovement()
    {
        base.AnimateMovement();
        
        if (viewDir != Vector2.zero)
        {
            stats.Animator.SetFloat("horizontal", viewDir.x);
            stats.Animator.SetFloat("vertical", viewDir.y);
        }
    }/*
    */
    protected enum NextAction { GoToTarget, RunToTarget, AttackTarget, RunFromTarget, StayOnPlace, None }
}
