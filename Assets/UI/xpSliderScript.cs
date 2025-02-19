using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Sluzi pre postupne zobrazovanie posuvnika skusenosti
/// </summary>
public class XpSliderScript : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Slider slider;
    [SerializeField] bool xpBar;
    /// <summary>
    /// Nastavuje a Ziskava hodnotu posuvnika <br />
    /// Zaroven pri nastaveni nastavi ci ma hodnota rast alebo klesat
    /// </summary>
    /// <value>HODNOTA posuvnika</value>
    public float SliderValue 
    { 
        get { return sliderValue; } 
        set 
        { 
            sliderValue = value; 
            valueChangedUP = slider.value < sliderValue;
            valueChangedDOWN = slider.value > sliderValue;
        } 
    }
    float sliderValue;
    Queue<byte> levelUPs;
    Queue<float> maxValueQ;
    bool valueChangedUP = false, valueChangedDOWN = false;
    /// <summary>
    /// Zisti ci je dostupna dalsia uroven
    /// </summary>
    bool QueuedLevelUP => 1 < maxValueQ.Count;
    /// <summary>
    /// Sluzi pre rovnomerne posuvanie hodnoty posuvnika bez ohladu na velkost
    /// </summary>
    /// <returns></returns>
    float Update => (maxValueQ.Peek() - slider.minValue) / 150f;

    bool awoken = false;

    /// <summary>
    /// Volane pri spusteni
    /// </summary>
    void Awake()
    {
        levelUPs = new();
        maxValueQ = new();
        sliderValue = 0f;
        slider.minValue = 0f;
        valueChangedUP= false;
        valueChangedDOWN= false;
        slider.value = sliderValue;

        awoken = true;
    }
    public void Load(uint xp, byte level)
    {
        Awake();
        
        // Zaciatok je koncom predchadzjucej urovne
        // Prva uroven potrebuje len 50 xp
        int minimalXP = 1 <= level ? 50 : 0;

        // A pre kazdy ziskany level sa prida 100 xp
        if (0 < level)
            minimalXP += (level-1)*100; 
        
        slider.minValue = minimalXP;
        sliderValue = xp;
        slider.value = sliderValue;

        // Zada 2 levely do slidera
        for (int i = 0; i < 2; i++, level++)
            LevelUP(level);
    }
    /// <summary>
    /// Volany v rovnakych intervaloch
    /// </summary>
    void FixedUpdate()
    {
        // Zacne menit hodnotu posuvnika k novej urovni
        if      (valueChangedUP)
        {
            // ak je pridana uz nova uroven a posuvnik ju dosiahol zmeni rozsahy posuvnika
            if (QueuedLevelUP && maxValueQ.Peek() <= slider.value)
            {
                slider.minValue = maxValueQ.Dequeue();
                slider.maxValue = maxValueQ.Peek();
                game.LevelUP(levelUPs.Dequeue());
            }

            if  (slider.value < SliderValue)
                slider.value += Update;
            else
                valueChangedUP = false;
        }   
        else if (valueChangedDOWN)
        {
            if (slider.value > SliderValue)
                slider.value -= Update;
            else
                valueChangedDOWN = false;
        }
        //FileManager.Log($"Update \nUP[{valueChangedUP}] \nDOWN[{valueChangedDOWN}] \nmaxValueQ[{maxValueQ.Count}]\nSlider [{SliderValue}] <{slider.minValue}, {slider.maxValue}>");
    }
    /// <summary>
    /// Prida novu uroven za predchazajucu
    /// </summary>
    /// <param name="level"></param>
    /// <param name="nMax"></param>
    public void LevelUP(byte level)
    {
        if (!awoken)
            Awake();

        // Koniec je o uroven vyssie ako aktualny level
        // Cize level x 100 a 1. level ma hodnotu 50
        int maxXP = level*100 + 50; 
        if (maxValueQ.Count < 1)
            slider.maxValue = maxXP;

        maxValueQ.Enqueue(maxXP);
        levelUPs.Enqueue(level);

        FileManager.Log("Level UP queued to " + level, FileLogType.RECORD);
    }
}
