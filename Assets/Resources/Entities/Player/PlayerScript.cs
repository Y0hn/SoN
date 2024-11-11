using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
public class PlayerController : EntityController
{
    /* Inhereted variables
     *
     * [SF] protected Rigidbody2D rb;
     * [SF] protected Animator animator;
     * [SF] protected EntityStats stats;
     * protected bool attacking = false;
     * protected Vector2 moveDir;
     * protected const float minC = 0.1f;
     *
     */
    [SerializeField] GameObject cam;
    [SerializeField] InputActionReference input_move;
    [SerializeField] InputActionReference input_look;
    [SerializeField] InputActionReference input_attack;

    private GameManager game;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            game = GameManager.instance;
            game.PlayerSpawned((PlayerStats)stats);
            input_attack.action.started += Fire;
            input_attack.action.canceled += Fire;
            cam.SetActive(true);
        }
    }
    protected override void Update()
    {
        if (!IsOwner) return;

        if (stats.IsAlive.Value && game.PlayerAble)
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.R))
            {
                Inventory.instance.DropItem();
            }
            moveDir = input_move.action.ReadValue<Vector2>();
        }
        else if (moveDir != Vector2.zero)
            moveDir = Vector2.zero;
    }
    protected override void FixedUpdate()
    {
        if (!IsOwner) return;
        
        AnimateMovement();
    }
    public void Fire(InputAction.CallbackContext context)
    {
        if (!stats.IsAlive.Value)
        {
            //Debug.Log(name + " called RespawnServerRpc()");
            SetLiveServerRpc(OwnerClientId);
            return;
        }
            
        if      (context.started)
                attacking = true;
        else if (context.canceled)
            attacking = false;
    }
    protected override void Attack()
    {
        base.Attack();
        if (IsOwner)
            GameManager.instance.AnimateFace("hit");
    }
    [ServerRpc] protected void SetLiveServerRpc(ulong playerId)
    {
        NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
    }
}
