using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
using AYellowpaper.SerializedCollections;
/// <summary>
/// Umoznuje klientovi ovladat svoj charakter
/// </summary>
public class PlayerController : EntityController
{
    /* ZDEDENE ATRIBUTY
     * [SF] protected EntityStats stats;
     * [SF] protected AudioSource step;
     * protected Vector2 moveDir;
     * protected float stepTimer;
     * protected bool attacking;
     * protected Vector2 viewDir = Vector2.zero;
     * protected const float MIN_MOVE_TOL = 0.1f;
     * protected EntityStats Stats => stats; 
     * public virtual Vector2 View => viewDir;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] GameObject cam;
    [SerializedDictionary("Key", "Input"),SerializeField] SerializedDictionary<string, InputActionReference> input_actions;
#if UNITY_EDITOR
    [SerializeField] Equipment[] equipmentPool;
#endif
    private GameManager game;
    protected bool wasAttacking;
    protected new PlayerStats Stats => (PlayerStats)base.Stats;


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
    
        if (Stats.IsAlive.Value && game.PlayerAble)
        {
            base.Update();

            if (!attacking && wasAttacking)
                AttackInterupt();

            moveDir = input_actions["move"].action.ReadValue<Vector2>();
            viewDir = game.MousePos.normalized;
        }
        else if (moveDir != Vector2.zero)
            moveDir = Vector2.zero;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            int e = Random.Range(0, equipmentPool.Length);
            Stats.DropRpc(equipmentPool[e].GetReferency, new(2,2));
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Stats.AddLvlRpc();
        }
#endif
    }
    void Q1(InputAction.CallbackContext context) { Q(1); }
    void Q2(InputAction.CallbackContext context) { Q(2); }
    void Q3(InputAction.CallbackContext context) { Q(3); }
    /// <summary>
    /// Pokusa sa aktivovat quick slot ako aktuany utko
    /// </summary>
    /// <param name="id"></param>
    void Q (byte id)
    {
        id--;
        game.inventory.Quick(id);
    }
    protected override void FixedUpdate()
    {
        if (!IsOwner) return;        
        AnimateMovement();
    }
    /// <summary>
    /// Animuje pohyb alebo utko a rotaciu pocas neho
    /// </summary>
    protected override void AnimateMovement()
    {
        if (attacking)
        {
            Stats.Animator.SetFloat("horizontal", viewDir.x);
            Stats.Animator.SetFloat("vertical", viewDir.y);
            Stats.RigidBody2D.linearVelocity = Vector2.zero;
            Stats.Animator.SetBool("move", false);
        }
        base.AnimateMovement();
    }
    /// <summary>
    /// Po kiknuti mysi sa utok povoli/zakaze <br />
    /// alebo hrac znovuzrodi
    /// </summary>
    /// <param name="context"></param>
    public void Fire(InputAction.CallbackContext context)
    {
        if (!Stats.IsAlive.Value)
        {
            SetLiveRpc(/*OwnerClientId*/);
            return;
        }
        attacking = !context.canceled;
    }

    protected override void Attack()
    {
        Stats.Animator.ResetTrigger("interuptAttack");
        base.Attack();
        wasAttacking = true;
    }
    /// <summary>
    ///  Prerusi utok ak hrac prestane drzat mys 
    /// </summary>
    protected virtual void AttackInterupt()
    {
        wasAttacking = false;
        if (Stats.TryInteruptAttack())
        {
            stats.Animator.SetTrigger("attack");
            stats.Animator.SetTrigger("interuptAttack");
            Debug.Log("Attack interupted");
        }
    }

    // RPCs
    /// <summary>
    /// Znovu zrodi hraca
    /// </summary>
    [Rpc(SendTo.Server)] protected void SetLiveRpc(/*ulong playerId*/)
    {
        //NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStats>().IsAlive.Value = true;
        Stats.IsAlive.Value = true;
    }
}
