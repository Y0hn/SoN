using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
public class PlayerControler : EntityControler
{
    /* Inhereted variables
     *
     * [SF] protected Rigidbody2D rb;
     * [SF] protected Animator animator;
     * [SF] protected EntityStats stats;
     * protected Vector2 moveDir;
     * protected const float minC = 0.1f;
     *
     */
    [SerializeField] GameObject cam;
    [SerializeField] InputActionReference input_move;
    [SerializeField] InputActionReference input_look;
    [SerializeField] InputActionReference input_attack;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager.instance.PlayerSpawned();
            input_attack.action.started += Fire;
            input_attack.action.canceled += Fire;
            cam.SetActive(true);
        }
    }
    protected override void Update()
    {
        if (!IsOwner)
            return;
        moveDir = input_move.action.ReadValue<Vector2>();
    }
    protected override void FixedUpdate()
    {
        if (IsOwner)
            AnimateMovement();
    }
    void Fire(InputAction.CallbackContext context)
    {
        if      (context.started)
            animator.SetBool("attack", true);
        else if (context.canceled)
            animator.SetBool("attack", false);
    }
}
