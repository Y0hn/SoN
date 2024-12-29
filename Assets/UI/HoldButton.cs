using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : Button
{
    public Action onEnterHold;
    public Action onExitHold;
    public bool isHolding;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        //Debug.Log("Down");
        if (!isHolding)
            StartHold();
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        //Debug.Log("Up");
        if (isHolding)
            EndHold();
    }
    protected void StartHold()
    {
        onEnterHold?.Invoke();
        isHolding = true;
    }
    protected void EndHold()
    {
        onExitHold?.Invoke();
        isHolding = false;
    }
}