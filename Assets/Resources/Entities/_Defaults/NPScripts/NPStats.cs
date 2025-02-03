using UnityEngine;
using System;
using System.Collections.Generic;
using Pathfinding;
public class NPStats : EntityStats
{
    /*  ZDEDENE ATRIBUTY
     *
     * [SF] protected TMP_Text nameTag;
     * [SF] protected Rase rase;
     * [SF] protected Slider hpBar;
     * [SF] protected GameObject body;
     * [SF] protected NetworkObject netObject;
     * [SF] protected Transform attackPoint;
     * [SF] protected ColorChainReference color;
     * [SF] protected SpriteRenderer weaponR, weaponL;
     * [SF] protected NetworkAnimator animator;
     * [SF] protected Rigidbody2D rb;
     * [SF] protected Collider2D coll;
     * [SF] protected AITarget aiTeam = AITarget.Team_2;
     * [SF] protected EntityController controller;
     * [SF, SD("Nazov","Audios")] protected SerializedDictionary<string, AudioSource> audioSources;
     *
     * [HII] public NetworkVariable<float> speed = new(100);
     * [HII] public NetworkVariable<byte> level = new(1);
     * [HII] public NetworkVariable<bool> IsAlive = new(true);
     * protected   NetworkVariable<int> hp = new(100);
     * protected   NetworkVariable<int> maxHp = new(100);    
     * protected   NetworkVariable<byte> stepIndex = new(0);
     * protected   NetworkVariable<WeaponIndex> weapE = new(new(0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
     *
     * public float HP                     => (float)hp.Value/(float)maxHp.Value;    
     * public virtual Quaternion Rotation  => transform.rotation;
     * protected virtual Weapon[] Weapons  => rase.weapons;
     * public Weapon EquipedWeapon         => Weapons[weapE.Value.eIndex];
     * public NetworkObject NetObject      => netObject;
     * public Rigidbody2D RigidBody2D      => rb;
     * public Transform AttackPoint        => attackPoint;
     * public virtual Attack Attack        => GetAttackByWeaponIndex(weapE.Value);  
     * public AITarget TargetTeam          => aiTeam;
     * public Animator Animator            => animator.Animator;
     * public Vector2 View                 => controller.View;
     * public Color Color                  => color.Color;
     * public float ViewAngle              => Mathf.Atan2(View.x, View.y);
     * public bool AttackBoth              => Attack.bothHanded;
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
    public bool AboutToFire    => aToFire <= Time.time;
    public float NextAttackTime=> atTime;
    
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AddToCount(1);
        
        SetWeaponIndex(0,0);
    }
    /// <summary>
    /// <inheritdoc/>
    /// Spusta sa po vymazani objektu v ramci siete
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        AddToCount(-1);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OwnerSubsOnNetValChanged()
    {
        if (!IsServer) return;
        base.OwnerSubsOnNetValChanged();
        speed.OnValueChanged += (float old, float now) =>
        {
            aIPath.maxSpeed = now/100f;
        };
    }
    /// <summary>
    /// Spusti sa pri zmene zivotov, zmeni utok alebo zbran ak je dosiahnuty prah zmeny
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="now"></param>
    protected override void OnHpUpdate(int prev, int now)
    {
        base.OnHpUpdate(prev, now);
        for (int i = rase.swapons.Length-1; 0 <= i; i--)
            if (i < rase.swapons.Length && rase.swapons[i].ReachedHP(HP))
                SetWeaponIndex(rase.swapons[i].weaponIndex);
        OnHit.Invoke();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void Die()
    {
        base.Die();
    }
    /// <summary>
    /// Prida nepriatela do pocitadla
    /// </summary>
    /// <param name="b"></param>
    protected virtual void AddToCount(sbyte b)
    {
        // iba na servery
        if (!IsServer) return;    
        NPCount = (byte)(NPCount + b);
    }
    /// <summary>
    /// Zastavuje ziskavanie pozicie ciela na urcity cas pred vystrelenim
    /// <param name="proj"></param>
    /// </summary>
    public void SetAboutToFireTime(Projectile proj)
    {
        aToFire = proj.FireTime;
        aToFire *= 1 - RANGED_ATTACK_INACURRACY;
        aToFire += Time.time;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="damage"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override bool TakeDamage(Damage damage)
    {
        return base.TakeDamage(damage);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="died"><inheritdoc/></param>
    public override void KilledEnemy(EntityStats died)
    {
        
    }
#pragma warning disable IDE0051 // Remove unused private members
    /// <summary>
    /// Kresli grafiku v editore ak je zapnuty parameter "drawGizmo"
    /// </summary>
    void OnDrawGizmos()
    {
        if (drawGizmo)
            OnDrawGizmosSelected();
    }
    /// <summary>
    /// Kresli grafiku v edtiore ak je objekt oznaceny
    /// </summary>
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
    /// <summary>
    /// Ak sa dotke vody tak sa znici
    /// </summary>
    /// <param name="collision"></param>
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