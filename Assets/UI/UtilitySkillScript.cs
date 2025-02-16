using UnityEngine;
using System;
using System.Collections.Generic;

public class UtilitySkillScript : MonoBehaviour
{
    [SerializeField] Utility.Function condition;
    [SerializeField] bool refreshOnChange = true;
    [SerializeField] bool defaultState = false;
    [SerializeField] bool requestStateOnStart = false;
    [SerializeField] bool requestStateOnEnable = false;
    [SerializeField] List<GameObject> gameObjects;
    GameManager game;
    /// <summary>
    /// Pri psusteni
    /// </summary>
    void Start()
    {
        game ??= GameManager.instance;
        if (refreshOnChange)
            game.UtilityUpdate += UtilityUpdate;

        if (requestStateOnStart)
            SetGameObjects(game.IsUtilityEnabled(condition));
        else
            SetGameObjects(defaultState);           
    }
    void Oestroy()
    {
        game.UtilityUpdate -= UtilityUpdate;
    }
    /// <summary>
    /// Pri povoleni
    /// </summary>
    void OnEnable()
    {
        if (requestStateOnEnable)
            SetGameObjects(game.IsUtilityEnabled(condition));
    }
    /// <summary>
    /// Pri zmene (pridani/odobrani) schopnosti <br />
    /// Porovna ci sa zhoduje so zavislostou, ak ano zapne/vypne zavisle objekty
    /// </summary>
    /// <param name="skill"></param>
    void UtilityUpdate(Utility skill)
    {
        if (condition.Equals(skill.function))
        {
            SetGameObjects(skill.aquired);
        }
    }
    /// <summary>
    /// Natavi vsetky zavisle objekty
    /// </summary>
    /// <param name="setTo"></param>
    void SetGameObjects(bool setTo = true)
    {
        gameObjects.ForEach(g => {if (gameObject != null) g.SetActive(setTo); });
    }
}