using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// Uklada inventar hraca lokalne
/// </summary>
public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public ushort Size 
    { 
        get { return size; } 
        set { size = value; onSizeChange.Invoke(); } 
    }
    public bool open { get; private set; }
    public bool FreeSpace { get { return itemSlots.Find(iS => iS.empty == true) != null; } }
    public string[] GetReference(bool inv = true) 
    { 
        string[] references;
        if (inv)
        {
            references= new string[itemSlots.Count];
            for (int i = 0; i < references.Length; i++)
                references[i] = itemSlots[i].Item.GetReferency;
        }
        else
        {
            references= new string[equipSlots.Count];
            for (int i = 0; i < references.Length; i++)
                references[i] = equipSlots[(Equipment.Slot)i].Item.GetReferency;
        }
        return references;
    }

    [SerializeField] ushort size = 1;
    [SerializeField] TMP_Text btn;
    [SerializeField] Button button;
    [SerializeField] Transform parent;
    [SerializeField] Animator animator;
    [SerializeField] GameObject slotPreFab;
    [SerializeField] GameObject dropPreFab;
    [SerializeField] GridLayoutGroup inventoryGrid;
    [SerializeField] InputActionReference input;
    [SerializeField] Vector2 pixelSize = new(1200, 500);
    [SerializeField] bool onGizmos = true;
    [SerializeField] List<AttackSlotActive> acSlots;
    [SerializeField] PassiveAttackSlotScript[] atSlots;
    
    // INVENTORY
    List<ItemSlot> itemSlots = new();
    
    [SerializedDictionary("Slot", "SlotObject"), SerializeField]    
    SerializedDictionary<Equipment.Slot, EquipmentSlot> equipSlots = new();
    private event Action onSizeChange;
    private GameManager game;
    void Start()
    {
        button.onClick.AddListener(OC_Inventory);
        input.action.started += OC_Inventory;
        game = GameManager.instance;
        SetQuicks();
        open = false;

        onSizeChange += Sizing;
    }
    void OnDrawGizmos()
    {
        if (onGizmos)
            Awake();
    }
    void Awake()
    {
        if (instance == null) 
            instance = this;
        Sizing();
    }
    void SetQuicks()
    {
        foreach (AttackSlotActive acSlot in acSlots)
            acSlot.SetShow();
        foreach (var atS in atSlots)
            atS.UnsetAttacks();
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
        Vector2 spacing = inventoryGrid.spacing;
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

        inventoryGrid.cellSize = new(cell,cell);
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
                        //itemSlots[i].Item.DropItemServerRpc();
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
                    iS.name = iS.name.Split('(')[0];
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
        DestroyImmediate(parent.GetChild(index).gameObject);
    }
    void OC_Inventory(InputAction.CallbackContext context) { OC_Inventory(); }
    public void OC_Inventory() 
    {
        if (GameManager.instance.PlayerAble || open)
        {
            if (!GameManager.instance.playerLives) return;
            open = !open;
            animator.SetBool("open", open);
            if (open) btn.text = "<";
            else btn.text = ">";
        }
    }
    public void SetSize(ushort newSize)
    {
        size = newSize;
        Sizing();
    }
    public bool Add(string refItem)
    {
        Item item = Item.GetItem(refItem);
        bool add = FreeSpace;

        string a = item.name;
        if (add)
        {
            itemSlots.Find(it => it.empty).Item = item;
            a += " was added to inventory";
        }
        else
            a += " cannot be added to inventory";
    
        //Debug.Log(a);
        return add;
    }
    public void Remove(string refItem)
    {
        
        Item item = Item.GetItem(refItem);
        if (itemSlots.Count > 0)
        {
            int i = itemSlots.Count - 1;
            if (item != null)
                i = itemSlots.FindIndex(it => it.Item == item);

            item = itemSlots[i].Item;
            if (item != null)
            {
                Instantiate(dropPreFab).GetComponent<ItemDrop>().Item = item;
                itemSlots[i].Item = null;
            }
        }
    }

    public void Equip (Equipment eq)
    {
        if (equipSlots.Keys.Contains(eq.slot))
        {
            equipSlots[eq.slot].Item = eq;

            game.LocalPlayer.SetEquipmentRpc(eq.GetReferency, eq.slot);
            if (eq is Weapon w)
            {
                int hand = -1;
                switch (w.slot)
                {
                    case Equipment.Slot.WeaponL:    hand = 1; break;
                    case Equipment.Slot.WeaponR:    hand = 0; break;
                    case Equipment.Slot.WeaponBoth: 
                        hand = 0;
                        equipSlots[eq.slot].Item = eq;
                        equipSlots[eq.slot].SetTransparent(true);
                        break;
                }
                if (hand >= 0)
                    atSlots[hand].SetAttacks(w.attack);

                // Auto sett attack
                int free = acSlots.Count - acSlots.FindAll(acS => acS.show).Count; 
                //Debug.Log($"acSlots.Count = {acSlots.Count} & acSlots.FindAll(acS => acS.show).Count = {acSlots.FindAll(acS => acS.show).Count}");   

                // nastavi prny volny            
                for (int slot = 0; slot < free; slot++)
                    atSlots[hand].Click(slot);
                if (0 < free)
                    ReloadAttacks();
            }
        }
    }
    public void UnEquip (EquipmentSlot equip)
    {
        Equipment eq = (Equipment)equip.Item;
        if (FreeSpace)
        {
            equip.Item = null;
            Add(eq.GetReferency);

            game.LocalPlayer.SetEquipmentRpc("", equip.slot);
            if (eq is Weapon w)
            {
                int i = -1;
                switch (w.slot)
                {
                    case Equipment.Slot.WeaponL:    i = 1; break;
                    case Equipment.Slot.WeaponR:    i = 0; break;
                    case Equipment.Slot.WeaponBoth: 
                        i = 0;
                        equipSlots[eq.slot].Item = eq;
                        equipSlots[eq.slot].SetTransparent(false);
                        break;
                }
                if (i >= 0)
                {
                    atSlots[i].UnsetAttacks();
                    ReloadAttacks();
                }
            }
        }
    }
    
    public void Quick(byte b)
    {
        if (!acSlots[b].active)
            acSlots[b].Select();
    }

    public void SetActiveAttackType(sbyte id, bool active)
    {
        bool already = acSlots.Find(acS => acS.id == id) != null;
        int b = acSlots.FindAll(acS => acS.show).Count;
        //Debug.Log($"Called set attack id= {id}, active= {active}, already= {already}, b= {b}");

        // ziska slot ktory zavolal metodu
        AttackSlotPassive change = null;
        for (int i = 0; i < atSlots.Length && change == null; i++)
            change = atSlots[i].GetSlot(id);
        
        // vypne posledny utok poslednej zbrane
        if      (active && !already && b == acSlots.Count)
        {
            for (int i = atSlots.Length-1; 0 < i; i--)
            {
                int index = atSlots[i].ShutLastActive();
                if (index >= 0)
                    acSlots.Find(acS => acS.id == id).SetShow();
            }
        }

        // ak je zapnuty a ma sa vypnut tak sa vypne
        else if (!active && already)
        {
            acSlots.Find(acS => acS.id == id).SetShow();
            change.SetActive(active);
        }
        
        ReloadAttacks();
    }
    public void ReloadAttacks()
    {
        // ziska prechadzjuci aktivny utok
        string prev = acSlots.Find(acS => acS.active)?.Identity;

        // vypne vsetky (aj aktivne) utoky v aktivnych slotoch
        for (int i = 0; i < acSlots.Count; i++)
            acSlots[i].SetShow();

        // nastavi aktivnym utokom parametre od najmensieho po najvacsi pasivny utok
        int ii = 0;
        for (int i = 0; i < atSlots.Length;i++)
            foreach (AttackSlotPassive aS in atSlots[i].GetActive())
            {
                acSlots[ii].Set(aS.attackType, aS.id);
                ii++;
            }

        // ak je miesto nastavi unarmened utok
        if (acSlots.FindAll(acS => acS.show).Count < acSlots.Count)
            acSlots[ii].Set(Attack.Type.RaseUnnarmed, -1);

        // ak je stale povodny utok zapnuty tak zostane zapnuty
        acSlots.Find(acS => acS.Identity == prev)?.Select();
    }
}