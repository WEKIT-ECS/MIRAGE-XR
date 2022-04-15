using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace TiltBrush {

  public class HandState {
    public Handedness handedness;
    public bool isPinching;
    public bool isGripping;
    public bool isHolding;
    public float pinchValue;

    public HandState(Handedness handedness) {
      this.handedness = handedness;
      isHolding = false;
      pinchValue = 0;
    }
  }

  public class MRTKHandTrackingManager : MonoBehaviour,
  IMixedRealitySourceStateHandler // Handle source detected and lost
  {
    static public MRTKHandTrackingManager m_Instance;
    static public event Action NewPosesApplied;

    static public event Action<SourceStateEventData> SourceDetected;
    static public event Action<SourceStateEventData> SourceLost;
    static public Dictionary<Handedness, HandState> handStates;

    [Range(0, 1)]
    public float pinchDetectedThreshold = 0.7f;
    [Range(0, 1)]
    public float pinchLostThreshold = 0.3f;

    private void Awake() {
      handStates = new Dictionary<Handedness, HandState>();
      handStates.Add(Handedness.Left, new HandState(Handedness.Left));
      handStates.Add(Handedness.Right, new HandState(Handedness.Right));
      m_Instance = this;
    }

    private void Start() {
      CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
    }

    private void OnDestroy() {
      CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
      NewPosesApplied = null;
      SourceDetected = null;
      SourceLost = null;
      m_Instance = null;
      handStates.Clear();
    }

    private void Update() {
      UpdatePinchState(Handedness.Left);
      //UpdateGripState(Handedness.Left);

      UpdatePinchState(Handedness.Right);
      //UpdateGripState(Handedness.Right);
    }

    private void UpdatePinchState(Handedness handedness) {
      //We don't want to allow drawing if an object is focused
      if(handedness == Handedness.Right && FocusDataProvider.Instance)
            {
        if(!handStates[handedness].isPinching && FocusDataProvider.Instance.hasPointerFocus)
          return;
      }

      var pinchValue = HandPoseUtilsCustom.CalculateIndexPinch(handedness);
      var handState = handStates[handedness];
      if (handState.isPinching && pinchValue < pinchLostThreshold)
        handState.isPinching = false;
      else if (!handState.isPinching && pinchValue > pinchDetectedThreshold)
        handState.isPinching = true;
    }

    //Grip disabled because we want to disable world grabbing as it causes bugs
    private void UpdateGripState(Handedness handedness) {
      handStates[handedness].isGripping = HandPoseUtilsCustom.IsIndexGrabbing(handedness)
      && HandPoseUtilsCustom.IsThumbGrabbing(handedness)
      && HandPoseUtilsCustom.IsMiddleGrabbing(handedness);
    }

    public static HandState GetHandState(Handedness handedness) {
      if (!handStates.ContainsKey(handedness))
        return null;

      return handStates[handedness];
    }

    public void OnSourceDetected(SourceStateEventData eventData) {
      SourceDetected?.Invoke(eventData);
    }

    public void OnSourceLost(SourceStateEventData eventData) {
      SourceLost?.Invoke(eventData);
    }
  }

}