using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class SkillSlot : MonoBehaviour
{
    public bool isPurchased { get; private set; }

    [SerializeField] SkillCreator skillCreator;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Image background;
    [SerializeField] List<SkillSlot> dependentcySkills;
    [SerializeField] bool needsAllDependecies = false;
    
    private static Dictionary<string, Color> pallete = new()
    {
        { "unavailableIc",  new (128f/255f, 128f/255f, 128f/255f, 0.5f ) },  // disabled //rgba(76, 76, 76, 0)
        { "unavailableBG",  new (  0f/255f,   0f/255f,   0f/255f, 1f   ) },  // disabled //rgba(76, 76, 76, 0)

        { "nextInLine",     new (255f/255f, 255f/255f, 255f/255f, 1f   ) },  // enabled  //rgba(204, 204, 204, 0.75)

        { "unafordableIc",  new (128f/255f, 128f/255f, 128f/255f, 0.75f) },  // disabled //rgba(128, 128, 128, 0.75)
        { "unafordableBG",  new (192f/255f, 192f/255f, 192f/255f, 1f   ) },  // disabled //rgba(128, 128, 128, 0.75)

        { "aquired",        new ( 89f/255f,  89f/255f,  89f/255f, 1f   ) },  // disabled //rgba(89, 89, 89, 1)
    };    
    private Color[] defaultColors = new Color[2];
    private SkillTree.Skill skill;
    private GameManager game;

    private bool DependenciesFullfiled 
    {
        get =>
            (needsAllDependecies && (dependentcySkills.Find(dS => dS.isPurchased) != null)) 
                || 
            (!needsAllDependecies && (dependentcySkills.FindAll(dS => dS.isPurchased).Count == dependentcySkills.Count))
                || 
            dependentcySkills.Count == 0 ;  // zacitocny skill
    }
    void Start()
    {
        button.interactable = false;
        button.onClick.AddListener(ActivateSkill);
        skill = skillCreator.Skill;
        defaultColors[1] = icon.color;
        defaultColors[0] = background.color;
        game = GameManager.instance;
        game.SkillTree.OnChangeAvailablePoints += PurchableSkill;
        SetGraficColor(pallete["unavailableIc"], pallete["unavailableBG"]);
    }
    /// <summary>
    /// Adds skill to Player Skill Tree and Enables Dependent skills
    /// </summary>
    void ActivateSkill()
    {
        isPurchased = true;
        button.interactable = false;

        game.LocalPlayer.AddSkillRpc(skill);
        game.SkillTree.SkillPointAplied();
        SetGraficColor(pallete["aquired"]);
    }
    /// <summary>
    /// Sets Skill (un)available to get by spending point
    /// </summary>
    /// <param name="enable"></param>
    void PurchableSkill(bool enable)
    {
        // ak aspon jedna zavislost je splnena
        if (!isPurchased && DependenciesFullfiled)
        {
            button.interactable = enable;
            if (enable)
                SetGraficColor(pallete["nextInLine"]);
            else
                SetGraficColor(pallete["unafordableIc"], pallete["unafordableBG"]);
        }
    }
    void SetGraficColor(Color color)
    {
        SetGraficColor(color, color);
    }
    void SetGraficColor(Color colorIcon, Color colorBG)
    {
        background.color = colorBG * defaultColors[0];
        icon.color = colorIcon * defaultColors[1];
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