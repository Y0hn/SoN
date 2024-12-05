using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;

public class NPSensor : NetworkBehaviour
{
    [SerializeField] CircleCollider2D coll;
    List<Transform> inRange = new();
    public Transform ClosestTarget  {get; private set;}
    public bool TargetInRange       {get; private set;}
    public AITarget me              { get; set; }
    public Action<Transform> targetChange;

#pragma warning disable IDE0051 // Remove unused private members
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.TryGetComponent(out EntityStats et) && et.TargetTeam != me)
        {
            //Debug.Log("Collision with sensor of " + transform.parent.name);
            inRange.Add(other.transform);
            FindClosestTarget();
            targetChange.Invoke(ClosestTarget);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsServer)
        {
            inRange.Remove(other.transform);
            if (ClosestTarget != null && ClosestTarget.Equals(other.transform))
            {
                FindClosestTarget();
                targetChange.Invoke(ClosestTarget);
            }
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    void FindClosestTarget()
    {
        float d = float.PositiveInfinity;
        int i = 0, index = i;

        if      (inRange.Count == 0)
        {
            SetTarget(null, false);
        }
        else if (inRange.Count >= 1) // preskakuje akje len 1 zapis
        {
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

            //try {
                SetTarget(inRange[index]);/*
            } catch {
                Debug.LogWarning($"Exeption e i={i} inRange.Count={inRange.Count}");
            }*/
        }
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
