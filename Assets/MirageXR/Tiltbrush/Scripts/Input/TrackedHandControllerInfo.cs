using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using TiltBrush;
using UnityEngine;

public class TrackedHandControllerInfo : ControllerInfo {
  public override bool IsTrackedObjectValid { get => true; set => throw new System.NotImplementedException(); }

  private BaseControllerBehavior behavior;
  private HandState handState;

  public TrackedHandControllerInfo(BaseControllerBehavior behavior)
  : base(behavior) {
    this.behavior = behavior;
  }

  public override float GetGripValue() {
    return 0;
  }

  public override Vector2 GetPadValue() {
    return Vector2.zero;
  }

  public override Vector2 GetPadValueDelta() {
    return Vector2.zero;
  }

  public override float GetScrollXDelta() {
    return 0;
  }

  public override float GetScrollYDelta() {
    return 0;
  }

  public override Vector2 GetThumbStickValue() {
    return Vector2.zero;
  }

  private Handedness GetHandedness(){
    if(behavior.ControllerName == InputManager.ControllerName.Wand)
      return InputManager.m_Instance.WandOnRight ? Handedness.Right : Handedness.Left;
    else if(behavior.ControllerName == InputManager.ControllerName.Brush)
      return InputManager.m_Instance.WandOnRight ? Handedness.Left : Handedness.Right;
    else
      return Handedness.None;
  }

  public override float GetTriggerRatio() {
    var isPinching =  MRTKHandTrackingManager.GetHandState(GetHandedness()).isPinching;
    return isPinching ? 1f : 0f;
  }

  public override float GetTriggerValue() {
    var isPinching = MRTKHandTrackingManager.GetHandState(GetHandedness()).isPinching;
    return isPinching ? 1f : 0f;
  }

  public override bool GetVrInput(VrInput input) {
    switch (input) {
      case VrInput.Trigger:
        return MRTKHandTrackingManager.GetHandState(GetHandedness()).isPinching;
      case VrInput.Grip:
        return MRTKHandTrackingManager.GetHandState(GetHandedness()).isGripping;
      default:
        return false;
    }
  }

  public override bool GetVrInputDown(VrInput input) {
    return false;
  }

  public override bool GetVrInputTouch(VrInput input) {
    return false;
  }

  public override void TriggerControllerHaptics(float seconds) {
  }
}
