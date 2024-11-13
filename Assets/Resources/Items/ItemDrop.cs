using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] NetworkObject netObj;
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] public Item item;
    [SerializeField] bool tester = false;
    public Item Item
    {
        get { return item; }
        set 
        {  
            item = value;
            if (item != null)
            {
                texture.sprite = Resources.Load<Sprite>(item.iconRef);
                texture.color = item.color;
            }
            else 
                netObj.Despawn();
        }
    }
    public override void OnNetworkSpawn()
    {
        // inak by sa mohol znicil prilis skoro
        if (item != null)
            Item = item;    // vyzera to zaujimavo ale nastavi to iconu pre item ak nie je null
    }
#pragma warning disable IDE0051 // Remove unused private members
    void OnDrawGizmos()
    {
        if (tester)
        {
            OnNetworkSpawn();
            tester = !tester;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) 
            return;
        // sem sa dostane len Server aby sa nestalo ze viaceri Clienti detekuju to iste

        if (collision.transform.TryGetComponent(out PlayerStats pl))
        {
            ulong id = pl.NetObject.OwnerClientId;
            // ziska id clienta
            pl.PickUpItemRpc(Item.GetReferency, RpcTarget.Single(id, RpcTargetUse.Temp));
            // hrac skusi zobrat ItemDrop

            // Pridat overenie ci hrac zobral item
            Item = null;
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    [Rpc(SendTo.Server)] public void PickedUpRpc()
    {
        Debug.Log($"Item {name} picked up");
        Item = null;
        // nasledne sa objekt znici
    }
}
