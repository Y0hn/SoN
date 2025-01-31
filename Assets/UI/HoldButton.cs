using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Rozsiruje tlacitko (Unity Button) o moznost drzania
/// </summary>
public class HoldButton : Button
{
    public Action onEnterHold;
    public Action onExitHold;
    public bool isHolding;

    /// <summary>
    /// Zavolane po kliknuti mysi
    /// </summary>
    /// <param name="eventData">inforamacie z EventSystemu v Scene</param>
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        //Debug.Log("Down");
        if (!isHolding)
            StartHold();
    }
    /// <summary>
    /// Zavolane po pusteni tlacitka mysi
    /// </summary>
    /// <param name="eventData">inforamacie z EventSystemu v Scene</param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        //Debug.Log("Up");
        if (isHolding)
            EndHold();
    }

    /// <summary>
    /// Zacne drzanie
    /// a zavola Akciu na zacatie drzania
    /// </summary>
    protected void StartHold()
    {
        onEnterHold?.Invoke();
        isHolding = true;
    }
    /// <summary>
    /// Sonci drzanie
    /// a zavola Akciu na ukoncenie drzania
    /// </summary>
    protected void EndHold()
    {
        onExitHold?.Invoke();
        isHolding = false;
    }
}