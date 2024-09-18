using Unity.Netcode;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item"), Serializable] public class Item : ScriptableObject, INetworkSerializable
{
    public new string name;
    public string description;
    public string iconRef = "Items/textures";
    public Color color = Color.white;
    public Color rarity = Color.white;
    [ServerRpc] public virtual void DropItemServerRpc()
    {
        // Server do this
        // Swawn of ItemDrop
        GameObject drop = Instantiate(Resources.Load<GameObject>("Items"));
        drop.GetComponent<ItemDrop>().Item = this;
        
        Debug.Log("Item Droped");
    }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref rarity);
        serializer.SerializeValue(ref iconRef);
        serializer.SerializeValue(ref description);
    }
}