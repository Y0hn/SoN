using UnityEngine;

/// <summary>
/// Upravuje referencnu farbu vlastnou
/// </summary>
public class ColorChainModifier : ColorChainReference
{
    [SerializeField] ColorChainReference colorRef;
    public override Color Color 
    { 
        get => ColorMixer(colorRef.Color, base.Color);

        set => base.Color = value; 
    }
    private Color ColorMixer(Color color1, Color color2)
    {
        float r = color1.r * color2.r;
        float g = color1.g * color2.g;
        float b = color1.b * color2.b;
        float a = color1.a * color2.a;

        return new Color(r, g, b, a);
    }
}
