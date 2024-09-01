using UnityEngine;

public class ColorChainReference : MonoBehaviour
{
    [SerializeField] Color color = new(255,255,255,1);
    
    public virtual Color Color
    {
        get { return color; }
        set { color = value; }
    }
}
