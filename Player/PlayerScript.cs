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
    [SerializeField] InputActionReference input_fire;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager.instance.PlayerSpawned();
            input_fire.action.performed += Fire;
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
        // Debug.Log("FIRE");
        HitServerRpc();
    }
    [ServerRpc]
    void HitServerRpc()
    {
        stats.TakeDamage(new Damage (Damage.Type.bludgeoning, 10));
        //Debug.Log("Take damage called");
    }
}
