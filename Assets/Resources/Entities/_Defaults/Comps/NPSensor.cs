using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NPSensor : NetworkBehaviour
{
    public bool TargetInRange {get; private set;}
    public Transform ClosestTarget {get; private set;}
    public AITarget me;
    [SerializeField] CircleCollider2D coll;
    List<Transform> inRange = new();
    //const string ENTITY_TAG = "Entity";
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer /*&& collision.transform.tag == ENTITY_TAG*/)
        {
            Debug.Log("Collision with sensor of " + transform.parent.name);
            inRange.Add(collision.transform);
            FindClosestTarget();
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsServer /*&& collision.transform.tag == ENTITY_TAG*/)
        {
            inRange.Remove(collision.transform);
            if (ClosestTarget == collision.transform)
                FindClosestTarget();
        }
    }
    void FindClosestTarget()
    {
        float d = float.PositiveInfinity;
        int i = 0, index = i;

        if      (inRange.Count == 0)
            SetTarget(null, false);
        else if (inRange.Count > 1) // preskakuje akje len 1 zapis
            inRange.ForEach(iR => 
            {
                Vector2 tP = iR.transform.position;
                Vector2 v = new(Mathf.Abs(transform.position.x - tP.x), Mathf.Abs(transform.position.y - tP.y));
                if (d > v.magnitude)
                {
                    d = v.magnitude;
                    index = i;
                }
                i++;
            });
        SetTarget(inRange[i]);
        
    }
    public void SetTarget(Transform target, bool inRange = true)
    {
        ClosestTarget = target;
        TargetInRange = inRange;
    }
    public void SetRange(float r)
    {
        coll.radius = r;
    }
}
