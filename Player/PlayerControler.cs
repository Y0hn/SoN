using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControler : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    InputActionReference input_move;

    public float speed = 1f;
    private Vector2 moveDir;
    private const float minC = 0.1f;

    void Start()
    {

    }
    void Update()
    {
        moveDir = input_move.action.ReadValue<Vector2>();
        moveDir = moveDir.normalized;
    }
    void FixedUpdate()
    {
        if (moveDir.magnitude > minC)
        {
            if (!animator.GetBool("walk"))
                animator.SetBool("walk", true);
            animator.SetFloat("hor", moveDir.x);
            animator.SetFloat("ver", moveDir.y);
            rb.linearVelocity = moveDir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("walk", false);
        }
    }
}
