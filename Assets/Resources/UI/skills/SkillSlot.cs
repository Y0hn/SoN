using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.Playables;
using Unity.Android.Gradle;

public class SkillSlot : MonoBehaviour
{
    public bool isPurchased { get; private set; }

    [SerializeField] SkillCreator skillCreator;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Image value;
    [SerializeField] Image moddifier;
    [SerializeField] Image background;
    [SerializeField] List<SkillSlot> dependentcySkills = new();
    [SerializeField] bool needsAllDependecies = false;
    
    private static Dictionary<string, Color> pallete = new()
    {
        { "unavailableIc",  new (128f/255f, 128f/255f, 128f/255f, 0.5f ) },  // disabled //rgba(76, 76, 76, 0.5)
        { "unavailableBG",  new (  0f/255f,   0f/255f,   0f/255f, 1f   ) },  // disabled //rgba(76, 76, 76, 0)

        { "nextInLine",     new (255f/255f, 255f/255f, 255f/255f, 1f   ) },  // enabled  //rgba(204, 204, 204, 0.75)

        { "unafordableIc",  new (128f/255f, 128f/255f, 128f/255f, 0.75f) },  // disabled //rgba(128, 128, 128, 0.75)
        { "unafordableBG",  new (192f/255f, 192f/255f, 192f/255f, 1f   ) },  // disabled //rgba(128, 128, 128, 0.75)

        { "aquired",        new ( 89f/255f,  89f/255f,  89f/255f, 1f   ) },  // disabled //rgba(89, 89, 89, 1)
    };    
    private Color[] defaultColors;
    public SkillTree.Skill Skill => skillCreator.Skill;
    private GameManager game;

    private bool DependenciesFullfiled 
    {
        get
        {
            if (dependentcySkills != null)
                return
                    dependentcySkills == null || dependentcySkills.Count == 0  // zacitocny skill
                        || 
                    (!needsAllDependecies && (dependentcySkills.Find(dS => dS.isPurchased) != null)) 
                        || 
                    (needsAllDependecies && (dependentcySkills.FindAll(dS => dS.isPurchased).Count == dependentcySkills.Count));
            else
                return false;
        }
    }
    void Start()
    {
        defaultColors = new Color[4];
        button.onClick.AddListener(ActivateSkill);
        ResetGrafic();
        SetGraficColor(pallete["unavailableIc"], pallete["unavailableBG"], pallete["unavailableIc"]);
        SetInteractable(false);
        //game = await GameManager.GetGameManager();
        game = GameManager.instance;
        game.SkillTree.OnChangeAvailablePoints += PurchableSkill;
    }
    void ResetGrafic()
    {
        icon.enabled = true;
        moddifier.enabled = true;
        background.enabled = true;
        defaultColors[0] = background.color;
        defaultColors[1] = icon.color;
        defaultColors[2] = moddifier.color;
        defaultColors[3] = value.color;
    }
    /// <summary>
    /// Adds skill to Player Skill Tree and Enables Dependent skills
    /// </summary>
    public void ActivateSkill()
    {
        isPurchased = true;
        moddifier.enabled = false;
        value.enabled = false;
        SetInteractable(false);

        game.LocalPlayer.AddSkillRpc(Skill);
        game.SkillTree.SkillPointAplied();
        SetGraficColor(pallete["aquired"]);
        TryActivateUtility();
    }
    void TryActivateUtility(bool active = true)
    {
        if (Skill is SkillTree.Utility u)
            game.EnableUtility(new (u.function, active));
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
            SetInteractable(enable);
            if (enable)
                SetGraficColor(pallete["nextInLine"]);
            else
                SetGraficColor(pallete["unafordableIc"], pallete["unafordableBG"], pallete["unafordableIc"]);
        }
    }
    void SetInteractable(bool interactable)
    {
        icon.raycastTarget = interactable;
        button.interactable = interactable;
        background.raycastTarget = interactable;
    }
    void SetGraficColor(Color color)
    {
        SetGraficColor(color, color, color);
    }
    void SetGraficColor(Color colorIcon, Color colorBG, Color colorMod)
    {
        background.color = colorBG * defaultColors[0];
        moddifier.color = colorMod * defaultColors[2];
        icon.color = colorIcon * defaultColors[1];
    }
    [Serializable] public class SkillCreator
    {
        public SkillType skillType;
        public string name;
        [SerializeField] float amount;
        [SerializeField] Damage.Type condition;
        [SerializeField] SkillTree.Utility.Function utilFunction;
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
                    case SkillType.AttackDamage:
                    case SkillType.AttackRate:
                        skill = new SkillTree.ModAttack(name, amount, condition, skillType == SkillType.AttackRate);
                        break;
                    case SkillType.Protection:
                        float am = -amount;
                        skill = new SkillTree.Combat(name, am, condition);
                        break;
                    case SkillType.Utility:
                    default: 
                        skill = new SkillTree.Utility(name, utilFunction);
                        break;
                }
                return skill;
            }
        }
        public enum SkillType
        {
            Utility, Health, Protection, AttackDamage, AttackRate, MovementSpeed
        }
    }
}