using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// V inventari drzi a spravuje vestky passivne utoky pre jednu zbran
/// </summary>
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
    public void Click(int id)
    {
        if (0 <= id && id < attackSlots.FindAll(atS => atS.show).Count)
            attackSlots[id].OnButtonClick();
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
    /// <summary>
    /// Zapne alebo Vypne konkretny pasivny utok
    /// </summary>
    /// <param name="id"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    public bool SetActive(int id = -1, bool active = false)
    {
        if      (id == -1) 
            id = attackSlots.Count-1;
        else if (id == -2)
            id = GetActive().Count;

        if (0 <= id && id < attackSlots.FindAll(atS => atS.show).Count)
        {
            bool prev = attackSlots[id].active;
            attackSlots[id].SetActive(active);
            Debug.Log($"Setted [{id}] Active to \"{active}\"");
            return prev != active;
        }
        return false;
    }
    /// <summary>
    /// Ak je viac zapnutych pasivnych utokov ako aktivnych utokov (3)
    /// </summary>
    /// <returns>ID vypnuteho</returns>
    public int ShutLastActive()
    {
        List<AttackSlotPassive> atsP = GetActive();
        if (0 < atsP.Count)
        {
            // prvy od konca
            int id = atsP[^1].id;
            if (SetActive(id))
                return id;
        }
        return -1;
    }
}