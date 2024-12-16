//using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackSlot : MonoBehaviour
{
    [SerializeField] List<PassiveAttackSlot> attackSlots;
    //[SerializeField] SerializedDictionary<string, Button> buttons;

    public void SetAttacks(List<Attack> attacks)
    {
        for (int i = 0; i < attackSlots.Count; i++)
        {
            if (i < attacks.Count)
            {
                attackSlots[i].Set(attacks[i].type);
            }
            else
            {
                attackSlots[i].Set(false);
            }
        } 
        gameObject.SetActive(true);
    }
    public void UnsetAttacks()
    {
        SetAttacks(new());
        gameObject.SetActive(false);
    }
    public bool IsSelected()
    {
        bool selected = false;
        
        for (int i = 0; i < attackSlots.Count && !selected; i++)
            selected = attackSlots.Find(x => x.active) != null;
            
        return selected;
    }
}
[Serializable] public class PassiveAttackSlot
{
    public string name;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [field:SerializeField] private Image placeHolder;
    [field:SerializeField] private Button button;

    static Color 
        activeC = new (74f/255f, 60f/255f, 27f/255f), 
        passiveC = new(58f/255f, 39f/255f, 25f/255f);
    [HideInInspector] public bool active, show;
    public virtual void Set(Attack.Type aType, bool active = false)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { OnButtonClick(aType); });
        SetActive(active);
        Set(true);
    }
    public virtual void Set(bool show = false)
    {
        foreground.enabled = show;
        placeHolder.enabled = show;
        button.interactable = show;
        if (!show)
            SetActive(false);

        this.show = show;
    }
    public virtual void SetActive(bool active = true)
    {
        if (active)
            background.color = activeC;
        else
            background.color = passiveC;

        this.active = active;
    }
    private void OnButtonClick(Attack.Type type)
    {

    }
}
[Serializable] public class ActiveAttackSlot : PassiveAttackSlot
{
    [HideInInspector] public Equipment.Slot slot;
    [field:SerializeField] protected Image edge;
    static Color 
        activeC = new (50f/255f, 103f/255f, 30f/255f), 
        passiveC = new(64f/255f,  64f/255f, 64f/255f);
    public void Set(Attack.Type aType)
    {
        string aref = FileManager.GetAttackRefferency(aType);
        foreground.sprite = Resources.Load<Sprite>(aref);
        Set(true);
    }
    public override void Set(bool show = false)
    {
        foreground.gameObject.SetActive(show);
        this.show = show;
        if (!show)
            SetActive(false);
    }
    public override void SetActive(bool active = true)
    {
        if (active)
            edge.color = activeC;
        else
            edge.color = passiveC;
        this.active = active;
    }
}