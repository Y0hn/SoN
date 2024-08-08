using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControler : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    InputActionReference input_move;

    public float speed = 1f;
    private Vector2 moveDir;
    private const float minC = 0.1f;

    void Start()
    {

    }
    void Update()
    {
        moveDir = input_move.action.ReadValue<Vector2>();
        moveDir = moveDir.normalized;
    }
    void FixedUpdate()
    {
        if (moveDir.magnitude > minC)
        {
            if (!animator.GetBool("move"))
                animator.SetBool("move", true);
            animator.SetFloat("horizontal", moveDir.x);
            animator.SetFloat("vertical", moveDir.y);
            rb.linearVelocity = moveDir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("move", false);
            CorrectGridPlacement();
        }
    }
    void CorrectGridPlacement()
    {
        float x = transform.position.x, y = transform.position.y;
        
        y = Mathf.Round(y*10)/10;
        switch (y%1)
        {
            case 0.1f:
            case 0.2f:
                y = Mathf.Floor(y);
                break;
            case 0.3f:
            case 0.4f:
            case 0.5f:
            case 0.6f:
            case 0.7f:
                y = Mathf.Floor(y) + 0.5f;
                break;
            case 0.8f:
            case 0.9f:
            case 0f:
                y = Mathf.Ceil(y);
                break;
        }

        switch (y/0.5%2)
        {
            case 0:
                x = Mathf.Round(x);
                break;
            case 1:
                x = Mathf.Round(x+0.5f)-0.5f;
                break;
        }

        rb.transform.position = new Vector3(x, y, rb.transform.position.z);
    }
}
