using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <inheritdoc/> prepinaca
/// </summary>
public class MainUIToggle : MainUIButton
{
    [SerializeField] Toggle toggle;

    protected override void OnClick()
    {
        base.OnClick();
        toggle.isOn= !toggle.isOn;
    }
}