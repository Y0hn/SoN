using UnityEngine;
using System;
using System.Collections.Generic;

public class UtilitySkillScript : MonoBehaviour
{
    [SerializeField] SkillTree.Utility.Function condition;
    //[SerializeField] bool loadCurrentOnStart = true;
    [SerializeField] bool refreshOnChange = true;
    [SerializeField] bool defaultState = false;
    [SerializeField] List<GameObject> gameObjects;
    GameManager game;
    void Start()
    {
        game = GameManager.instance;
        if (refreshOnChange)
            game.UtilityUpdate += UtilityUpdate;
        SetGameObjects(defaultState);        
    }
    void UtilityUpdate(UtilitySkill skill)
    {
        if (condition.Equals(skill.function))
        {
            SetGameObjects(skill.aquired);
        }
    }
    void SetGameObjects(bool setTo = true)
    {
        gameObjects.ForEach(g => g.SetActive(setTo) );
    }
}
[Serializable] public class UtilitySkill
{
    public SkillTree.Utility.Function function;
    public bool aquired = false;
    public UtilitySkill(SkillTree.Utility.Function f, bool a = false)
    {
        function = f;
        aquired = a;
    }
}