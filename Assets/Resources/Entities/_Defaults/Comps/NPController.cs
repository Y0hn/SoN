using UnityEngine;
using System;

public class NPController : EntityController
{
    /* Inhereted variables
     *
     *
     */
    
    protected NextAction nextAction;
    protected float nextDecisionTimer = 0f;
    public bool ForceDecision { get; protected set; }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ((NPStats)stats).OnHit += delegate { ForceDecision = true; };
    }
    protected override void Update()
    {
        base.Update();
        if (IsServer && (nextDecisionTimer < Time.time || ForceDecision))
            DecideNextMove();
    }
    protected override void AnimateMovement()
    {
        base.AnimateMovement();
    }
    protected virtual void DecideNextMove()
    {
        float nextChange = 1f;
        float hp = stats.HP;

        switch (((NPStats)stats).WC)
        {
            default:
                break;
        }
        switch (((NPStats)stats).DC)
        {
            default:
                break;
        }
        switch (((NPStats)stats).Behave)
        {
            case NPStats.Behavior.Scared:       nextAction = NextAction.RunFromTarget; break;
            case NPStats.Behavior.Berserk:      nextAction = NextAction.RunToTarget; break;
            case NPStats.Behavior.Neutral:      nextAction = NextAction.StayOnPlace; break;
            case NPStats.Behavior.Agressive:
            case NPStats.Behavior.Defesive:
                break;
        }
        if (ForceDecision)
            ForceDecision = false;
        nextDecisionTimer = Time.time + nextChange;
    }
    protected enum NextAction { GoToTarget, RunToTarget, AttackTarget, RunFromTarget, StayOnPlace }
}
