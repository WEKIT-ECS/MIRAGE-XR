using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace TiltBrush {
  public class FocusDataProvider : MonoBehaviour {

    public static FocusDataProvider Instance;

    public bool hasPointerFocus => 
    CoreServices.FocusProvider.PrimaryPointer != null
    && CoreServices.FocusProvider.PrimaryPointer.Result != null
    && CoreServices.FocusProvider.PrimaryPointer.Result.CurrentPointerTarget != null;

    private void Awake() {
      Instance = this;
    }

    private void OnDestroy() {
      Instance = null;
    }

    public Ray GetMainPointerRay() {  
      //If no poke pointer, return the primary pointer
      var primaryPointer = CoreServices.InputSystem.FocusProvider.PrimaryPointer;
      if(primaryPointer != null && primaryPointer.Rays.Length > 0)
        return primaryPointer.Rays[0];

      //If not using MRTK or no pointer is found, return the tiltbrush AttachPoint
      var rAttachPoint = InputManager.m_Instance.GetBrushControllerAttachPoint();
      return new Ray(rAttachPoint.position, rAttachPoint.forward);
    }
  }
}