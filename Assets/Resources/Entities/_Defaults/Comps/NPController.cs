using UnityEngine;
using System;
using System.Collections.Generic;

public class NPController : EntityController
{
    /* Inhereted variables
     *
     *
     */
    
    protected NextAction nextAction;
    protected float nextDecisionTimer = 0f;
    protected List<Transform> patrol = new();
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
        bool inRange = false, gotInRange = false ;

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
            case NPStats.Behavior.Scared:       DecideByTreshhold(1, inRange, gotInRange, out nextChange);      break;
            case NPStats.Behavior.Berserk:      DecideByTreshhold(0, inRange, gotInRange, out nextChange);      break;
            case NPStats.Behavior.Neutral:      DecideByTreshhold(0.5f, inRange, gotInRange, out nextChange);   break;
            case NPStats.Behavior.Agressive:    DecideByTreshhold(0.2f, inRange, gotInRange, out nextChange);   break;
            case NPStats.Behavior.Defesive:     DecideByTreshhold(0.7f, inRange, gotInRange, out nextChange);   break;
        }
        if (ForceDecision)
            ForceDecision = false;
        nextDecisionTimer = Time.time + nextChange;
    }
    protected virtual void DoNextMove()
    {
        if (nextAction != NextAction.None)
        {
            nextAction = NextAction.None; 
        }
    }
    void DecideByTreshhold(float tHP, bool inRange, bool gotRange, out float tDecay)
    {
        float hp = stats.HP;
        if      (hp < tHP && inRange)
        {
            nextAction = NextAction.RunFromTarget;
            tDecay = 3;
        }
        else if (gotRange && (!inRange || inRange && tHP < hp))
        {
            nextAction = NextAction.AttackTarget;
            tDecay = 0.5f;
        }
        else if (!gotRange && tHP < hp)
        {
            nextAction = NextAction.RunToTarget;
            tDecay = 0.2f;
        }
        else if (patrol.Count > 0)
        {
            nextAction = NextAction.GoToTarget;
            tDecay = 1;
        }
        else
        {
            nextAction = NextAction.StayOnPlace;
            tDecay = 3;
        }
    }
    protected enum NextAction { GoToTarget, RunToTarget, AttackTarget, RunFromTarget, StayOnPlace, None }
}
