using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] NetworkObject netObj;
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] public Item item;
    [SerializeField] bool tester = false;
    private static Dictionary<int, ItemOnFoor> itemsOnFoor = new();
    private int floorID;
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
            {
                if (IsServer)
                    itemsOnFoor.Remove(floorID);
                netObj.Despawn();
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        Item = item;

        if (IsServer)
        {
            floorID = itemsOnFoor.Count;
            itemsOnFoor.Add(floorID, new (transform.position, item.GetReferency));
        }
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
    [Rpc(SendTo.Everyone)] public void SetItemRpc(string pathReferncy)
    {
        Item = Item.GetItem(pathReferncy);
    }

    public class ItemOnFoor
    {
        public Vector2 pos;
        public string itemRef;
        public ItemOnFoor(Vector2 _pos, string _itemRef)
        {
            pos = _pos;
            itemRef = _itemRef;
        }
    }
}
