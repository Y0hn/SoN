using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
/// <summary>
/// Spravuje schopnosti lokalneho hraca
/// </summary>
public class SkillPanel : MonoBehaviour
{
    public event Action<bool> OnChangeAvailablePoints;
    public bool AvailablePoints => 0 < freePointCouter;
    [SerializeField] TMP_Text skillCounterText;
    [SerializeField] Transform skills;
    [SerializeField] HoldButton button;
    [SerializeField] GameManager game;
    [SerializeField] Vector2 limitPosition;
    [SerializeField] Vector2 limitOffset;
    [SerializeField] bool ReloadLimimts = false;

    Vector2[] limits = new Vector2[2];
    Vector2 startMouse;
    Dictionary<string, SkillSlot> skillSlots = new();
    byte usedPointsCounter = 0, freePointCouter = 0;
    bool awoken = false;
    /// <summary>
    /// Nastavi pociatocne hodnoty pre pocitadla <br />
    /// Vypocita limity pohybu stromu
    /// </summary>
    public void Awake()
    {
        if (awoken) return;

        freePointCouter = 0; 
        usedPointsCounter = 0;
        button.onEnterHold += delegate { startMouse = game.MousePos; };
        CalculateLimits();
        foreach (var skS in transform.GetComponentsInChildren<SkillSlot>())
            skillSlots.Add(skS.name, skS);
        skillCounterText.text = freePointCouter.ToString();

        awoken = true;
        //OnChangeAvailablePoints += (bool change) => { Debug.Log($"OnChangeAvailablePoints.Invoked({change})"); };
    }
    /// <summary>
    /// Animovanie tahania stromu schopnosti
    /// </summary>
    void FixedUpdate()
    {
        if (button.isHolding)
        {
            Vector2 v = game.MousePos - startMouse;
            startMouse = game.MousePos;
            MoveSkills(v);
        }
        if (ReloadLimimts)
            CalculateLimits();
    }
    /// <summary>
    /// Vypocita hranice pohybu pre grafiku stromu schopnosti
    /// </summary>
    private void CalculateLimits()
    {
        limits[0] = new (
            Mathf.Round(transform.position.x-limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y-limitPosition.y-limitOffset.y));
        limits[1] = new (
            Mathf.Round(transform.position.y+limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y+limitPosition.y-limitOffset.y));
    }
    /// <summary>
    /// Pripocitanie bodu po dosiahnuti urovne
    /// </summary>
    /// <param name="level"></param>
    public void LevelUP (byte level)
    {
        freePointCouter = (byte)(level - usedPointsCounter);
        skillCounterText.text = freePointCouter.ToString();
        OnChangeAvailablePoints?.Invoke(AvailablePoints);
    }
    /// <summary>
    /// Nastava ked je schopnost kupena, 
    /// </summary>
    public void SkillPointAplied()
    {
        if (AvailablePoints)
        {
            usedPointsCounter++;
            freePointCouter--;
            skillCounterText.text = freePointCouter.ToString();
            OnChangeAvailablePoints?.Invoke(AvailablePoints);
        }
    }
    /// <summary>
    /// Hybe stromom schopnosti v ramci vopred vypocitanych hranic
    /// </summary>
    /// <param name="moveBy"></param>
    public void MoveSkills (Vector2 moveBy)
    {
        Vector2 newPosition = new (skills.position.x + moveBy.x, skills.position.y + moveBy.y);

        if  (limits[0].x < newPosition.x && newPosition.x < limits[1].x
                &&
             limits[0].y < newPosition.y && newPosition.y < limits[1].y)
        {
            skills.position = newPosition;
            /*
            Debug.Log(  $"Moving skillTree to ({newPosition.x},{newPosition.y})\n" + 
                        $"Base ({transform.position.x},{transform.position.y})\n" +
                        $"Limits: \n0 => ({limits[0].x},{limits[0].y}) \n1 => ({limits[1].x},{limits[1].y})");*/
        }
    }
    /// <summary>
    /// Nacitava schopnosti zo suboru
    /// </summary>
    /// <param name="loadSkills"></param>
    public void LoadSkills(World.PlayerSave player)
    {
        FileManager.Log($"Skills loaded {player.etName} count={player.skillTree.skills.Length} level={player.level}",FileLogType.RECORD);
        freePointCouter = player.level;
        List<string> skills= new();

        foreach (Skill skill in player.skillTree.skills)
            skills.Add(skill.name);

        SkillSlot[] sSlots = GetComponentsInChildren<SkillSlot>();

        foreach (SkillSlot s in sSlots)
            if (skills.Contains(s.name))
                s.ActivateSkill();

        OnChangeAvailablePoints?.Invoke(AvailablePoints);
    }
    /// <summary>
    /// Sluzi pre kupu schopnosti v okruznou cestou
    /// </summary>
    /// <param name="skillName"></param>
    public void BuySkill(string skillName)
    {
        if (skillSlots.ContainsKey(skillName))
            skillSlots[skillName].BuySkill();
        else
            FileManager.Log($"Load Skill failed {skillName}, count= {skillSlots.Count}", FileLogType.ERROR);
    }
}