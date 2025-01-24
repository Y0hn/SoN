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
    [SerializeField] protected GameObject canvas;
    protected new BosStats Stats => (BosStats)base.Stats;

    protected override void Update()
    {
        base.Update();

        DecideNextMove();
    }
    public override void SetTarget(Transform t)
    {
        base.SetTarget(t);
        canvas.SetActive(!selfTarget);
        // nefunguje
    }
    void DecideByTreshhold(float tHP, bool inRange, bool gotRange, out float tDecay)
    {
        /*
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
        */
    }
}