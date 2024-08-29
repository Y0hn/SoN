using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EntityStats))]
public class EntityControler : NetworkBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
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
        if (moveDir.magnitude > minC)
        {
            if (!animator.GetBool("move"))
                animator.SetBool("move", true);

            animator.SetFloat("horizontal", moveDir.x);
            animator.SetFloat("vertical", moveDir.y);

            rb.linearVelocity = moveDir * stats.speed.Value;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("move", false);
        }
    }
    /*protected virtual void AnimateAttack()
    {

    }*/
    protected virtual void Attack()
    {
        if (stats.AttackTrigger())
        {
            float atBlend = animator.GetFloat("atBlend") * -1;
            animator.SetFloat("atBlend", atBlend);
            animator.SetTrigger("attack");
        }
    }
}
