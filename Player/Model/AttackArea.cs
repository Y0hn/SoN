using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class AttackArea : NetworkBehaviour
{
    [SerializeField] PolygonCollider2D coll;
    private Damage damage;
    public void Edit(Vector2 right, Vector2 center, Vector2 left)
    {
        coll.points[3] = right;
        coll.points[4] = center;
        coll.points[5] = left;
    }
    public void Edit(float range)
    {
        coll.points[3] = new (-range, range);
        coll.points[4] = new (0, range+0.4f);
        coll.points[5] = new (range,  range);
    }
    public void SetDamage(Damage damage)
    {
        this.damage = damage;
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out EntityStats stats))
        {
            stats.TakeDamage(damage);
        }
    }
}
