using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(EntityStats))]
public class EntityController : NetworkBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected NetworkAnimator animator;
    [SerializeField] protected EntityStats stats;

    protected bool attacking = false;
    protected Vector2 moveDir;
    protected const float minC = 0.1f;

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
        if (animator == null)
            return;
        if (moveDir.magnitude > minC)
        {
            if (!animator.Animator.GetBool("move"))
                animator.Animator.SetBool("move", true);

            animator.Animator.SetFloat("horizontal", moveDir.x);
            animator.Animator.SetFloat("vertical", moveDir.y);

            rb.linearVelocity = moveDir * stats.speed.Value;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.Animator.SetBool("move", false);
        }
    }
    protected virtual void Attack()
    {
        if (stats.AttackTrigger())
        {
            float atBlend = animator.Animator.GetFloat("atBlend") * -1;
            animator.Animator.SetFloat("atBlend", atBlend);
            animator.Animator.SetTrigger("attack");
        }
    }
}
