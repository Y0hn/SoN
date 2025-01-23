using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XpSliderScript : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Slider slider;
    [SerializeField] bool xpBar;
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
    bool QueuedLevelUP => 1 < maxValueQ.Count;
    float Update => (maxValueQ.Peek() - slider.minValue) / 150f ;
#pragma warning disable IDE0051 // Remove unused private members
    void Start()
    {
        sliderValue = 0f;
        slider.minValue = 0f;
        valueChangedUP= false;
        valueChangedDOWN= false;
        slider.value = sliderValue;
    }
    void FixedUpdate()
    {
        if      (valueChangedUP)
        {
            if  (slider.value < SliderValue)
            {
                slider.value += Update;
            }
            else
                valueChangedUP = false;

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
            {
                slider.value -= Update;
            }
            else
                valueChangedDOWN = false;
        }
        //Debug.Log($"Update \nUP[{valueChangedUP}] \nDOWN[{valueChangedDOWN}] \nmaxValueQ[{maxValueQ.Count}]\nSlider [{SliderValue}] <{slider.minValue}, {slider.maxValue}>");
    }
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
