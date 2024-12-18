using System.Collections.Generic;
using UnityEngine;

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