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
    [SerializeField] TMP_Text skillCounterText;
    [SerializeField] RectTransform skills;
    [SerializeField] HoldButton button;
    [SerializeField] GameManager game;
    [SerializeField] Vector2 limitPosition;
    [SerializeField] Vector2 limitOffset;
    [SerializeField] bool ReloadLimimts = false;

    Vector2[] limits = new Vector2[2];
    Vector2 startMouse;
    Dictionary<string, SkillSlot> skillSlots = new();
    byte 
        usedPoints = 0, 
        totalPoints = 0;

    public bool AvailablePoints => 0 < Points;
    private int Points => totalPoints > usedPoints ? totalPoints - usedPoints : 0; 

    bool awoken = false;
    /// <summary>
    /// Nastavi pociatocne hodnoty pre pocitadla <br />
    /// Vypocita limity pohybu stromu
    /// </summary>
    public void Awake()
    {
        if (awoken) return;

        button.onEnterHold += delegate { startMouse = game.MousePos; };
        CalculateLimits();
        foreach (var skS in transform.GetComponentsInChildren<SkillSlot>())
            skillSlots.Add(skS.name, skS);
        skillCounterText.text = Points.ToString();

        GameManager.GameQuit += Clear;

        awoken = true;
    }
    /// <summary>
    /// Znova prepocita limity
    /// </summary>
    void OnEnable() => CalculateLimits();
    /// <summary>
    /// Animovanie tahania stromu schopnosti
    /// </summary>
    void Update()
    {
        if (button.isHolding)
        {
            Vector2 v = game.CornerMousePos - startMouse;
            startMouse = game.CornerMousePos;
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
        // ohranicuje lavu sranu a spodok
        limits[0] = new (
            Mathf.Round(transform.position.x-limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y-limitPosition.y-limitOffset.y));

        // ohranicuje provu sranu a vrch
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
        totalPoints = level;
        skillCounterText.text = Points.ToString();
        OnChangeAvailablePoints?.Invoke(AvailablePoints);
    }
    /// <summary>
    /// Nastava ked je schopnost kupena, 
    /// </summary>
    public void SkillPointAplied()
    {
        if (AvailablePoints)
        {
            usedPoints++;
            skillCounterText.text = Points.ToString();
            OnChangeAvailablePoints?.Invoke(AvailablePoints);
        }
    }
    /// <summary>
    /// Hybe stromom schopnosti v ramci vopred vypocitanych hranic
    /// </summary>
    /// <param name="moveBy"></param>
    public void MoveSkills (Vector2 moveBy)
    {
        Vector2 newPosition = new (skills.localPosition.x + moveBy.x, skills.localPosition.y + moveBy.y);

        bool canMove = limits[0].x < newPosition.x && newPosition.x < limits[1].x;
        canMove &= limits[0].y < newPosition.y && newPosition.y < limits[1].y;

        if  (canMove)
        {
            skills.localPosition = newPosition;
            /*
            Debug.Log(  $"Moving skillTree to ({newPosition.x},{newPosition.y})\n" + 
                        $"Base ({transform.position.x},{transform.position.y})\n" +
                        $"Limits: \n0 => ({limits[0].x},{limits[0].y}) \n1 => ({limits[1].x},{limits[1].y})");*/
        }
        else
        {
            FileManager.Log($"Cannot move to ({newPosition.x},{newPosition.y}) Base ({transform.position.x},{transform.position.y})"+
                            $"Limits: 0 => ({limits[0].x},{limits[0].y}) 1 => ({limits[1].x},{limits[1].y})");
        }
    }
    /// <summary>
    /// Nacitava schopnosti zo suboru
    /// </summary>
    /// <param name="loadSkills"></param>
    public void LoadSkills(World.PlayerSave player)
    {
        FileManager.Log($"Skills loaded {player.etName} count={player.skillTree.skills.Length} level={player.level}",FileLogType.RECORD);
        totalPoints = player.level;
        List<string> skills= new();

        foreach (Skill skill in player.skillTree.skills)
            skillSlots[skill.name].ActivateSkill();

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
    /// <summary>
    /// Resetuje cely strom schopnosti
    /// </summary>
    public void Clear()
    {
        foreach (var s in skillSlots)
            s.Value.Restart();

        totalPoints = 0;
        usedPoints = 0;
    }
}