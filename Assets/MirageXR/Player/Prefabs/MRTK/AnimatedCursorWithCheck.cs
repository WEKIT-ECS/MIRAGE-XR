using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class AnimatedCursorWithCheck : AnimatedCursor
{
    protected override void Start()
    {
        Debug.LogWarning("-----AnimatedCursorWithCheck.Start");
        base.Start();
    }

    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        Debug.Log("-----AnimatedCursorWithCheck.OnPointerUp");
        if (eventData?.InputSource?.Pointers != null)
        {
            base.OnPointerUp(eventData);
        }
    }
}
