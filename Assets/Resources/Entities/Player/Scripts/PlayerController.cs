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
        else if (Input.GetKey(KeyCode.L))
            ((PlayerStats)stats).AddXPRpc(10000);
    }
    void Q1(InputAction.CallbackContext context) { Q(1); }
    void Q2(InputAction.CallbackContext context) { Q(2); }
    void Q3(InputAction.CallbackContext context) { Q(3); }
    void Q (byte id)
    {
        id--;
        game.inventory.Quick(id);
    }
    [Rpc(SendTo.Server)] void DropRpc()
    {
        Vector2 pos = new (transform.position.x + Random.Range(-5, 6), transform.position.y + Random.Range(-5, 6));
        GameObject i = Instantiate(Resources.Load<GameObject>("Items/ItemDrop"), pos, Quaternion.identity);
        switch (Random.Range(1, 3))
        {
            case 1: i.GetComponent<ItemDrop>().Item = Item.GetItem("Items/weapons/sword-1");    break;
            case 2: i.GetComponent<ItemDrop>().Item = Item.GetItem("Items/weapons/bow-1");      break;
        } 
        i.GetComponent<NetworkObject>().Spawn();
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
            SetLiveRpc(/*OwnerClientId*/);
            return;
        }
        attacking = !context.canceled;
    }
    protected override void Attack()
    {
        base.Attack();
    }
    [Rpc(SendTo.Server)] protected void SetLiveRpc(/*ulong playerId*/)
    {
        //NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
        stats.IsAlive.Value = true;
    }
}
