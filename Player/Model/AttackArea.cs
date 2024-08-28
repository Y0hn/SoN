using UnityEngine;
using Unity.Netcode;

public class AttackField : NetworkBehaviour
{
    [SerializeField] PolygonCollider2D coll;
    [SerializeField] EntityStats myStats;
    private Damage damage = new (Damage.Type.bludgeoning, 0);
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
        if (IsServer)
        {
            this.damage = damage;
            Debug.Log(name + " AT field Damage updated");
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D");
        if (collider.TryGetComponent(out EntityStats stats) && IsOwner)
        {
            ulong ownID = stats.GetComponent<NetworkObject>().OwnerClientId;
            myStats.TakeDamageServerRpc(damage, ownID);
            Debug.Log("Attack area collided with " + stats.name);
        }
        
        if (IsOwner)
        {
            Debug.Log("Called by owner");
        }
    }
    private void OnEnable()
    {
        Debug.Log("Attack Zone Enabled");
    }
}
