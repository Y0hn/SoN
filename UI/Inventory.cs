using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
public class Inventory : MonoBehaviour
{
    public Inventory instance;
    [SerializeField] int size = 1;
    [SerializeField] TMP_Text btn;
    [SerializeField] Button button;
    [SerializeField] Transform parent;
    [SerializeField] Animator animator;
    [SerializeField] GameObject slotPreFab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] InputActionReference input;
    bool inv = false;
    const int pixelSize = 600;
    const int spacing = 10;
    void Start()
    {
        button.onClick.AddListener(() => OC_Inventory(new()));
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
            }
        else if (kids < size)
            for (int i = 0; i < size - kids; i++)
                Instantiate(slotPreFab, parent);
        else
            return;

        int space = pixelSize%size + spacing;
        int rows = (int)Math.Ceiling(Math.Sqrt(size));

        space /= rows;
        
        float f = pixelSize / rows - space;

        grid.cellSize = new(f,f);
        grid.spacing = new(space,space);
    }
    
    void OC_Inventory(InputAction.CallbackContext context)
    {
        if (!GameManager.instance.playerLives) return;
        inv = !inv;
        animator.SetBool("open", inv);
        if (inv) btn.text = "<";
        else btn.text = ">";
    }
}

#region Items
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item"), Serializable] public class Item : ScriptableObject
{
    public new string name;
    public string description;
    public Sprite icon;
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
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Weapon"), Serializable] public class Weapon : Item
{
    public Attack attack;
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Money"), Serializable] public class Coin : Item
{
    public int amount;
}
#endregion