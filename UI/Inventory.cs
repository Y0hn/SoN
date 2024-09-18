using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using System;
using TMPro;
public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField] int size = 1;
    [SerializeField] TMP_Text btn;
    [SerializeField] Button button;
    [SerializeField] Transform parent;
    [SerializeField] Animator animator;
    [SerializeField] GameObject slotPreFab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] InputActionReference input;
    // Inventory
    [SerializeField] List<Item> inventory;
    List<ItemSlot> itemSlots;
    bool inv = false;
    const int pixelSize = 600;
    const int spacing = 10;
    void Start()
    {
        button.onClick.AddListener(OC_Inventory);
        input.action.started += OC_Inventory;
    }
    void OnDrawGizmos()
    {
        Awake();
    }
    void Awake()
    {
        if (instance == null) instance = this;
        if (size <= 0) return;
        
        int kids = parent.childCount;

        if      (kids > size)
            for (int i = kids - 1; kids - size < i; i--)
            {
#if UNITY_EDITOR
                DestroyImmediate(parent.GetChild(i).gameObject);
#else
                Destroy(parent.GetChild(i).gameObject);
#endif
                Item last = inventory[inventory.Count - 1];
                if (last != null)
                {
                    
                }
                else
                    inventory.RemoveAt(inventory.Count - 1);
            }
        else if (kids < size)
            for (int i = 0; i < size - kids; i++)
            {
                itemSlots.Add(Instantiate(slotPreFab, parent).GetComponent<ItemSlot>());
                inventory.Add(null);
            }
        else
            return;

        int space = pixelSize%size + spacing;
        int rows = (int)Math.Ceiling(Math.Sqrt(size));

        space /= rows;
        
        float f = pixelSize / rows - space;

        grid.cellSize = new(f,f);
        grid.spacing = new(space,space);
    }
    void OC_Inventory(InputAction.CallbackContext context) { OC_Inventory(); }
    void OC_Inventory() 
    {
        if (!GameManager.instance.playerLives) return;
        inv = !inv;
        animator.SetBool("open", inv);
        if (inv) btn.text = "<";
        else btn.text = ">";
    }
    public bool AddItem(Item item)
    {
        if (inventory.Count >= size || item == null)
            return false;
        
        inventory.Add(item);
        return true;
    }
}

#region Items
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item"), Serializable] public class Item : ScriptableObject, INetworkSerializable
{
    public new string name;
    public string description;
    public string iconRef;
    public Color color;
    public Color rarity;
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
        serializer.SerializeValue(ref iconRef);
        serializer.SerializeValue(ref description);
    }
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Equipment"), Serializable] public class Equipment : Item
{
    public Rezistance rezistance;
    public Slot slot;
    public enum Slot
    {
        Head, Torso, Hands, Legs,
        Body
    }
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref rezistance);
        serializer.SerializeValue(ref slot);
    }
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] public class Weapon : Item
{
    public Attack attack;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref attack);
    }
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Money"), Serializable] public class Coin : Item
{
    public int amount;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref amount);
    }
}
#endregion