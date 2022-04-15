using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TiltBrush;
using UnityEngine;

namespace TiltBrush
{
  public class TrackedHandControllerGeometry : ControllerGeometry
  {
    void Start(){
      var hand = GetComponentInParent<BaseControllerBehavior>().ControllerName;
      var followPinch = PointerAttachAnchor.gameObject.AddComponent<FollowPinch>();
      followPinch.handedness = hand == InputManager.ControllerName.Brush ? Handedness.Right : Handedness.Left;

      var followPinch2 = ToolAttachAnchor.gameObject.AddComponent<FollowPinch>();
      followPinch2.handedness = hand == InputManager.ControllerName.Brush ? Handedness.Right : Handedness.Left;

      // var hand = GetComponentInParent<BaseControllerBehavior>().ControllerName;
      // var solver = PointerAttachAnchor.gameObject.AddComponent<SolverHandler>();
      // solver.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.HandJoint;
      // solver.TrackedHandness = hand == InputManager.ControllerName.Brush 
      //   ? Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right 
      //   : Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;

      // solver.TrackedHandJoint = Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.ThumbTip;

      // var handConstraint = PointerAttachAnchor.gameObject.AddComponent<HandConstraint>();
      // handConstraint.RotationBehavior = HandConstraint.SolverRotationBehavior.None;
      // handConstraint.OffsetBehavior = HandConstraint.SolverOffsetBehavior.TrackedObjectRotation;
      // handConstraint.Smoothing = false;
      // handConstraint.MoveLerpTime = 2;
      // handConstraint.RotateLerpTime = 2;
    }
  }
}
