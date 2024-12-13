using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] NetworkObject networkObject;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D coll;
    public EntityController etc;
    public Damage damage;
    public float delay = 0f;
    public float range = 0f;
    public float graficDelay = 0f;
    private const float FORCE = 1000;
    Vector3 force;
    Vector2 startPos = Vector2.positiveInfinity;
    private float[] timer = { -1, -1};
    public override void OnNetworkSpawn()
    {
        //Debug.Log("Projectile Lauched with attack: " + damage.amount);
        coll.enabled = false;
        sprite.enabled = false;
        
        timer[0] = Time.time + delay;
        timer[1] = Time.time + graficDelay;
    }
#pragma warning disable IDE0051 // Remove unused private members
    void Update()
    {
        if (timer[0] > 0 && Time.time >= timer[0])  // vystrelenie projektilu
        {
            transform.SetParent(null);
            coll.enabled = true;
            force = transform.up;
            force *= FORCE;
            startPos = transform.position;
            rb.AddForce(force);
            timer[0] = 0;
            etc = null;
        }
        if (timer[1] > 0 && Time.time >= timer[1])  // vykreslenie textury
        {
            sprite.enabled = true;
            timer[1] = 0;
        }
        if (coll.enabled && Vector2.Distance(startPos, transform.position) >= range)
        {
            networkObject.Despawn();
        }
    }
    void FixedUpdate()
    {

        if (etc != null && etc.ViewAngle != transform.rotation.z)
            RotateAroundPoint();
    }
    void RotateAroundPoint()
    {
        /*
        Vector3 rot = etc.transform.position;
        Vector3 direction = new Vector3(etc.View.x,etc.View.y,0) - rot;

        float angle = etc.ViewAngle;
        Vector3 axis = new (0,0,1);
        direction = Quaternion.AngleAxis(angle, axis) * direction;

        // Update object position
        transform.position = rot + direction;

        Debug.Log($"Direction ({direction}) Angle ({angle}) View ({etc.View.x},{etc.View.y})");*/
        transform.position = etc.ets.ProjectilePoint;
        transform.rotation = etc.ets.ProjPointTrans.rotation;
        transform.Rotate(new Vector3(0,0,1), etc.ViewAngle, Space.Self);
    }
    /*void SetRotationAroundPoint()
    {
        Vector2 center = etc.transform.position;
        float angleDegrees = etc.ViewAngle;
        // Convert angle to radians
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Get the current position
        Vector2 position = transform.position;

        // Calculate the offset from the center
        Vector2 offset = position - center;

        // Rotate the offset
        float rotatedX = offset.x * Mathf.Cos(angleRadians) - offset.y * Mathf.Sin(angleRadians);
        float rotatedY = offset.x * Mathf.Sin(angleRadians) + offset.y * Mathf.Cos(angleRadians);
        Vector2 rotatedOffset = new Vector2(rotatedX, rotatedY);

        // Set the new position
        transform.position = center + rotatedOffset;

        // Optionally rotate the object itself
        transform.rotation = Quaternion.Euler(0, 0, angleDegrees);
    }*/
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.TryGetComponent(out EntityStats et) /*&& et.TargetTeam != me*/)
        {
            et.TakeDamage(damage);
            networkObject.Despawn();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}
