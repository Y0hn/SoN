using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class ColorChain : MonoBehaviour
{
    [SerializeField] ColorChainReference reference;
    void Awake()
    {
        if (reference == null) return;
        reference.colorChanged += SetColor;
        SetColor();
    }
    void OnDrawGizmos()
    {
        if (reference != null)
            SetColor();
    }
    void SetColor()
    {
        SetColor(reference.Color);
    }
    void SetColor(Color color)
    {
        /*if      (TryGetComponent(out Button btn))
        {
            var v = btn.colors;
            v.normalColor = reference.Color;
            btn.colors = v;
        }
        else */if (TryGetComponent(out SpriteRenderer spr))
        {
            spr.color = color;
        }
        else if (TryGetComponent(out Image img))
        {
            img.color = color;
        }
        else if (TryGetComponent(out RawImage rmg))
        {
            rmg.color = color;
        }
        else if (TryGetComponent(out TMP_Text txt))
        {
            txt.color = color;
        }
        // Debug.Log("Setted to: " + color);
    }
}
