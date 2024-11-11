using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] NetworkObject netObj;
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] public Item item;
    public Item Item
    {
        get { return item; }
        set 
        {  
            item = value;
            if (item != null)
            {
                texture.sprite = Resources.Load<Sprite>(item.iconRef);
                /*List<Sprite> sprites = Resources.LoadAll<Sprite>(defaultTexturePath).ToList();
                texture.sprite = sprites.Find(sprite => sprite.name == item.iconRef);*/
                
                // if class Sprite had implemented 'IEnumerable<Customer>'
                /* 
                    Sprite[] sprites = Resources.LoadAll<Sprite>(defaultTexturePath);
                    sprites.Where(s => s.name == item.iconRef);

                    There would be no need for .ToList()
                */
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
    [Rpc(SendTo.Server)] public void PickedUpRpc()
    {
        Debug.Log($"Item {name} picked up");
        Item = null;
        // nasledne sa objekt znici
    }
}
