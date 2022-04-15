using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class MRTKSelectable : MonoBehaviour, IMixedRealityInputHandler, IMixedRealityTouchHandler
{
  public bool isSelected = false;

  public void OnTouchCompleted(HandTrackingInputEventData eventData)
  {
    isSelected = false;
  }

  public void OnTouchStarted(HandTrackingInputEventData eventData)
  {
    isSelected = true;
  }

  public void OnTouchUpdated(HandTrackingInputEventData eventData)
  {
    //Do nothing
  }

  public void OnInputUp(InputEventData eventData)
  {
    if(eventData.MixedRealityInputAction.Description == "Select")
      isSelected = false;
  }

  public void OnInputDown(InputEventData eventData)
  {
    if(eventData.MixedRealityInputAction.Description == "Select")
      isSelected = true;
  }
}
