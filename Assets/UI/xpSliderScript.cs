using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XpSliderScript : MonoBehaviour
{
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
    Queue<float> maxValueQ = new();
    bool valueChangedUP = false, valueChangedDOWN = false;
    float Update => (maxValueQ.Peek() - slider.minValue) / 150f ;
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

            if (1 < maxValueQ.Count && maxValueQ.Peek() <= slider.value)
            {
                slider.minValue = maxValueQ.Dequeue();
                slider.maxValue = maxValueQ.Peek();
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
        Debug.Log($"Update \nUP[{valueChangedUP}] \nDOWN[{valueChangedDOWN}] \nmaxValueQ[{maxValueQ.Count}]\nSlider [{SliderValue}] <{slider.minValue}, {slider.maxValue}>");
    }
    public void AddMax(float newMax)
    {
        if (maxValueQ.Count < 1)
            slider.maxValue = newMax;
        maxValueQ.Enqueue(newMax);
    }
}
