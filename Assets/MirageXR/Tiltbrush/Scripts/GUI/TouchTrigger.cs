using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace TiltBrush {
  public class TouchTrigger : MonoBehaviour, IMixedRealityTouchHandler {
    
    private BaseButton button;
    
    //in seconds
    public float delayBetweenTouch = 1.5f;
    
    private float currentDelay = -1f;
    private bool pressed = false;

    private void Start(){
      button = GetComponentInParent<BaseButton>();
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData) {
      if(pressed){
        pressed = false;
        button.ButtonReleased();
      }
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData) {
      if(currentDelay <= 0){
        button.ButtonPressed(default(RaycastHit));
        currentDelay = delayBetweenTouch;
        pressed = true;
      }
    }

    private void Update(){
      if(currentDelay > 0){
        currentDelay -= Time.deltaTime;
      }
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData) {
      //
    }
  }

}