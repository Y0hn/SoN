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
    private Queue<float> changes;
    private byte lastLevel;
    private float Update => 0.01f;

    /// <summary>
    /// Volane pri spusteni
    /// </summary>
    void Awake()
    {
        GameManager.GameQuit += SetUp;
        SetUp();
    }
    void SetUp()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        lastLevel = 0;
        changes = new();
        slider.value = 0f;
    }
    public void Load(float xp, byte level)
    {
        SetUp();
        slider.value = xp;
        lastLevel = level;
    }
    /// <summary>
    /// Volany v rovnakych intervaloch
    /// </summary>
    void FixedUpdate()
    {
        if (0 < changes.Count)
        {
            if (slider.value < changes.Peek())
            {
                slider.value += Update;
                //FileManager.Log($"Closing in on <{slider.value}-{changes.Peek()}>");
            }
            else if (changes.Dequeue() == 1)
            {
                game.LevelUP();
                slider.value = 0f;
            }
        }
    }
    /// <summary>
    /// Prida novu uroven za predchazajucu
    /// </summary>
    /// <param name="level"></param>
    /// <param name="nMax"></param>
    public void QueueChange(float xp, byte level)
    {
        // Tolko krat prejde celym barom, kolko levelov preslo
        for (int i = lastLevel; i < level; i++)
            changes.Enqueue(1);

        if (0 < xp && xp < 1)
            changes.Enqueue(xp);

        FileManager.Log($"", FileLogType.RECORD);
        lastLevel = level;
    }
}
