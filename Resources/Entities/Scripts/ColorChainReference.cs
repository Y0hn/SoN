using UnityEngine;
using System;
public class ColorChainReference : MonoBehaviour
{
    [SerializeField] Color color = new(255,255,255,1);

    public event Action<Color> colorChanged;

    protected virtual void OnColorChanged()
    {
        if (colorChanged != null) colorChanged(Color);
        Debug.Log("Event called");
    }
    
    public virtual Color Color
    {
        get { return color; }
        set { color = value; }
    }
}
