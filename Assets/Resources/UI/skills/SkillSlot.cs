using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
/// <summary>
/// Drzi, povoluje a aktivuje schopnost
/// </summary>
public class SkillSlot : MonoBehaviour
{
    /// <summary>
    /// Zistuje ci je zakupena schopnost
    /// </summary>
    /// <value>PRAVDA ak je uz ziskana</value>
    public bool isPurchased { get; private set; }

    [SerializeField] SkillCreator skillCreator;
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Image value;
    [SerializeField] Image moddifier;
    [SerializeField] Image background;
    [SerializeField] TMP_Text amountT;
    [SerializeField] List<SkillSlot> dependentcySkills = new();
    [SerializeField] bool needsAllDependecies = false;
    bool started = false;
    
    /// <summary>
    /// Urcuje zmenu farby podla zmneny stavu pre konkretne casti grafiky schopnosti
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Ziska schopnosti jej vytvorenim pomocou vytvaraca schopnosti  
    /// </summary>
    public Skill Skill => skillCreator.Skill;
    private GameManager game;
    /// <summary>
    /// Kontroluje ci boli ziskane potrebne predchadzajuce schopnosti
    /// </summary>
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
        if (started) return;
        started = true;

        skillCreator.name = name;
        defaultColors = new Color[4];
        button.onClick.AddListener(ActivateSkill);
        ResetGrafic();
        SetGraficColor(pallete["unavailableIc"], pallete["unavailableBG"], pallete["unavailableIc"]);
        SetInteractable(false);
        game = GameManager.instance;
        game.SkillTree.OnChangeAvailablePoints += PurchableSkill;

    }
    /// <summary>
    /// Nastavi grafiku do vychodiskovej podoby
    /// </summary>
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
    /// Prida schopnost do stromu schopnosti hraca
    /// </summary>
    public void ActivateSkill()
    {
        game ??= GameManager.instance;
        game.LocalPlayer.AddSkill(Skill);
    }
    /// <summary>
    /// Povoluje zavisle schopnosti
    /// </summary>
    public void BuySkill()
    {
        amountT.text = "";
        isPurchased = true;
        moddifier.enabled = false;
        value.enabled = false;
        SetInteractable(false);
        game.SkillTree.SkillPointAplied();
        SetGraficColor(pallete["aquired"]);
    }
    /// <summary>
    /// Nastavi schopnost (ne)kupitenu za bod <br />
    /// Zalezi na tom ci bola spnena podmienka zavislosti predchadzajucich schopnosti
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
    /// <summary>
    /// Nastavi ci sa da schopnost ziskat (ci sa da na nu kilkinut)
    /// </summary>
    /// <param name="interactable"></param>
    void SetInteractable(bool interactable)
    {
        amountT.text = interactable ? skillCreator.Amount : "";
        button.interactable = interactable;
        background.raycastTarget = interactable;
        background.raycastTarget = interactable;
    }
    /// <summary>
    /// Sluzi ako skrateny zapis <br />
    /// Nastavi zhodnu farbu vsetkym 
    /// </summary>
    /// <param name="color"></param>
    void SetGraficColor(Color color)
    {
        SetGraficColor(color, color, color);
    }
    /// <summary>
    /// Nastavi farby pre rozne casti grafiky
    /// </summary>
    /// <param name="colorIcon"></param>
    /// <param name="colorBG"></param>
    /// <param name="colorMod"></param>
    void SetGraficColor(Color colorIcon, Color colorBG, Color colorMod)
    {
        if (!started) Start();
        background.color = colorBG * defaultColors[0];
        moddifier.color = colorMod * defaultColors[2];
        icon.color = colorIcon * defaultColors[1];
    }
    void OnDrawGizmos()
    {
        //skillCreator = new(name);
    }

    /// <summary>
    /// Pouzivane na vytvaranie schopnosti a nastavovanie konkretnych prarametrov podla hodnot v editore
    /// </summary>
    [Serializable] public class SkillCreator
    {
        [HideInInspector] public string name;
        [SerializeField] Utility.Function function;
        [SerializeField] Damage.Type condition;
        [SerializeField] bool isAttack;
        [SerializeField] bool speed;
        [SerializeField] float amount;

        /// <summary>
        /// Vytvori nanovo celeho tvrocu schopnosti
        /// </summary>
        /// <param name="n">meno objektu, potrebne pre identifikaciu</param>
        public SkillCreator(string n)
        {
            name = n;

            function = Utility.Function.None;
            condition = Damage.Type.None;
            isAttack = false;
            speed = false;
            amount = 0;
        }
        /// <summary>
        /// Vrati schopnost typu podla vyplnenych parametrov
        /// </summary>
        /// <value>SCHOPNOST</value>
        public Skill Skill
        {
            get
            {
                if (function != Utility.Function.None)
                    return new Utility(name, function, true);
                else if (condition == Damage.Type.None)
                    return new ModSkill(name, amount, speed);
                else
                    return new ModDamage(name, amount, condition, speed, isAttack);
            }
        }
        /// <summary>
        /// Vrati mnozstvo spolu s pridavnymi znakmi podla parametrov 
        /// </summary>
        /// <value>TEXT mnozstva so znakami</value>
        public string Amount
        {
            get 
            {
                string a = "";
                
                if (function == Utility.Function.None)
                {
                    ModSkill ms = (ModSkill)Skill;
                    a += ms.amount;
                    
                    if (ms.isPercentyl)
                        a += " %";
                    else
                        a = "+ " + a;
                }

                return a;
            }
        }
    }
}