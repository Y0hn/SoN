using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Utoci cez animator
/// </summary>
public class AttackTrigger : NetworkBehaviour
{
    [SerializeField] EntityStats stats;
    [SerializeField] AudioSource audioSource;
    
    public void Trigger()
    {        
        stats.AtackSound.Play(ref audioSource);

        if (IsServer)
            stats.AttackRpc();
    }
}
