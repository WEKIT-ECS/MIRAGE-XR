using Microsoft.MixedReality.Toolkit.Input;

public class MobileAnimatedCursor : AnimatedCursor
{
    public override void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (eventData.InputSource != null)
        {
            base.OnPointerUp(eventData);
        }
    }
}
