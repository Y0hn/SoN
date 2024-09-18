using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField] ushort size = 1;
    [SerializeField] TMP_Text btn;
    [SerializeField] Button button;
    [SerializeField] Transform parent;
    [SerializeField] Animator animator;
    [SerializeField] GameObject slotPreFab;
    [SerializeField] GameObject dropPreFab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] InputActionReference input;
    // Inventory
    //[SerializeField] List<Item> inventory;
    List<ItemSlot> itemSlots = new();
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
        if (size <= 0) 
        {
            gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        if (parent.childCount != itemSlots.Count) 
        { 
            Debug.LogWarning($"Inventory Count discrepancy {parent.childCount} Kids for {itemSlots.Count} ItemSlots");
            FixDiscrepancy();
        }

        if (itemSlots.Count == size) return;

        FixDiscrepancy();

        int space = pixelSize%size + spacing;
        int rows = (int)Math.Ceiling(Math.Sqrt(size));

        space /= rows;
        
        float f = pixelSize / rows - space;

        grid.cellSize = new(f,f);
        grid.spacing = new(space,space);
    }
    void FixDiscrepancy()
    {
        int kids = parent.childCount,
            itCo = itemSlots.Count;

        // viac Realnych objektov ako Virtualnych slotov
        if      (kids > itCo)
        {
            for (int i = kids -1; i >= itCo; i--)
                DestroySlot(i);
        }
        // viac Virtualnych slotov ako Realnych objektov
        else if (kids < itCo)
        {
            for (int i = itCo -1; i >= kids; i--)
                itemSlots.RemoveAt(i);
        }
        // Nastavit podla "size"
        else
        {
            // Treba zmensit inventar (pripadne dropnut itemy)
            if      (kids > size)
            {
                for (int i = kids - 1; size <= i; i--)
                {
                    if (itemSlots[i].empty)
                    {
                        itemSlots.RemoveAt(i);
                    }
                    else
                    {
                        itemSlots[i].Item.DropItemServerRpc();
                        itemSlots.RemoveAt(i);
                        // DROP item
                    }
                    DestroySlot(i);
                }
            }
            // Treba zvacsit iventar 
            else if (kids < size)
                for (int i = 0; i < size - kids; i++)
                {
                    ItemSlot iS = Instantiate(slotPreFab, parent).GetComponent<ItemSlot>();
                    itemSlots.Add(iS);
                }
        }
    }
    void DestroySlot(int index)
    {
#if UNITY_EDITOR
        DestroyImmediate(parent.GetChild(index).gameObject);
#else
        Destroy(parent.GetChild(index).gameObject);
#endif
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
        if (!itemSlots.Exists(it => it.empty) || item == null)
        {
            Debug.Log("Cannot pickup item " + item.name);
            return false;
        }
    
        itemSlots.Find(it => it.empty).SetItem(item);
        
        Debug.Log(item.name + " added to invetory");
        return true;
    }
    public void DropItem(Item item = null)
    {
        int i = itemSlots.Count - 1;
        if (item != null)
            i = itemSlots.FindIndex(it => it.Item == item);

        item = itemSlots[i].Item;
        Instantiate(dropPreFab).GetComponent<ItemDrop>().Item = item;
        itemSlots[i].SetItem();
    }
}