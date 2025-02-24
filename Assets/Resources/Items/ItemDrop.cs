using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] NetworkObject netObj;
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] private Item item;
    [SerializeField] bool tester = false;
    //private static List<World.ItemOnFoor> itemsOnFoor = new();     // iba na Servery
    //private World.ItemOnFoor itFoor;
    /// <summary>
    /// Atribut nastavujuci spadnuty predmet <br />
    /// ak je nastavenie "NULL" objekt sa znici
    /// </summary>
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
                //Register();
            }
            else
                netObj.Despawn();
        }
    }
    /// <summary>
    /// Vykona sa po vzniknuti objektu napriec sietou
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (item == null) return;
        name = name.Split('(')[0];
        Item = item;
        //Register();
    }
    /// <summary>
    /// Registruje predmet na zemi
    /// </summary>
    /*private void Register()
    {
        if (!IsServer || itFoor != null)  return;
        itFoor = new (transform.position, item.GetReferency);
        //itemsOnFoor.Add(itFoor);
        name = name.Split('-')[0] + "-" + item.name;
    }*/
    /// <summary>
    /// Vykona sa pred zanikom objektu v sieti
    /// </summary>
    public override void OnNetworkDespawn()
    {/*
        if (IsServer && itFoor != null)
            itemsOnFoor.Remove(itFoor);       */
    }
#pragma warning disable IDE0051 // Remove unused private members
    /// <summary>
    /// Kresli a obnovuje parametre ak je zanuta premennta "tester" 
    /// </summary>
    void OnDrawGizmos()
    {
        if (tester)
        {
            OnNetworkSpawn();
            tester = !tester;
        }
    }
    /// <summary>
    /// Pri kolizii sa pokusi pridat sa do inventara hraca 
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out PlayerStats pl))
        {
            if (pl.PickedUp(Item.GetReferency))
                PickedUpRpc();
            // Pridat overenie ci hrac zobral item
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
    /// <summary>
    /// Po zdvihnuti znici predmet
    /// </summary>
    [Rpc(SendTo.Server)] public void PickedUpRpc()
    {
        //Debug.Log($"Item {name} picked up");
        Item = null;
        // nasledne sa objekt znici
    }
    /// <summary>
    /// Nastavi predmet podla cesty k nemu
    /// </summary>
    /// <param name="pathReferncy"></param>
    [Rpc(SendTo.Everyone)] public void SetItemRpc(string pathReferncy)
    {
        Item = Item.GetItem(pathReferncy);
    }
    /// <summary>
    /// Nastavi predmet rpe konkretneho klienta
    /// </summary>
    /// <param name="pathReferncy"></param>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.SpecifiedInParams)] public void SetItemRpc(string pathReferncy, RpcParams rpcParams)
    {
        Item = Resources.Load<Item>(pathReferncy);
    }
}
