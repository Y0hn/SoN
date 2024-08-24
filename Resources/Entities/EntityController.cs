using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;
public class EntityControler : NetworkBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected EntityStats stats;
    protected Vector2 moveDir;
    protected const float minC = 0.1f;
    public override void OnNetworkSpawn()
    {

    }
    protected virtual void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {
        
    }
}
