using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
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
    public void ResetTargeting()
    {
        if (IsServer)
        {
            ClosestTarget = null;
            string tt = "";
            Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, coll.radius);

            foreach (Collider2D c in colls)
            {
                tt += c.name + "  ";
                OnTriggerEnter2D(c);
            }
            targetChange.Invoke(ClosestTarget);
            
            Debug.Log("Targeting Reset\nTargets: " + tt);
        }
    }
    public void DisableSensor()
    {
        coll.enabled = false;
        ClosestTarget = null;
        inRange.Clear();
        targetChange.Invoke(ClosestTarget);
    }
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
        if (ClosestTarget != null)
            ClosestTarget.GetComponent<EntityStats>().OnDeath -= ResetTargeting;

        ClosestTarget = target;

        if (ClosestTarget != null)
            target.GetComponent<EntityStats>().OnDeath += ResetTargeting;

        TargetInRange = inRange;
        targetChange.Invoke(ClosestTarget);
    }
    public void SetRange(float r)
    {
        coll.radius = r;
    }
}
[CustomEditor(typeof(NPSensor))]
public class MyButtonExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Referencia na cieľový objekt
        NPSensor example = (NPSensor)target;

        // Tlačidlo v inspektore
        if (GUILayout.Button("Reset Targeting"))
        {
            example.ResetTargeting();
        }
    }
}