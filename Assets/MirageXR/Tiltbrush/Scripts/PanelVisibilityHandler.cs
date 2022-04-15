using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace TiltBrush {
  public class PanelVisibilityHandler : MonoBehaviour {
    
    public HandConstraintPalmUp wandFollow;
    private bool handActive;

    public void Start(){
      wandFollow.OnHandDeactivate.AddListener(DisablePanels);
      wandFollow.OnHandActivate.AddListener(EnablePanels);
      SketchControlsScript.m_Instance.PanelsVisibilityChangeRequested += ShouldPanelsStayHidden;
    }

    //If PanelVisible requested, we check if hand allows it
    private void ShouldPanelsStayHidden(bool visible){
      if(visible && !handActive){
        SketchControlsScript.m_Instance.RequestPanelsVisibility(false);
      }
    }

    private void EnablePanels(){
      handActive = true;
      SketchControlsScript.m_Instance.RequestPanelsVisibility(true);
    }

    private void DisablePanels(){
      handActive = false;
      SketchControlsScript.m_Instance.RequestPanelsVisibility(false);
    }
  }
}
