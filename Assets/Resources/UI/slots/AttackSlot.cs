using UnityEngine.UI;
using UnityEngine;
using System;
/// <summary>
/// Sluzi ako zakladna pre Drzitelov utokov 
/// </summary>
[Serializable] public abstract class AttackSlot
{
    /// <summary>
    /// identifikacia
    /// </summary>
    public sbyte id;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [HideInInspector] public Attack.Type attackType;
    public bool active = false, show = false;
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
/// <summary>
/// Drzi utok zbrane v inventari <br />
/// Po aktivacii prida hodnoty z utoku medzi aktivne utoky <br />
/// </summary>
[Serializable] public class AttackSlotPassive : AttackSlot
{
    [field:SerializeField] protected Image placeHolder;
    [field:SerializeField] protected Button button;
    /// <summary>
    /// Zadava zmenu farby podla toho ci je utok aktivny alebo pasivny
    /// </summary>
    /// <param name="72f/255f"></param>
    /// <returns></returns>
    private static Color 
        activeC = new (190f/255f,  72f/255f, 31f/255f),      // #BE481F
        passiveC = new(149f/255f, 100f/255f, 65f/255f);      // #956441
    /// <summary>
    /// Sluzi na nastavenie utoku a jeho grafiku podla typu utoku
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="active"></param>
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
    /// <summary>
    /// Nastavi ci je drzitel aktivny
    /// </summary>
    /// <param name="active"></param>
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
    /// Prida tento utok medzi aktivne utoky (max. 3)
    /// </summary>
    /// <param name="type">Typ utoku zbrane</param>
    public void OnButtonClick()
    {
        SetActive(!active);
        GameManager.instance.inventory.SetActiveAttackType(id, active);
    }
}
/// <summary>
/// Drzi typ utoku v Hracovom Panely UI <br />
/// Po aktivacii sa nastavi ako aktuany utok hraca   
/// </summary>
[Serializable] public class AttackSlotActive : AttackSlot
{
    [HideInInspector] public Equipment.Slot slot;
    [field:SerializeField] protected Image edge;
    private static Action<bool> ChangeActive;
    /// <summary>
    /// Nastavenie farby pre rozne stavy
    /// </summary>
    /// <returns></returns>
    private static Color 
        activeC = new (50f/255f, 103f/255f, 30f/255f),      // #32671e
        passiveC = new(64f/255f,  64f/255f, 64f/255f),      // #404040
        defaultC = new(1f,1f,1f);                           // #ffffff
    /// <summary>
    /// Vyrobi ukazovatel drzanej zbrane podla parametrov 
    /// </summary>
    /// <value>UKAZOVATEL_ZBRANE</value>
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
    private string aRef;
    public string Identity { get => aRef; }
    /// <summary>
    /// Nastavi 
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="id"></param>
    public void Set(Attack.Type aType, sbyte id)
    {
        aRef = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aRef);
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
        //Debug.Log($"SetActive active= {active} => this.active= {this.active}");
    }
}