using Unity.Netcode;
using UnityEngine;
public class EntityControler : NetworkBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected EntityStats stats;
    protected Vector2 moveDir;
    protected const float minC = 0.1f;
    public override void OnNetworkSpawn()
    {
        
    }
    protected virtual void Update()
    {
        
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

            rb.linearVelocity = moveDir * stats.speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("move", false);
        }
    }
}
