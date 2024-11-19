using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(EntityStats))]
public abstract class EntityController : NetworkBehaviour
{
    [SerializeField] protected EntityStats stats;

    protected bool attacking = false;
    protected Vector2 moveDir;
    protected const float MIN_MOVE_TOL = 0.1f;

    public override void OnNetworkSpawn()
    {
        
    }
    protected virtual void Update()
    {
        if (attacking)
            Attack();
    }
    protected virtual void FixedUpdate()
    {
        if (IsServer)
            AnimateMovement();
    }
    protected virtual void AnimateMovement()
    {
        if (stats.Animator == null)
            return;
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
        if (stats.AttackTrigger())
        {
            float atBlend = stats.Animator.GetFloat("atBlend") * -1;
            stats.Animator.SetFloat("atBlend", atBlend);
            stats.Animator.SetTrigger("attack");
        }
    }
}
