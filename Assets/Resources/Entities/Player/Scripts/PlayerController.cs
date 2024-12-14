using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
using AYellowpaper.SerializedCollections;
public class PlayerController : EntityController
{
    /* Inhereted variables
     *
     *
     */
    [SerializeField] GameObject cam;
    [SerializedDictionary("Key", "Input"),SerializeField] SerializedDictionary<string, InputActionReference> input_actions;
    private GameManager game;
    private Vector2 lastView;
    private bool fromView;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            game = GameManager.instance;
            game.PlayerSpawned((PlayerStats)stats);
            input_actions["attack"].action.started += Fire;
            input_actions["attack"].action.canceled += Fire;
            input_actions["q1"].action.started += Q1;
            input_actions["q2"].action.started += Q2;
            input_actions["q3"].action.started += Q3;
            cam.SetActive(true);
            fromView = false;
        }
    }
    protected override void Update()
    {
        if (!IsOwner) return;

        if (stats.IsAlive.Value && game.PlayerAble)
        {
            base.Update();
            moveDir = input_actions["move"].action.ReadValue<Vector2>();
            viewDir = game.MousePos.normalized;
        }
        else if (moveDir != Vector2.zero)
            moveDir = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.P))
            DropRpc();        
    }
    void Q1(InputAction.CallbackContext context) { ((PlayerStats)stats).SetAttackTypeRpc(1); }
    void Q2(InputAction.CallbackContext context) { ((PlayerStats)stats).SetAttackTypeRpc(2); }
    void Q3(InputAction.CallbackContext context) { ((PlayerStats)stats).SetAttackTypeRpc(3); }
    [Rpc(SendTo.Server)] void DropRpc()
    {
        GameObject i = Instantiate(
        Resources.LoadAll<GameObject>("Items/ItemDrop")[0], 
            new Vector3(Random.Range(-11, 10), Random.Range(-11, 10), -3), 
            Quaternion.identity);
        i.GetComponent<NetworkObject>().Spawn();
        i.GetComponent<ItemDrop>().SetItemRpc("Items/weapons/sword-1");
    }
    protected override void FixedUpdate()
    {
        if (!IsOwner) return;        
        AnimateMovement();
    }
    protected override void AnimateMovement()
    {
        if (attacking)
        {
            stats.Animator.SetFloat("horizontal", viewDir.x);
            stats.Animator.SetFloat("vertical", viewDir.y);
            stats.RigidBody2D.linearVelocity = Vector2.zero;
            stats.Animator.SetBool("move", false);
        }
        base.AnimateMovement();
    }
    public void Fire(InputAction.CallbackContext context)
    {
        if (!stats.IsAlive.Value)
        {
            SetLiveServerRpc(OwnerClientId);
            return;
        }
        attacking = !context.canceled;
    }
    protected override void Attack()
    {
        base.Attack();
    }
    [ServerRpc] protected void SetLiveServerRpc(ulong playerId)
    {
        NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
    }
}
