using UnityEngine.UI;
using UnityEngine;
using System;
[Serializable] public abstract class AttackSlot
{
    public sbyte id;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [HideInInspector] public bool active = false, show = false;
    public abstract void SetShow(bool show = false);
    public abstract void SetActive(bool active = true);
}
[Serializable] public class AttackSlotPassive : AttackSlot
{
    [field:SerializeField] protected Image placeHolder;
    [field:SerializeField] protected Button button;
    [HideInInspector] public Attack.Type attackType;
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
        Debug.Log("Active attcak slot setted \naref= " + aref);
    }
    /// <summary>
    /// Vypina/Zapina zobrazovanie utoku v ramceku
    /// </summary>
    /// <param name="show">zap/vyp</param>
    public override void SetShow(bool show = false)
    {
        foreground.enabled = show;
        placeHolder.enabled = show;
        button.interactable = show;
        if (!show)
            SetActive(false);
        this.show = show;
    }
    public override void SetActive(bool active = true)
    {
        if (active)
            background.color = activeC;
        else
            background.color = passiveC;
        this.active = active;
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
    private static Color 
        activeC = new (50f/255f, 103f/255f, 30f/255f),      // #32671e
        passiveC = new(64f/255f,  64f/255f, 64f/255f);      // #404040
    public void Set(Attack.Type aType, sbyte id)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        this.id = id;
        SetShow(true);
        Debug.Log("Active attcak slot setted \naref= " + aref);
    }
    public override void SetShow (bool show = false)
    {
        foreground.gameObject.SetActive(show);
        this.show = show;
        if (!show)
        {
            //id = -1;  vytvara Stack ovorflow
            SetActive(false);
        }
    }
    public override void SetActive (bool active = true)
    {
        if (active)
            edge.color = activeC;
        else
            edge.color = passiveC;
        this.active = active;
    }
}