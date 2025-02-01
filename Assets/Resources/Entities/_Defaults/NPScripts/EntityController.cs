using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Pre ovladanie Entit
/// </summary>
[RequireComponent(typeof(EntityStats))]
public abstract class EntityController : NetworkBehaviour
{
    [SerializeField] protected EntityStats stats;
    [SerializeField] protected AudioSource step;
    protected Vector2 moveDir;
    protected float stepTimer;
    protected bool attacking;
    protected Vector2 viewDir = Vector2.zero;
    protected const float MIN_MOVE_TOL = 0.1f;
    protected EntityStats Stats => stats; 
    public virtual Vector2 View => viewDir;

    /// <summary>
    /// Vykovana sa pri Vzniku objektu v ramci siete
    /// </summary>
    public override void OnNetworkSpawn()
    {
        moveDir = Vector2.zero;
        attacking = false;

        step.maxDistance = 10f;
    }
    /// <summary>
    /// Stara sa aby sa co narychlejsie vykonala zmena spravania
    /// 
    /// V zaklade len strazi ci charakter chce utocit => pokusa sa o utok
    /// alebo nie
    /// </summary>
    protected virtual void Update()
    {
        if (attacking)
            Attack();
    }
    /// <summary>
    /// Stara sa o animacie
    /// </summary>
    protected virtual void FixedUpdate()
    {
        if (IsServer)
            AnimateMovement();
    }
    /// <summary>
    /// Stara sa o Animovanie a Ozvucenie Pohybu charakteru (chodze)
    /// </summary>
    protected virtual void AnimateMovement()
    {
        if (stats.Animator == null) return;
        if (moveDir.magnitude > MIN_MOVE_TOL)
        {
            if (!stats.Animator.GetBool("move"))
                stats.Animator.SetBool("move", true);

            stats.Animator.SetFloat("horizontal", moveDir.x);
            stats.Animator.SetFloat("vertical", moveDir.y);

            float mod = stats.speed.Value * Time.deltaTime;
            stats.RigidBody2D.linearVelocity = moveDir * mod;
            
            if (stepTimer == 0)
            {
                stepTimer = Time.time + 1/(stats.speed.Value/100f)/8f;
            }
            if (stepTimer < Time.time)
            {
                step.Play();
                stepTimer = Time.time + 1/(stats.speed.Value/100f)/2f;
            }
        }
        else
        {
            stats.RigidBody2D.linearVelocity = Vector2.zero;
            stats.Animator.SetBool("move", false);
            stepTimer = 0;
        }
    }
    /// <summary>
    /// Skusa vykonat utok
    /// </summary>
    protected virtual void Attack()
    {
        if (stats.AttackTrigger())
        {
            // sem sa dostane ak moze utocit (uz utoci) 
            // ak je casovac utoku < cas
            if (stats.AttackBoth)   
            {
                // ak utok pouziva obe ruky tak sa meni utociaca ruka
                float atBlend = stats.Animator.GetFloat("atBlend") * -1;
                stats.Animator.SetFloat("atBlend", atBlend);
            }
            // zanimuje utok
            stats.Animator.SetTrigger("attack");
        }
    }
}
