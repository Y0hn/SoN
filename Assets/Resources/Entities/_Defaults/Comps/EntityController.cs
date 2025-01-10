using Unity.Netcode;
using UnityEngine;
/// <summary>
/// For controlloing Entities
/// </summary>
[RequireComponent(typeof(EntityStats))]
public abstract class EntityController : NetworkBehaviour
{
    [SerializeField] protected EntityStats stats;
    protected Vector2 moveDir;
    protected bool attacking, wasAttacking;
    protected Vector2 viewDir = Vector2.zero;
    protected const float MIN_MOVE_TOL = 0.1f;
    public Vector2 View     { get => viewDir; }
    public override void OnNetworkSpawn()
    {
        moveDir = Vector2.zero;
        attacking = false;
    }
    protected virtual void Update()
    {
        if      (attacking)
            Attack();
        else if (wasAttacking)
            AttackInterupt();
    }
    protected virtual void FixedUpdate()
    {
        if (IsServer)
            AnimateMovement();
    }
    protected virtual void AnimateMovement()
    {
        if (stats.Animator == null) return;
        if (moveDir.magnitude > MIN_MOVE_TOL)
        {
            if (!stats.Animator.GetBool("move"))
                stats.Animator.SetBool("move", true);

            stats.Animator.SetFloat("horizontal", moveDir.x);
            stats.Animator.SetFloat("vertical", moveDir.y);

            float mod = stats.speed.Value * Time.deltaTime;
            stats.RigidBody2D.linearVelocity = moveDir * mod;
        }
        else
        {
            stats.RigidBody2D.linearVelocity = Vector2.zero;
            stats.Animator.SetBool("move", false);
        }
    }
    protected virtual void Attack()
    {
        stats.Animator.SetBool("interupAttck", false);
        if (stats.AttackTrigger())
        {
            wasAttacking = true;
            if (stats.AttackBoth)
            {
                float atBlend = stats.Animator.GetFloat("atBlend") * -1;
                stats.Animator.SetFloat("atBlend", atBlend);
            }
            stats.Animator.SetTrigger("attack");
        }
    }
    protected virtual void AttackInterupt()
    {
        wasAttacking = false;
        if (stats.TryInteruptAttack())
        {
            stats.Animator.ResetTrigger("attack");
            stats.Animator.SetBool("interupAttck", true);
            Debug.Log("Attack interupted");
        }
    }
    /*protected Vector2 RoundVector(Vector2 v, byte d = 1)
    {
        return new(Round(v.x,d), Round(v.y,d));
    }
    protected float Round(float f, byte d = 1)
    {
        return Mathf.Round(f*d)/(float)d;
    }*/
}
