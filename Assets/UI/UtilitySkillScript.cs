using UnityEngine;
using System;
using System.Collections.Generic;

public class UtilitySkillScript : MonoBehaviour
{
    [SerializeField] Utility.Function condition;
    //[SerializeField] bool loadCurrentOnStart = true;
    [SerializeField] bool refreshOnChange = true;
    [SerializeField] bool defaultState = false;
    [SerializeField] bool requestStateOnStart = false;
    [SerializeField] List<GameObject> gameObjects;
    GameManager game;
    void Start()
    {
        game = GameManager.instance;
        if (refreshOnChange)
            game.UtilityUpdate += UtilityUpdate;

        if (requestStateOnStart)
            SetGameObjects(game.IsUtilityEnabled(condition));
        else
            SetGameObjects(defaultState);                
    }
    void UtilityUpdate(Utility skill)
    {
        if (condition.Equals(skill.function))
        {
            SetGameObjects(skill.aquired);
        }
    }
    void SetGameObjects(bool setTo = true)
    {
        gameObjects.ForEach(g => {if (gameObject != null) g.SetActive(setTo); });
    }
}