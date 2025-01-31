using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.Netcode;
/// <summary>
/// Sluzi na zachtenie hracov/nepriatelov v dosahu
/// </summary>
public class NPSensor : NetworkBehaviour
{
    [SerializeField] Collider2D coll;
    protected List<Transform> inRange = new();
    public Transform ClosestTarget  {get; private set;}
    public bool TargetInRange       {get; private set;}
    public EntityStats.AITarget me              { get; set; }
    public Action<Transform> targetChange;

#pragma warning disable IDE0051 // Remove unused private members
    /// <summary>
    /// Do senzoru nieco voslo, ak je to v inom AI time ako ja 
    /// => prida sa to na zoznam cielov a prepocita sa najblizsi ciel
    /// </summary>
    /// <param name="other"></param>
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
    /// <summary>
    /// Zo senzoru nieco vyslo, 
    /// ak to bolo v zozname cielov -> odstrani sa to, 
    /// ak to bol aktualny ciel => prepocita sa najblizsi ciel
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsServer && inRange.Contains(other.transform))
        {
            inRange.Remove(other.transform);
            if (ClosestTarget != null && ClosestTarget.Equals(other.transform))
                FindClosestTarget();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    /// <summary>
    /// Vyamaze list znamych cielov a ziska od znova vsetko v dosahu
    /// </summary>
    public void ResetTargeting()
    {
        if (IsServer)
        {
            Collider2D[] colls = new Collider2D[0];
            ClosestTarget = null;
            inRange.Clear();
            string tt = "";

            if (coll is CircleCollider2D circle)
                colls = Physics2D.OverlapCircleAll(transform.position, circle.radius);
            else if (coll is BoxCollider2D box)
                colls = Physics2D.OverlapBoxAll(transform.position, box.size, 0);

            foreach (Collider2D c in colls)
            {
                tt += c.name + "  ";
                OnTriggerEnter2D(c);
            }
            targetChange.Invoke(ClosestTarget);
            
            //Debug.Log("Targeting Reset\nTargets: " + tt);
        }
    }
    /// <summary>
    /// Vypne senzor a resetuje premenne
    /// </summary>
    public void DisableSensor()
    {
        coll.enabled = false;
        ClosestTarget = null;
        inRange.Clear();
        targetChange.Invoke(ClosestTarget);
    }
    /// <summary>
    /// Najde najblizsi ciel k sebe z listu moznych cielov
    /// </summary>
    void FindClosestTarget()
    {
        float d = float.PositiveInfinity;
        int i = 0, index = i;

        if      (inRange.Count == 0)
        {
            SetTarget(null, false);
        }
        else
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

            SetTarget(inRange[index]);
        }
    }
    /// <summary>
    /// Nastavuje aktualny ciel senzoru (ten nablizsi)
    /// </summary>
    /// <param name="target"></param>
    /// <param name="inRange"></param>
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
    /// <summary>
    /// Nastavuje vzdialenost snimania od stredu
    /// </summary>
    /// <param name="r">polomer</param>
    public void SetRange(float r)
    {
        if (coll is CircleCollider2D circle)
            circle.radius = r;
        else if (coll is BoxCollider2D box)
            box.size = new (r*2, r*2);
    }
}
#if UNITY_EDITOR
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
#endif