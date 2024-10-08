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
    [SerializeField] Vector2 pixelSize = new(1200, 500);
    List<ItemSlot> itemSlots = new();
    bool inv = false;
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
        if (instance == null) 
            instance = this;
        Sizing();
    }
    void Sizing()
    {
        if (size <= 0) return;

        /*
        RectTransform rs = parent.GetComponent<RectTransform>();
        Vector2 pixelSize = new (
            (rs.anchorMax.x - rs.anchorMin.x)*Screen.width  - (grid.padding.right + grid.padding.left),
            (rs.anchorMax.y - rs.anchorMin.y)*Screen.height - (grid.padding.top + grid.padding.bottom)
             600, 1200
        );
        */
        Vector2 spacing = grid.spacing;
        if (size <= 0) 
        {
            gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        
        if (parent.childCount != itemSlots.Count)
            FixDiscrepancy();
        if (itemSlots.Count == size)
            return;

        FixDiscrepancy();

        int rows = Mathf.CeilToInt((float)Math.Sqrt(size * pixelSize.x/pixelSize.y)),
            //cols = (byte)Mathf.Floor(cols-spacing.x/pixelSize.x*cols);
            cols = (int)Mathf.Ceil(((float)size/(float)rows));
        //Debug.Log($"Mathf.CeilToInt({size}/{rows}) = {cols}");
        //space /= rows;
        
        float cell = Mathf.Sqrt((float)(pixelSize.x*pixelSize.y) / (float)(cols*rows)) - spacing.x;

        //float cell = pixelSize.y/(float)rows - spacing.y;
        //Debug.Log($"Mathf.Sqrt({pixelSize.x} * {pixelSize.y} / ({cols}*{rows})) - {spacing.x} = {cell}");

        MakeItFit(pixelSize, new(rows, cols), ref cell, spacing.x);

        grid.cellSize = new(cell,cell);
        //grid.spacing = new(spacing,spacing);

        //Debug.Log($"Pixel size {cell} => [r:{rows},c:{cols}]");
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
    bool Fits(Vector2 size, Vector2 grid, float cell)
    {
        return !(size.x < grid.x * cell || grid.y * cell > size.y);
    }
    void MakeItFit(Vector2 size, Vector2 grid, ref float cell, float spase)
    {
        for (int i=1; i < size.x && !Fits(size, grid, cell + spase); i++)
        {
            cell -= i;
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
    public void SetSize(ushort newSize)
    {
        size = newSize;
        Sizing();
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