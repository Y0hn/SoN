using UnityEngine;

public class BosController : NPController
{
    /* ZDEDENE ATRIBUTY
     *  [SF] protected EntityStats stats;
     *  [SF] AIDestinationSetter destinationSetter;
     *  [SF] AIPath path;
     *  [SF] NPSensor sensor;
     *  protected Vector2 moveDir;
     *  protected bool attacking;
     *  protected const float MIN_MOVE_TOL = 0.1f;
     *  protected NextAction nextAction;
     *  protected float nextDecisionTimer = 0f;
     *  protected List<Transform> patrol = new();
     *  protected bool selfTarget;
     *
     *  public bool ForceDecision       { get; protected set; }
     *  protected Vector3 TargetPosition => sensor.ClosestTarget.position;    
     *  *  *  *  *  *  *  *  *  *  *  *  *  *  */
    protected new BosStats Stats => (BosStats)base.Stats;

    protected override void Update()
    {
        base.Update();

        DecideNextMove();
    }

    protected virtual void DecideNextMove()
    {
        float nextChange = 1f;
        bool inRange = false, gotInRange = false ;

        switch (Stats.WC)
        {
            default:
                break;
        }
        switch (Stats.DC)
        {
            default:
                break;
        }
        switch (Stats.Behave)
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
}