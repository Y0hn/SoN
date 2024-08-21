using UnityEngine;

public class charakterPallet : MonoBehaviour
{
    public Color color;
    [SerializeField]
    SpriteRenderer[] parts;
    void Awake()
    {
        foreach (var part in parts)
        {
            part.color = color;
        }
    }
}
