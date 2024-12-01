using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Collections;

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
     *  [SF]    protected NetworkVariable<int> maxHp = new();
     *
     *  protected NetworkVariable<int> hp = new();
     *  protected NetworkVariable<Attack> attack = new ();
     *  protected NetworkVariable<FixedString128Bytes> weapRef = new();
     *  public NetworkVariable<bool> IsAlive = new(true);
     *  public NetworkVariable<float> speed = new();
     *  public NetworkVariable<byte> level = new(1);
     *
     *  public float HP { get { return (float)hp.Value/(float)maxHp.Value; } }
     *  public NetworkObject NetObject { get { return netObject; } }
     *  public Animator Animator { get { return animator.Animator; } }
     *  public Rigidbody2D RigidBody2D { get { return rb; } }
     *
     *  protected const float timeToDespawn = 0f;
     *  private bool clampedDMG = true;
     *  protected Defence defence;
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    [SerializeField] Behavior behavior = Behavior.Neutral;
    public Weapon.Class WC   { get; protected set; }
    public Defence.Class DC    { get; protected set; }
    public Behavior Behave  { get { return behavior; } protected set => behavior = value; }
    public Action OnHit;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkPostSpawn();
    }
    protected override void EntitySetUp()
    {
        base.EntitySetUp();
        CallculateWC();
        CallculateDC();
    }
    protected override void OnEquipmentUpdate(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        base.OnEquipmentUpdate(changeEvent);
        Equipment.Slot slot = (Equipment.Slot)changeEvent.Index;
        
        switch (slot)
            {
                case Equipment.Slot.Head:   // 0
                case Equipment.Slot.Torso:  // 1
                case Equipment.Slot.Legs:   // 2
                    DC = defence.CallculateDC();
                    break;
                case Equipment.Slot.WeaponL:    // 4
                case Equipment.Slot.WeaponR:    // 5
                    CallculateWC();
                    break;
            }
    }
    protected override void OnHpUpdate()
    {
        float newHp = HP;
        base.OnHpUpdate();
        OnHit.Invoke();
    }
    public void CallculateWC()
    {
        Weapon w = (Weapon)Item.GetItem(equipment[weapE.Value.eIndex].ToString());
        WC = w.CallculateWC();
    }
    public void CallculateDC()
    {
        DC = defence.CallculateDC();
    }
    public enum Behavior 
    {
        Scared,     // unika pred target
        Defesive,   // brani poziciu
        Neutral,    // nerobi nic (idle)
        Agressive,  // aktivne utoci na target
        Berserk,    // --||-- neberie ohlad na nic ine
    }
}