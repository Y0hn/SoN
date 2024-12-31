using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SkillSlot : MonoBehaviour
{
    public Action<bool> SetActive;
    
    [SerializeField] SkillCreator skillCreator;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Image background;
    [SerializeField] SkillSlot dependentcySkill;
    
    private static Dictionary<string, Color> pallete = new()
    {
        { "unavailable", new ( 76/255,  76/255,  76/255, 0.75f) },  // disabled //rgba(76, 76, 76, 0.75)
        { "nextInLine",  new (255/255, 255/255, 255/255, 1f   ) },  // enabled  //rgba(204, 204, 204, 0.75)
        { "unafordable", new (128/255, 128/255, 128/255, 0.75f) },  // disabled //rgba(128, 128, 128, 0.75)
        { "aquired",     new ( 89/255,  89/255,  89/255, 1f   ) },  // disabled //rgba(89, 89, 89, 1)
    };    
    private Color[] defaultColors = new Color[2];
    private SkillTree.Skill skill;
    private GameManager game;
    private bool purchased;
    void Start()
    {
        button.interactable = false;
        button.onClick.AddListener(ActivateSkill);
        skill = skillCreator.Skill;
        defaultColors[1] = icon.color;
        defaultColors[0] = background.color;
        game = GameManager.instance;
        SetGraficColor(pallete["unavailable"]);

        if (dependentcySkill != null)
            dependentcySkill.SetActive += (bool prevAcive) => 
            { 
                if (!game.SkillTree.AvailablePoints)
                    game.SkillTree.OnAvailablePoints += PurchableSkill;
                else 
                    PurchableSkill(true);
            };
        else
            game.SkillTree.OnAvailablePoints += PurchableSkill;
    }
    /// <summary>
    /// Adds skill to Player Skill Tree and Enables Dependent skills
    /// </summary>
    void ActivateSkill()
    {
        game.SkillTree.OnAvailablePoints -= PurchableSkill;
        game.LocalPlayer.AddSkillRpc(skill);
        game.SkillTree.SkillPointAplied();
        SetGraficColor(pallete["aquired"]);
        button.interactable = false;
        SetActive?.Invoke(true);
        purchased= true;
    }
    /// <summary>
    /// Sets Skill (un)available to get by spending point
    /// </summary>
    /// <param name="enable"></param>
    void PurchableSkill(bool enable)
    {
        if (!purchased)
        {
            button.interactable = enable;
            if (enable)
                SetGraficColor(pallete["nextInLine"]);
            else
                SetGraficColor(pallete["unafordable"]);
        }
    }
    void SetGraficColor(Color color)
    {
        icon.color =       color * defaultColors[1]; // Color.Lerp(defaultColors[1], color, 0.5f);
        background.color = color * defaultColors[0]; // Color.Lerp(defaultColors[0], color, 0.5f);
    }
    [Serializable] class SkillCreator
    {
        public SkillType skillType;
        public string name;
        [SerializeField] float amount;
        [SerializeField] Damage.Type condition;
        public SkillTree.Skill Skill 
        {
            get
            {
                SkillTree.Skill skill;
                switch (skillType)
                {
                    case SkillType.Health:
                        int a = Mathf.RoundToInt(amount);
                        skill = new SkillTree.Health(name, a);
                        break;
                    case SkillType.Attack:
                        skill = new SkillTree.Combat(name, amount, condition);
                        break;
                    case SkillType.Protection:
                        float am = -amount;
                        skill = new SkillTree.Combat(name, am, condition);
                        break;
                    case SkillType.Utility:
                    default: 
                        skill = new(name);
                        break;
                }
                return skill;
            }
        }
        public enum SkillType
        {
            Utility, Health, Protection, Attack
        }
    }
}