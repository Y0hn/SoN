using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] NetworkObject netObj;
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] private Item item;
    [SerializeField] bool tester = false;
    private static List<World.ItemOnFoor> itemsOnFoor = new();     // iba na Servery
    private World.ItemOnFoor itFoor;
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
                Register();
            }
            else
                netObj.Despawn();
        }
    }
    public override void OnNetworkSpawn()
    {
        if (item == null) return;

        Item = item;
        
        Register();
    }
    private void Register()
    {
        if (!IsServer || itFoor != null)  return;
        itFoor = new (transform.position, item.GetReferency);
        itemsOnFoor.Add(itFoor);
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer && itFoor != null)
            itemsOnFoor.Remove(itFoor);       
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
        if (collision.transform.TryGetComponent(out PlayerStats pl))
        {
            pl.PickedUpRpc(Item.GetReferency);
            PickedUpRpc();
            // Pridat overenie ci hrac zobral item
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
    [Rpc(SendTo.SpecifiedInParams)] public void SetItemRpc(string pathReferncy, RpcParams rpcParams)
    {
        Item = Resources.Load<Item>(pathReferncy);
    }
}
