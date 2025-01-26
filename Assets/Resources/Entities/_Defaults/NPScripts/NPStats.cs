using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using Pathfinding;
public class NPStats : EntityStats
{
    /*  ZDEDENE ATRIBUTY
     *      
     *  [SF] protected TMP_Text nameTag;
     *  [SF] protected Rase rase;
     *  [SF] protected Slider hpBar;
     *  [SF] protected GameObject body;
     *  [SF] protected NetworkObject netObject;
     *  [SF] protected Transform attackPoint;
     *  [SF] protected SpriteRenderer weaponR, weaponL;
     *  [SF] protected NetworkAnimator animator;
     *  [SF] protected Rigidbody2D rb;
     *
     *  [SF] protected NetworkVariable<int> maxHp = new();
     *
     *  protected NetworkVariable<int> hp = new();
     *  protected NetworkVariable<Attack> attack = new ();
     *  protected NetworkVariable<FixedString128Bytes> weapRef = new();
     *  public NetworkVariable<bool> IsAlive = new(true);
     *  public NetworkVariable<float> speed = new();
     *  public NetworkVariable<byte> level = new(1);
     *
     *  public float HP                 { get { return (float)hp.Value/(float)maxHp.Value; } }
     *  public NetworkObject NetObject  { get { return netObject; } }
     *  public Rigidbody2D RigidBody2D  { get { return rb; } }
     *  public AITarget TargetTeam      { get { return aiTeam; } }
     *  public Animator Animator        { get { return animator.Animator; } }
     *  public bool AttackBoth          { get { return attack.Value.bothHanded; } }
     *  public bool Armed               { get { return equipment[(int)Equipment.Slot.WeaponL] != "" || "" !=  equipment[(int)Equipment.Slot.WeaponR]; } }
     *
     *  protected const float timeToDespawn = 0f;
     *  private bool clampedDMG = true;
     *  protected Defence defence;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] protected NPSensor sensor;
    [SerializeField] protected AIPath aIPath;
    [SerializeField] protected bool drawGizmo = false;
    protected float aToFire;
    public Action OnHit;

    // symbolizuje percento casu utoku kedy nedostava polohu ciela [+inacuracy => -presnost]
    protected const float 
        RANGED_ATTACK_INACURRACY = 0.4f,
        ATTACK_DISTANCE_PERCENTAGE = 0.3f;

    public static byte NPCount = 0;

    public override Quaternion Rotation => body.transform.rotation;
    public float AttackDistance             
    { 
        get 
        { 
            if      (Attack.IsMelee) 
                return Attack.range*2;
            else if (Attack.IsRanged)
                return Attack.range;
            else
                return 0f;
        }
    }
    public Defence.Class DC     { get; protected set; }
    public Weapon.Class WC      { get; protected set; }
    public bool AboutToFire    => aToFire <= Time.time;
    public float NextAttackTime=> atTime;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AddToCount(1);
        
        SetWeaponIndex(0,0);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        AddToCount(-1);
    }
    protected override void EntitySetUp()
    {
        base.EntitySetUp();
        if (IsServer)
        {
            sensor.me = aiTeam;
            sensor.SetRange(rase.view);
            aIPath.maxSpeed = speed.Value/100f;
        }
    }
    protected override void Update()
    {
        base.Update();
    }


    protected override void OwnerSubsOnNetValChanged()
    {
        if (!IsServer) return;
        base.OwnerSubsOnNetValChanged();
        speed.OnValueChanged += (float old, float now) =>
        {
            aIPath.maxSpeed = now/100f;
        };
    }
    protected override void OnHpUpdate(int prev, int now)
    {
        base.OnHpUpdate(prev, now);
        OnHit.Invoke();
    }
    protected virtual void AddToCount(sbyte b)
    {
        // iba na servery
        if (!IsServer) return;    
        NPCount = (byte)(NPCount + b);
    }
    public void SetAboutToFireTime(Projectile proj)
    {
        aToFire = proj.FireTime;
        aToFire *= 1 - RANGED_ATTACK_INACURRACY;
        aToFire += Time.time;
    }
    
    public override bool TakeDamage(Damage damage)
    {
        return base.TakeDamage(damage);
    }
    public override void KilledEnemy(EntityStats died)
    {
        
    }
#pragma warning disable IDE0051 // Remove unused private members
    void OnDrawGizmos()
    {
        if (drawGizmo)
            OnDrawGizmosSelected();
    }
    void OnDrawGizmosSelected()
    {
        float view = 0f;
        List<float> range = new();
        if (rase != null) 
        {
            view = rase.view;
            range.Add(Attack.range);
        }

        Gizmos.color = Color.magenta;
        if (view > 0)
            Gizmos.DrawWireSphere(transform.position, view);
        Gizmos.color = Color.blue;
        for (int i = 0; i < range.Count; i++)
            Gizmos.DrawWireSphere(transform.position, range[i]);
        Gizmos.color = Color.red;
        if (range.Count > 0)
        {
            List<Vector3> vecs = new();
            foreach (Weapon w in rase.weapons)
                foreach (Attack a in w.attack)
                    vecs.Add( new(transform.position.x, transform.position.y + a.range, a.range));

            foreach (Vector3 v in vecs)
            {
                Gizmos.DrawWireSphere(v, v.z);
                Gizmos.color *= 1.1f;
            }            
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer && collision.gameObject.layer.Equals(LayerMask.NameToLayer("Water")))
        {
            /*int php = hp.Value;
            hp.Value = 0;
            hp.OnValueChanged.Invoke(php, hp.Value);*/
            netObject.Despawn();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}