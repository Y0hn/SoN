using UnityEngine;

public class BosStats : NPStats
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
     *  [SF] protected Behavior behavior = Behavior.Neutral;
     *  [SF] protected NPSensor sensor;
     *  [SF] protected Equipment[] setUpEquipment;
     *  [SF] protected bool drawGizmo = false;
     *
     *  protected Defence defence;
     *  protected const float timeToDespawn = 0f;
     *  private bool clampedDMG = true;
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
     *
     *  protected const float RANGED_ATTACK_INACURRACY = 0.4f; // symbolizuje percento casu utoku kedy nedostava polohu ciela [+inacuracy => -presnost]
     *  protected const float ATTACK_DISTANCE_PERCENTAGE = 0.3f;
     *  public override Quaternion Rotation     { get => body.transform.rotation; }
     *  public float AttackDistance             
     *  public override Attack Attack
     *  public Defence.Class DC     { get; protected set; }
     *  public Weapon.Class WC      { get; protected set; }
     *  public Behavior Behave      { get { return behavior; } protected set => behavior = value; }
     *  public bool AboutToFire    => aToFire <= Time.time;
     *  public float NextAttackTime=> atTime;
     *  public Action OnHit;
     *  protected float aToFire;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  */

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            if (FileManager.World.boss == null)
                FileManager.World.boss = new (this);
            else                
                LoadSavedData(FileManager.World.boss);
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OwnerSubsOnNetValChanged()
    {
        if (!IsServer) return;
        base.OwnerSubsOnNetValChanged();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="save">ulozene DATA hlavneho nepriatela</param>
    protected override void LoadSavedData(World.EntitySave save)
    {
        var b = (World.BossSave)save;
        if (!b.isAlive)
            netObject.Despawn();
        else
            base.LoadSavedData(save);
    }
}