using UnityEngine;
using System;

public class NPController : EntityController
{
    [SerializeField] Behavior behavior = Behavior.Neutral;
    protected WeaponClass wc;
    protected ArmorClass ac;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    protected override void Update()
    {
        base.Update();
        if (IsServer)
            DecideNextMove();
    }
    protected override void AnimateMovement()
    {
        base.AnimateMovement();
    }
    protected virtual void DecideNextMove()
    {
        float hp = stats.HP;

        // ac / wc
        // behavior
        
        // HERE IS DECIDING FACTOR ACORDING TO PARAMETERS ABOVE
    }
    protected void CallculateAC()
    {
        
    }
    protected void CallculateWC()
    {

    }
    public enum Behavior 
    {
        Scared,     // unika pred target
        Defesive,   // brani poziciu
        Neutral,    // nerobi nic (idle)
        Agressive,  // aktivne utoci na target
        Berserk,    // --||-- neberie ohlad na nic ine
    }
    public enum ArmorClass  { None, Small, Medium, Heavy, Dedicated }
    public enum WeaponClass { Light, Medium, Heavy, Ranged }
}
