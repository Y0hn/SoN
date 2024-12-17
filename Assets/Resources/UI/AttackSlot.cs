//using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackSlotScript : MonoBehaviour
{
    [SerializeField] List<AttackSlot.Passive> attackSlots;
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
                attackSlots[i].Setup(false);
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
    public List<AttackSlot.Passive> GetActive()
    {
        return attackSlots.FindAll(atS => atS.active);
    }
    public bool SetActive(int id = -1, bool active = false)
    {
        if (id < 0) id = attackSlots.Count-1;
        bool prev = attackSlots[id].active;
        attackSlots[id].SetActive(active);
        return prev != active;
    }
}
[Serializable] public abstract class AttackSlot
{
    public string name;
    [field:SerializeField] protected Image background;
    [field:SerializeField] protected Image foreground;
    [HideInInspector] public bool active, show;
    public abstract void Setup(bool show = false);
    public abstract void SetActive(bool active = true);

    [Serializable] public class Passive : AttackSlot
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
            button.onClick.AddListener(delegate { OnButtonClick(aType); });
            SetActive(active);
            Setup(true);
        }
        /// <summary>
        /// Vypina/Zapina zobrazovanie utoku v ramceku
        /// </summary>
        /// <param name="show">zap/vyp</param>
        public override void Setup(bool show = false)
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
        private void OnButtonClick(Attack.Type type)
        {
            GameManager.instance.inventory.SetActiveAttackType(type);
        }
    }
    [Serializable] public class Active : AttackSlot
    {
        [HideInInspector] public Equipment.Slot slot;
        [field:SerializeField] protected Image edge;
        private static Color 
            activeC = new (50f/255f, 103f/255f, 30f/255f),      // #32671e
            passiveC = new(64f/255f,  64f/255f, 64f/255f);      // #404040
        public void Set(Attack.Type aType)
        {
            string aref = FileManager.GetAttackRefferency(aType);
            foreground.sprite = Resources.Load<Sprite>(aref);
            Setup (true);
        }
        public override void Setup (bool show = false)
        {
            foreground.gameObject.SetActive(show);
            this.show = show;
            if (!show)
                SetActive(false);
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
}