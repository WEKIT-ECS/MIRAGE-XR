using Microsoft.MixedReality.Toolkit.Input;

public class AnimatedCursorWithCheck : AnimatedCursor
{
    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (eventData?.InputSource?.Pointers != null)
        {
            base.OnPointerUp(eventData);
        }
    }
}
