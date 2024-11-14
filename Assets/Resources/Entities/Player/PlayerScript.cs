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
            moveDir = input_move.action.ReadValue<Vector2>();
        }
        else if (moveDir != Vector2.zero)
            moveDir = Vector2.zero;
        

        if (Input.GetKeyDown(KeyCode.P))
            DropRpc();        
    }
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

        Vector2 pos = game.mousePos.normalized;
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

        if (p != "")
            Debug.Log(p + "pos: " + $"[{pos.x},{pos.y}]");
            
        if      (context.started)
                attacking = true;
        else if (context.canceled)
            attacking = false;
    }
    protected Vector2 RoundVector(Vector2 v, byte d = 1)
    {
        return new(Round(v.x,d), Round(v.y,d));
    }
    protected float Round(float f, byte d = 1)
    {
        return Mathf.Round(f*d)/(float)d;
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
