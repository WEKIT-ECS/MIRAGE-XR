using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class FollowPinch : MonoBehaviour, IMixedRealityHandJointHandler
{
  public Handedness handedness;

  public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
  {
    if (eventData.Handedness != handedness){
      return;
    }
    
    if(!eventData.InputData.ContainsKey(TrackedHandJoint.IndexTip) || !eventData.InputData.ContainsKey(TrackedHandJoint.ThumbTip))
      return;
      
    var indexTip = eventData.InputData[TrackedHandJoint.IndexTip];
    var thumbTip = eventData.InputData[TrackedHandJoint.ThumbTip];

    transform.position = new Vector3((indexTip.Position.x + thumbTip.Position.x) / 2f, (indexTip.Position.y + thumbTip.Position.y) / 2f, (indexTip.Position.z + thumbTip.Position.z) / 2);
  }

  void Start()
  {
    CoreServices.InputSystem.RegisterHandler<IMixedRealityHandJointHandler>(this);
  }

  void OnDestroy()
  {
    if(CoreServices.InputSystem != null)
      CoreServices.InputSystem.UnregisterHandler<IMixedRealityHandJointHandler>(this);
  }
}
