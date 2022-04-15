using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TiltBrush
{
  //This component fills the gap between Unity's Button and MRTK's overcomplicated Interactable.
  //Allows to set a sound for nearInteraction button presses because otherwise there is no audio feedback at all
  //Also allows to set a button in the "selected" state which prevents focus events to remove the selected color
  public class MrtkSimpleBtn : Selectable, IMixedRealityTouchHandler, IMixedRealityInputHandler
  {
    public UnityEvent onClick;

    public AudioClip pressSound;
    public AudioClip unpressSound;
    private AudioSource source;

    private bool isDisabled;
    private bool isSelected;
    private bool isFocused;
    private bool pressedThis;
    
    private float timeout = 0.3f;
    private float disabledFor = 0.0f;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
      //Ignore state transitions when selected
      if(isSelected)
        return;

      base.DoStateTransition(state, instant);
    }

    private void ForceDoStateTransition(SelectionState state, bool instant){
      base.DoStateTransition(state, instant);
    }

    protected override void Awake()
    {
      base.Awake();

      //Awake is called even when not playing by Selectable
      if(Application.isPlaying)
        source = gameObject.EnsureComponent<AudioSource>();
    }

    public void SetSelected(bool selected){
      isSelected = selected;
      if(selected)
        ForceDoStateTransition(SelectionState.Selected, true);
      else
        ForceDoStateTransition(SelectionState.Normal, true);
    }

    public void SetDisabled(bool disabled){
      interactable = !disabled;
    }

    void Update(){
      if(disabledFor > 0){
        disabledFor -= Time.deltaTime;
      }
    }

    public void OnInputDown(InputEventData eventData)
    {
      if(disabledFor > 0 || !interactable)
        return;
        
      pressedThis = true;
    }

    public void OnInputUp(InputEventData eventData)
    {
      if (pressedThis)
        onClick?.Invoke();

      pressedThis = false;
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
      if(disabledFor > 0 || !interactable)
        return;

      disabledFor = timeout;
      pressedThis = true;
      if(pressSound)
        source.PlayOneShot(pressSound);
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData)
    {
      if (pressedThis)
      {
        onClick?.Invoke();
        if(unpressSound)
          source.PlayOneShot(unpressSound);
      }
      disabledFor = timeout;
      pressedThis = false;
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData)
    {
      //Nothing
    }
  }

}