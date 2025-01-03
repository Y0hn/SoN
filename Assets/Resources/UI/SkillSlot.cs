using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.Playables;

public class SkillSlot : MonoBehaviour
{
    public bool isPurchased { get; private set; }

    [SerializeField] SkillCreator skillCreator;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Image moddifier;
    [SerializeField] Image background;
    [SerializeField] List<SkillSlot> dependentcySkills;
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
        get =>
            (!needsAllDependecies && (dependentcySkills.Find(dS => dS.isPurchased) != null)) 
                || 
            (needsAllDependecies && (dependentcySkills.FindAll(dS => dS.isPurchased).Count == dependentcySkills.Count))
                || 
            dependentcySkills.Count == 0 ;  // zacitocny skill
    }
    void Start()
    {
        defaultColors = new Color[3];
        button.onClick.AddListener(ActivateSkill);
        game = GameManager.instance;
        if (game != null)
            game.SkillTree.OnChangeAvailablePoints += PurchableSkill;
        ResetGrafic();
        SetGraficColor(pallete["unavailableIc"], pallete["unavailableBG"], pallete["unavailableIc"]);
        SetInteractable(false);
    }
    void OnDrawGizmos()
    {
        /*skillCreator.name = name;
        if (skillCreator.skillType == SkillCreator.SkillType.Utility) return;
        string[] s = FileManager.GetSkillRefferency(skillCreator.skillType);
        icon.sprite = Resources.Load<Sprite>(s[0]);
        if (s.Length > 1 && skillCreator.skillType != SkillCreator.SkillType.Utility)
        {
            moddifier.sprite = Resources.Load<Sprite>(s[1]);
            moddifier.enabled = true;
        }
        else
            moddifier.enabled = false;*/
    }
    void ResetGrafic()
    {
        icon.enabled = true;
        background.enabled = true;
        defaultColors[0] = background.color;
        defaultColors[1] = icon.color;
        defaultColors[2] = moddifier.color;

        if (skillCreator.skillType != SkillCreator.SkillType.Utility)
        {
            string[] s = FileManager.GetSkillRefferency(skillCreator.skillType);
            icon.sprite = Resources.Load<Sprite>(s[0]);
            if (s.Length > 1)
            {
                moddifier.sprite = Resources.Load<Sprite>(s[1]);
                moddifier.enabled = true;
            }
        }       
    }
    /// <summary>
    /// Adds skill to Player Skill Tree and Enables Dependent skills
    /// </summary>
    void ActivateSkill()
    {
        isPurchased = true;
        SetInteractable(false);

        game.LocalPlayer.AddSkillRpc(Skill);
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
    public void LoadSkill()
    {
        ActivateSkill();
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