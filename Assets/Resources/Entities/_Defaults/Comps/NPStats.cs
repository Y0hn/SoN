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
    [SerializeField] protected Behavior behavior = Behavior.Neutral;
    [SerializeField] protected NPSensor sensor;
    [SerializeField] protected Equipment[] setUpEquipment;
    [SerializeField] protected bool drawGizmo = false;
    protected const float ATTACK_DISTANCE_PERCENTAGE = 0.3f;
    public float TargetDistance { get { return Attack.range*2 /*- ATTACK_DISTANCE_PERCENTAGE*Attack.range*/; } }
    protected Attack Attack     
    { 
        get 
        { 
            if (attack.Value.IsSet) 
                return attack.Value; 
            else
                return rase.attack;
        } 
    }
    public Defence.Class DC     { get; protected set; }
    public Weapon.Class WC      { get; protected set; }
    public Behavior Behave      { get { return behavior; } protected set => behavior = value; }
    public Action OnHit;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        EquipmentSetUp();
    }
    protected override void EntitySetUp()
    {
        base.EntitySetUp();
        if (IsServer)
        {
            sensor.me = aiTeam;
            sensor.SetRange(rase.view);
        }
    }
    protected virtual void EquipmentSetUp()
    {
        foreach(Equipment e in setUpEquipment) Equip(e.GetReferency);
        CallculateWC();
        CallculateDC();
    }
    protected void Equip(string equip)
    {
        Equipment e = Equipment.GetItem(equip);
        equipment[(int)e.slot] = e.GetReferency;
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
        base.OnHpUpdate();
        OnHit.Invoke();
    }
    public void CallculateWC()
    {
        if (weapE.Value.eIndex >= 0)
        {
            Weapon w = (Weapon)Item.GetItem(equipment[weapE.Value.eIndex].ToString());
            WC = w.CallculateWC();
        }
        else
            WC = Weapon.Class.Medium;
    }
    public void CallculateDC()
    {
        DC = defence.CallculateDC();
    }
    public override void PickedUpRpc(string reference)
    {
        
    }
#pragma warning disable IDE0051 // Remove unused private members
    void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, rase.view);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, TargetDistance);

            Gizmos.color = Color.red;
            Vector3 v = new(transform.position.x, transform.position.y + rase.attack.range, 0);
            Gizmos.DrawWireSphere(v, rase.attack.range);
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    public enum Behavior 
    {
        Scared,     // unika pred target
        Defesive,   // brani poziciu
        Neutral,    // nerobi nic (idle)
        Agressive,  // aktivne utoci na target
        Berserk,    // --||-- neberie ohlad na nic ine
    }
}