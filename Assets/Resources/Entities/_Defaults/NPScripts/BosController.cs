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
    }
    public override void SetTarget(Transform t)
    {
        base.SetTarget(t);
        canvas.SetActive(!selfTarget);
        // nefunguje
    }
}