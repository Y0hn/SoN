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
    public bool ForceDecision       { get; protected set; }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ((NPStats)stats).OnHit += delegate { ForceDecision = true; };
        if (IsServer)
        {
            sensor.targetChange += SetTarget;
            path.endReachedDistance = ((NPStats)stats).AttackDistance;
        }
    }
    protected override void Update()
    {
        if (!IsServer || path == null || !stats.IsAlive.Value) return;

        if (nextDecisionTimer < Time.time || ForceDecision)
        {

            //DecideNextMove();
        }
            
        if (selfTarget && moveDir != Vector2.zero)
        {
            viewDir = Vector2.zero;
            moveDir = Vector2.zero;
            attacking = false;   
        }
        else if (!selfTarget && attacking && path.reachedEndOfPath)
        {
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
        viewDir = sensor.ClosestTarget.position - transform.position;
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
    protected virtual void DecideNextMove()
    {
        float nextChange = 1f;
        bool inRange = false, gotInRange = false ;

        switch (((NPStats)stats).WC)
        {
            default:
                break;
        }
        switch (((NPStats)stats).DC)
        {
            default:
                break;
        }
        switch (((NPStats)stats).Behave)
        {
            case NPStats.Behavior.Scared:       DecideByTreshhold(1, inRange, gotInRange, out nextChange);      break;
            case NPStats.Behavior.Berserk:      DecideByTreshhold(0, inRange, gotInRange, out nextChange);      break;
            case NPStats.Behavior.Neutral:      DecideByTreshhold(0.5f, inRange, gotInRange, out nextChange);   break;
            case NPStats.Behavior.Agressive:    DecideByTreshhold(0.2f, inRange, gotInRange, out nextChange);   break;
            case NPStats.Behavior.Defesive:     DecideByTreshhold(0.7f, inRange, gotInRange, out nextChange);   break;
        }
        if (ForceDecision)
            ForceDecision = false;
        nextDecisionTimer = Time.time + nextChange;
    }
    void DecideByTreshhold(float tHP, bool inRange, bool gotRange, out float tDecay)
    {
        float hp = stats.HP;
        if      (hp < tHP && inRange)
        {
            nextAction = NextAction.RunFromTarget;
            tDecay = 3;
        }
        else if (gotRange && (!inRange || inRange && tHP < hp))
        {
            nextAction = NextAction.AttackTarget;
            tDecay = 0.5f;
        }
        else if (!gotRange && tHP < hp)
        {
            nextAction = NextAction.RunToTarget;
            tDecay = 0.2f;
        }
        else if (patrol.Count > 0)
        {
            nextAction = NextAction.GoToTarget;
            tDecay = 1;
        }
        else
        {
            nextAction = NextAction.StayOnPlace;
            tDecay = 3;
        }
    }*/
    protected enum NextAction { GoToTarget, RunToTarget, AttackTarget, RunFromTarget, StayOnPlace, None }
}
