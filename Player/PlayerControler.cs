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
    [SerializeField]
    InputActionReference input_fire;

    public float speed = 1f;
    private Vector2 moveDir;
    private const float minC = 0.1f;

    void Start()
    {
        input_fire.action.performed += Fire;
    }
    void Update()
    {
        moveDir = input_move.action.ReadValue<Vector2>();
    }
    void FixedUpdate()
    {
        if (moveDir.magnitude > minC)
        {
            if (!animator.GetBool("move"))
                animator.SetBool("move", true);
            animator.SetFloat("horizontal", moveDir.x);
            animator.SetFloat("vertical", moveDir.y);
            rb.linearVelocity = moveDir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("move", false);
        }
    }
    void Fire(InputAction.CallbackContext context)
    {
            Debug.Log("FIRE");
    }
}
