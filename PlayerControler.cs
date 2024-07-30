using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControler : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    PlayerInput playerInput;
    const float minC = 0.1f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float 
            h = Input.GetAxisRaw("hor"),
            v = Input.GetAxisRaw("ver");

        //InputSystem_Actions.PlayerActions.Move(h, v);
        Vector2 move = new Vector2(h, v);
        move = move.normalized;

        if (move.magnitude > minC)
        {
            animator.SetBool("walk", true);
            animator.SetFloat("hor", h);
            animator.SetFloat("ver", v);

            rb.linearVelocity = move;
        }
        else if (rb.linearVelocity.magnitude > 0)
        {
            animator.SetBool("walk", false);
            rb.linearVelocity = Vector2.zero;
        }

    }
}
