using UnityEngine;
using UnityEditor;
using System;
public class ColorChainReference : MonoBehaviour
{
    [SerializeField] Color color = new(255,255,255,1);
    Color oldColor;
    public event Action<Color> colorChanged;
    void Awake()
    {
        oldColor = color;
    }
    void Update()
    {
        if (color != oldColor)  // for updates in animator
        {
            colorChanged?.Invoke(color);
            oldColor = color;
        }
    }
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
