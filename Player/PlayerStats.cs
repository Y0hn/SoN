using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : EntityStats
{
    /*  Inhereted Variables
     * 
     * [SF] protected TMP_Text nameTag;
     * protected NetworkList<Rezistance> rezists = new();
     * [SF] protected NetworkVariable<int> maxHp = new();
     * protected NetworkVariable<int> hp = new();
     * [SF] protected Slider hpBar;
     * [SF] public float speed;
     * protected const float timeToDespawn = 0f;
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
