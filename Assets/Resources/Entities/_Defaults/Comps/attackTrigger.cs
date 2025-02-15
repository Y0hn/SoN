using Unity.Netcode;
using UnityEngine;

public class AttackTrigger : NetworkBehaviour
{
    [SerializeField] EntityStats stats;
    public void Trigger()
    {
        if (IsServer)
            stats.AttackRpc();
    }
}
