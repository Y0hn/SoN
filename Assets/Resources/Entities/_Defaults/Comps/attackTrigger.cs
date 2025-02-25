using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Utoci cez animator
/// </summary>
public class AttackTrigger : NetworkBehaviour
{
    [SerializeField] EntityStats stats;
    [SerializeField] EntityController controller;
    [SerializeField] AudioSource audioSource;
    private bool atoFire = false;
    public void Trigger()
    {        
        stats.AtackSound.Play(ref audioSource);

        if (IsServer)
            stats.AttackRpc();
    }
    public void AboutToFire()
    {        
        if (IsServer && stats is NPStats npS)
        {
            atoFire = true;
            npS.AboutToFire= atoFire;
        }
    }
    public void TrySwitchHand()
    {
        if (IsOwner)
        {
            controller.SwitchHand();
        }
    }
}
