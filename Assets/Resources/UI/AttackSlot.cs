using UnityEngine.UI;
using UnityEngine;
using System;
[Serializable] public abstract class AttackSlot
{
    public sbyte id;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [HideInInspector] public Attack.Type attackType;
    /*[HideInInspector]*/ public bool active = false, show = false;
    public virtual void SetShow(bool show = false)
    {
        this.show = show;
        if (!show)
            SetActive(show);
    }
    public virtual void SetActive(bool active = true)
    {
        this.active = active;
    }
}
[Serializable] public class AttackSlotPassive : AttackSlot
{
    [field:SerializeField] protected Image placeHolder;
    [field:SerializeField] protected Button button;
    private static Color 
        activeC = new (190f/255f,  72f/255f, 31f/255f),      // #BE481F
        passiveC = new(149f/255f, 100f/255f, 65f/255f);      // #956441
    public void Set(Attack.Type aType, bool active = false)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
        attackType = aType;
        SetActive(active);
        SetShow(true);
        //Debug.Log("Active attcak slot setted \naref= " + aref);
    }
    /// <summary>
    /// Vypina/Zapina zobrazovanie utoku v ramceku
    /// </summary>
    /// <param name="show">zap/vyp</param>
    public override void SetShow(bool show = false)
    {
        base.SetShow(show);
        foreground.enabled = show;
        placeHolder.enabled = show;
        button.interactable = show;
    }
    public override void SetActive(bool active = true)
    {
        if (!show && active) return;
        base.SetActive(active);
        if (active)
            background.color = activeC;
        else
            background.color = passiveC;
    }
    /// <summary>
    /// Nastavi aktivny utok pre danu zbran ako tento utok
    /// </summary>
    /// <param name="type">Typ utoku zbrane</param>
    private void OnButtonClick()
    {
        SetActive(!active);
        GameManager.instance.inventory.SetActiveAttackType(id, active);
    }
}
[Serializable] public class AttackSlotActive : AttackSlot
{
    [HideInInspector] public Equipment.Slot slot;
    [field:SerializeField] protected Image edge;
    private static Action<bool> ChangeActive;
    private static Color 
        activeC = new (50f/255f, 103f/255f, 30f/255f),      // #32671e
        passiveC = new(64f/255f,  64f/255f, 64f/255f),      // #404040
        defaultC = new(1f,1f,1f);                           // #ffffff
    public WeaponIndex Weapon 
    { 
        get 
        { 
            sbyte 
                attack = (sbyte)(id%10), 
                weapon = (sbyte)(id-attack);
            attack--;
            weapon = weapon == 1 ? (sbyte)Equipment.Slot.WeaponR : (sbyte)Equipment.Slot.WeaponL;
            return new(weapon, attack);
        } 
    }
    public void Set(Attack.Type aType, sbyte id)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        if (id > 0)
            foreground.color = defaultC;
        else
            foreground.color = GameManager.instance.LocalPlayer.Color;
        attackType = aType;
        this.id = id;
        SetShow(true);

        // ak neni ziadni iny utok aktivny
        if (ChangeActive == null)
            Select();
        //Debug.Log("Active attcak slot setted \naref= " + aref);
    }
    public void Select()
    {
        SetActive(!active);
        if (active)
            GameManager.instance.LocalPlayer.SetWeaponIndex(id);
    }
    public override void SetShow (bool show = false)
    {
        base.SetShow(show);
        foreground.gameObject.SetActive(show);
    }
    public override void SetActive (bool active = true)
    {
        if (!show && active) return;
        base.SetActive(active);

        if (active)
        {
            edge.color = activeC;
            ChangeActive?.Invoke(!active);
            ChangeActive += SetActive;
        }
        else
        {
            edge.color = passiveC;
            ChangeActive -= SetActive;
        }
        //sDebug.Log($"SetActive active= {active} => this.active= {this.active}");
    }
}