using System.Collections.Generic;
using UnityEngine;

public class PassiveAttackSlotScript : MonoBehaviour
{
    [SerializeField] List<AttackSlotPassive> attackSlots;
    public void SetAttacks(List<Attack> attacks)
    {
        for (int i = 0; i < attackSlots.Count; i++)
        {
            if (i < attacks.Count)
                attackSlots[i].Set(attacks[i].type);
            else
                attackSlots[i].SetShow(false);
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
    public List<AttackSlotPassive> GetActive()
    {
        return attackSlots.FindAll(atS => atS.active);
    }
    public AttackSlotPassive GetSlot(int id)
    {
        return attackSlots.Find(atS => atS.id == id);
    }
    public bool SetActive(int id = -1, bool active = false)
    {
        if (id < 0) id = attackSlots.Count-1;
        if (id < 0)
        {
            bool prev = attackSlots[id].active;
            attackSlots[id].SetActive(active);
            return prev != active;
        }
        return false;
    }
    public int ShutLastActive()
    {
        List<AttackSlotPassive> atsP = GetActive();
        if (atsP.Count > 0)
        {
            int id = atsP[^1].id;
            if (SetActive(id))
                return id;
        }
        return -1;
    }
}