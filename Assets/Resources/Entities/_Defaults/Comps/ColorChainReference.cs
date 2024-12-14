using UnityEngine;
using UnityEditor;
using System;
public class ColorChainReference : MonoBehaviour
{
    [SerializeField] Color color = new(255,255,255,1);
    public Color GetColor { get => color; }
    public event Action<Color> colorChanged;
    public void SetColor(Color color)
    {
        this.color = color;
        InvokeChange();
    }
    public void InvokeChange()
    {
        //colorChanged.Invoke(color);
    }
    public virtual Color Color
    {
        get { return color; }
        set { color = value; }
    }
}
[CustomEditor(typeof(ColorChainReference))]
public class MyButtonExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Referencia na cieľový objekt
        ColorChainReference example = (ColorChainReference)target;

        // Tlačidlo v inspektore
        if (GUILayout.Button("Refresh color"))
        {
            example.InvokeChange();
        }
    }
}
