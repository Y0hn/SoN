using UnityEngine.UI;
using UnityEngine;
using TMPro;
/// <summary>
/// Urcene pre jednoduchu zmenu farby pre celok podla jednej referencnej farby
/// </summary>
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
    /// <summary>
    /// Sluzi ako prostriedok pre volanie nastavenia farby 
    /// </summary>
    void SetColor()
    {
        SetColor(reference.Color);
    }
    /// <summary>
    /// Postupne prechadza komponenty a ked najde nastavi jeho farbu podla vstup
    /// </summary>
    /// <param name="color">vstupna FARBA</param>
    void SetColor(Color color)
    {

        if (TryGetComponent(out SpriteRenderer spr))
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
        //FileManager.Log($"{name} color setted to: " + color);
    }
}
