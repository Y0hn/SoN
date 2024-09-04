using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
using Unity.VisualScripting;
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager.instance.PlayerSpawned((PlayerStats)stats);
            input_attack.action.started += Fire;
            input_attack.action.canceled += Fire;
            cam.SetActive(true);
        }
    }
    protected override void Update()
    {
        if (!IsOwner) return;

        if (stats.IsAlive.Value)
        {
            base.Update();

            moveDir = input_move.action.ReadValue<Vector2>();
        }
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
    [ServerRpc] protected void SetLiveServerRpc(ulong playerId)
    {
        NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
    }
}
