using System;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Pre ovladanie Entit
/// </summary>
[RequireComponent(typeof(EntityStats))]
public abstract class EntityController : NetworkBehaviour
{
    [SerializeField] protected EntityStats stats;
    protected Vector2 moveDir;
    protected float stepTimer;
    protected bool attacking;
    protected Vector2 viewDir = Vector2.zero;
    protected const float MIN_MOVE_TOL = 0.1f;
    protected EntityStats Stats => stats; 
    public virtual Vector2 View => viewDir;
    protected float atTime;

    /// <summary>
    /// Vykovana sa pri Vzniku objektu v ramci siete
    /// </summary>
    public override void OnNetworkSpawn()
    {
        moveDir = Vector2.zero;
        attacking = false;
        Stats.OnDeath += delegate { attacking = false; };
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
        if (Stats.Animator == null) return;
        if (moveDir.magnitude > MIN_MOVE_TOL)
        {
            if (!Stats.Animator.GetBool("move"))
                Stats.Animator.SetBool("move", true);

            Stats.Animator.SetFloat("horizontal", moveDir.x);
            Stats.Animator.SetFloat("vertical", moveDir.y);

            float mod = Stats.speed.Value * Time.deltaTime;
            Stats.RigidBody2D.linearVelocity = moveDir * mod;
            
            if (stepTimer == 0)
            {
                stepTimer = Time.time + 1/(Stats.speed.Value/100f)/8f;
            }
            if (stepTimer < Time.time)
            {
                Stats.PlaySoundRpc("step");
                stepTimer = Time.time + 1/(Stats.speed.Value/100f)/2f;
            }
        }
        else
        {
            Stats.RigidBody2D.linearVelocity = Vector2.zero;
            Stats.Animator.SetBool("move", false);
            stepTimer = 0;
        }
    }
    /// <summary>
    /// Skusa vykonat utok
    /// </summary>
    protected virtual void Attack()
    {
        if (atTime < Time.time)
        {
            atTime = Time.time + Stats.Attack.AttackTime;

            // zanimuje utok
            Stats.Animator.SetTrigger("attack");
        }
    }
    /// <summary>
    /// Pokusi sa vymenit ruku pocas utoku
    /// </summary>
    public void SwitchHand()
    {
        // sem sa dostane ak moze utocit (uz utoci) 
        // ak je casovac utoku < cas
        if (Stats.AttackBoth)   
        {
            // ak utok pouziva obe ruky tak sa meni utociaca ruka
            float atBlend = Stats.Animator.GetFloat("atBlend") * -1;
            Stats.Animator.SetFloat("atBlend", atBlend);
        }
    }
}
