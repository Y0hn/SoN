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
    Queue<byte> levelUPs = new();
    Queue<float> maxValueQ = new();
    bool valueChangedUP = false, valueChangedDOWN = false;
    /// <summary>
    /// Zisti ci je dostupna dalsia uroven
    /// </summary>
    bool QueuedLevelUP => 1 < maxValueQ.Count;
    /// <summary>
    /// Sluzi pre rovnomerne posuvanie hodnoty posuvnika bez ohladu na velkost
    /// </summary>
    /// <returns></returns>
    float Update => (maxValueQ.Peek() - slider.minValue) / 150f ;
#pragma warning disable IDE0051 // Remove unused private members
    
    /// <summary>
    /// Volane pri spusteni
    /// </summary>
    void Start()
    {
        sliderValue = 0f;
        slider.minValue = 0f;
        valueChangedUP= false;
        valueChangedDOWN= false;
        slider.value = sliderValue;
    }
    /// <summary>
    /// Volany v rovnakych intervaloch
    /// </summary>
    void FixedUpdate()
    {
        // Zacne menit hodnotu posuvnika k novej urovni
        if      (valueChangedUP)
        {
            if  (slider.value < SliderValue)
                slider.value += Update;
            else
                valueChangedUP = false;

            // ak je pridana uz nova uroven a posuvnik ju dosiahol zmeni rozsahy posuvnika
            if (QueuedLevelUP && maxValueQ.Peek() <= slider.value)
            {
                slider.minValue = maxValueQ.Dequeue();
                slider.maxValue = maxValueQ.Peek();
                game.LevelUP(levelUPs.Dequeue());
            }
        }   
        else if (valueChangedDOWN)
        {
            if (slider.value > SliderValue)
                slider.value -= Update;
            else
                valueChangedDOWN = false;
        }
        //Debug.Log($"Update \nUP[{valueChangedUP}] \nDOWN[{valueChangedDOWN}] \nmaxValueQ[{maxValueQ.Count}]\nSlider [{SliderValue}] <{slider.minValue}, {slider.maxValue}>");
    }
    /// <summary>
    /// Prida novu uroven za predchazajucu
    /// </summary>
    /// <param name="level"></param>
    /// <param name="nMax"></param>
    public void LevelUP(byte level, float nMax)
    {
        if (maxValueQ.Count < 1)
            slider.maxValue = nMax;

        maxValueQ.Enqueue(nMax);
        level++;
        levelUPs.Enqueue(level);

        //Debug.Log("Level UP queued to " + level);
    }
#pragma warning restore IDE0051 // Remove unused private members
}
