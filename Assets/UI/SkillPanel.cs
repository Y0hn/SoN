using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
public class SkillPanel : AwakeBehavior
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
    byte usedPointsCounter = 0, freePointCouter = 0;
    public override void Awake()
    {
        freePointCouter = 0; 
        usedPointsCounter = 0;
        button.onEnterHold += delegate
        {
            startMouse = game.MousePos;
        };
        CalculateLimits();
        skillCounterText.text = freePointCouter.ToString();
        //OnChangeAvailablePoints += (bool change) => { Debug.Log($"OnChangeAvailablePoints.Invoked({change})"); };
    }
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
    private void CalculateLimits()
    {
        limits[0] = new (
            Mathf.Round(transform.position.x-limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y-limitPosition.y-limitOffset.y));
        limits[1] = new (
            Mathf.Round(transform.position.y+limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y+limitPosition.y-limitOffset.y));
    }
    public void LevelUP (byte level)
    {
        freePointCouter = (byte)(level - usedPointsCounter);
        skillCounterText.text = freePointCouter.ToString();
        OnChangeAvailablePoints?.Invoke(AvailablePoints);
        //Debug.Log("After level up to " + level + " AvailablePoints= " + AvailablePoints);
    }
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
    public void LoadSkills(Skill[] loadSkills)
    {
        List<string> skills= new();
        foreach (Skill skill in loadSkills)
            skills.Add(skill.name);

        SkillSlot[] sSlots = GetComponentsInChildren<SkillSlot>();
        foreach (SkillSlot s in sSlots)
            if (skills.Contains(s.name))
                s.ActivateSkill();
    }
}