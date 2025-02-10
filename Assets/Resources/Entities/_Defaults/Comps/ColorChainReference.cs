using UnityEngine;
using UnityEditor;
using System;
/// <summary>
/// Sluzi ako pociatocna farba pre retazec
/// </summary>
public class ColorChainReference : MonoBehaviour
{
    [SerializeField] Color color = new(255,255,255,1);
    Color oldColor;
    public event Action<Color> colorChanged;
    void Awake()
    {
        oldColor = color;
    }
    /// <summary>
    /// Kontroluje zmenu farby
    /// </summary>
    void Update()
    {
        if (color != oldColor)  // for updates in animator
        {
            colorChanged?.Invoke(color);
            oldColor = color;
        }
    }
    /// <summary>
    /// Nastavuje fagbu podla vstupu
    /// </summary>
    /// <param name="color">vstupna FARBA</param>
    public void SetColor(Color color)
    {
        this.color = color;
        oldColor = color;
        colorChanged?.Invoke(color);
    }
    public virtual Color Color
    {
        get { return color; }
        set { color = value; }
    }
}
