using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
using AYellowpaper.SerializedCollections;
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
        new Vector3(Random.Range(-11, 10), 
        Random.Range(-11, 10), -3), 
        Quaternion.identity);
        i.GetComponent<NetworkObject>().Spawn();
        i.GetComponent<ItemDrop>().SetItemRpc("Items/weapons/sword-1");
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

        Vector2 pos = game.MousePos.normalized;
        pos = RoundVector(pos, 1);
        string p = "";
        
        if      (pos.x < 0)
            p += "L";
        else if (pos.x > 0)
            p += "R";
        if      (pos.y < 0)
            p += "D";
        else if (pos.y > 0)
            p += "U";
        /*
        if (p != "")
            Debug.Log(p + "pos: " + $"[{pos.x},{pos.y}]");*/

        attacking = !context.canceled;
    }
    protected override void Attack()
    {
        base.Attack();
        /*if (IsOwner)
            GameManager.instance.AnimateFace("hit");*/
    }
    [ServerRpc] protected void SetLiveServerRpc(ulong playerId)
    {
        NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
    }
}
