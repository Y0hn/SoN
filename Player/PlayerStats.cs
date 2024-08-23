using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : EntityStats
{
    /*  Inhereted Variables
     * 
     *  NetworkList<Rezistance> rezists = new();
     *  [SF] NetworkVariable<int> maxHp = new();
     *  NetworkVariable<int> hp = new();
     *  [SF] Slider hpBar;
     *
     */
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            hpBar.gameObject.SetActive(false);
            hpBar = GameManager.instance.GetPlayerHpBar();
            nameTag.text = GameManager.instance.GetPlayerName();
        }
    }
    protected override void Update()
    {
        base.Update();
    }
}
