using UnityEngine.UI;
using UnityEngine;
using System;
#region base Slot Utoku
/// <summary>
/// Sluzi ako zakladna pre Drzitelov utokov 
/// </summary>
[Serializable] public abstract class AttackSlot
{
    /// <summary>
    /// identifikacia
    /// </summary>
    public sbyte id;
    [field:SerializeField] protected Button button;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [HideInInspector] public Damage.Type type;
    public bool active = false, show = false;
    /// <summary>
    /// Nastavuje viditelnost texutry
    /// </summary>
    /// <param name="show"></param>
    public virtual void SetShow(bool show = false)
    {
        this.show = show;
        button.interactable = show;


        button.onClick.RemoveAllListeners();
        if (!show)
            SetActive(show);
        else
            button.onClick.AddListener(OnButtonClick);
    }
    /// <summary>
    /// Nastavuje / povoluje slot
    /// </summary>
    /// <param name="active"></param>
    public virtual void SetActive(bool active = true)
    {
        this.active = active;
    }
    /// <summary>
    /// Po stlaceni tlacitka
    /// </summary>
    public abstract void OnButtonClick();
    /// <summary>
    /// 
    /// </summary>
    /// <returns>suhrn PARAMETROV</returns>
    public override string ToString() => $"[ID {id}] damage= {Enum.GetName(typeof(Damage.Type), type)}, act= {active}, sh= {show}";
}
#endregion
#region Povolenie Utoku
/// <summary>
/// Drzi utok zbrane v inventari <br />
/// Po aktivacii prida hodnoty z utoku medzi aktivne utoky <br />
/// </summary>
[Serializable] public class AttackSlotPassive : AttackSlot
{
    [field:SerializeField] protected Image placeHolder;
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
    public void Set(Damage.Type aType, bool disable = false)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        type = aType;
        if (disable)
            SetActive(false);
        SetShow(true);
    }
    /// <summary>
    /// Vypina/Zapina zobrazovanie utoku v ramceku
    /// </summary>
    /// <param name="show">zap/vyp</param>
    public override void SetShow(bool show = true)
    {
        base.SetShow(show);
        background.gameObject.SetActive(show);
        foreground.enabled = show;
        placeHolder.enabled = show;
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
    /// <inheritdoc/> <br />
    // Prida tento utok medzi aktivne utoky (max. 3)
    /// </summary>
    /// <param name="type">Typ utoku zbrane</param>
    public override void OnButtonClick()
    {
        SetActive(!active);
        GameManager.instance.inventory.SetActiveAttackType(id, active);
    }
}
#endregion
#region Rychly Utok
/// <summary>
/// Drzi typ utoku v Hracovom Panely UI <br />
/// Po aktivacii sa nastavi ako aktuany utok hraca   
/// </summary>
[Serializable] public class AttackSlotActive : AttackSlot
{
    [field:SerializeField] protected Image edge;
    [field:SerializeField] protected Image num;
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
            byte 
                attack = (byte)(id%10),
                weapon = 0;

            if (0 < id)
            {
                weapon = (byte)(id/10);
            }
            return new(weapon, attack);
        } 
    }
    private string aRef; public string Identity => aRef; 


    /// <summary>
    /// Nastavi 
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="id"></param>
    public void Set(Damage.Type aType, sbyte id)
    {
        aRef = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aRef);
        foreground.color = defaultC;

        type = aType;
        this.id = id;
        SetShow(true);

        // ak neni ziadny iny utok aktivny
        if (ChangeActive == null)
            Select();
        //FileManager.Log("Active attcak slot setted \naref= " + aref);
    }
    public void Select(bool over = false)
    {
        SetActive(!active);

        if (active && GameManager.instance.LocalPlayer != null)
            GameManager.instance.LocalPlayer.SetWeaponIndex(Weapon);
    }
    public override void SetShow (bool show = false)
    {
        base.SetShow(show);
        num.gameObject.SetActive(show);
        foreground.gameObject.SetActive(show);
    }
    public override void SetActive (bool active = true)
    {
        if (!show && active || this.active && active)
        {
            //FileManager.Log("Already active");
            return;
        }

        if (active)
        {
            num.color = activeC;
            edge.color = activeC;
            ChangeActive?.Invoke(!active);
            ChangeActive += SetActive;
        }
        else
        {
            num.color = passiveC;
            edge.color = passiveC;
            ChangeActive -= SetActive;
        }

        base.SetActive(active);

        //FileManager.Log($"SetActive {Identity} {Weapon} active= {active} => this.active= {this.active}");
    }
    public override void OnButtonClick()
    {
        SetActive(true);
    }

    public override string ToString()
    {
        return base.ToString() + $" Identity= {aRef} WeaponE= {Weapon}";
    }
}
#endregion