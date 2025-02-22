using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
/// <summary>
/// Zistuje ci je mys nad objektom
/// </summary>
public class HoverButton : Button
{
    public Action CursorExit;
    public Action CursorEnter;
    public bool CursorInside => cursorOverButton;

    private bool cursorOverButton;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        cursorOverButton = true;
        CursorEnter?.Invoke();
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        cursorOverButton = true;
        CursorExit?.Invoke();
    }
}