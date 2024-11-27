using UnityEngine;
using System;

public class NPController : EntityController
{
    //[SerializeField] Behavior behavior = Behavior.Neutral;

    public override void OnNetworkSpawn()
    {
        
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
