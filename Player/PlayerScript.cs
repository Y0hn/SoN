using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
public class PlayerControler : NetworkBehaviour
{
    [SerializeField] GameObject cam;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] InputActionReference input_move;
    [SerializeField] InputActionReference input_fire;

    public float speed = 1f;
    Vector2 moveDir;
    const float minC = 0.1f;
    private 
    void Start()
    {
        if (IsOwner)
        {
            GameManager.instance.PlayerSpawned();
            input_fire.action.performed += Fire;
            cam.SetActive(true);
        }
    }
    void Update()
    {
        if (!IsOwner)
            return;
        moveDir = input_move.action.ReadValue<Vector2>();
    }
    void FixedUpdate()
    {
        if (!IsOwner)
            return;
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
