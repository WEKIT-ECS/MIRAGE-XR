using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

[RequireComponent(typeof(HandConstraintPalmUp))]
public class DisableOnPalmDown : MonoBehaviour
{
  public GameObject ToDisable;
  private HandConstraintPalmUp constraint;

  void Awake(){
    Disable();
    
    constraint = GetComponent<HandConstraintPalmUp>();
    constraint.OnHandDeactivate.AddListener(Disable);
    constraint.OnHandActivate.AddListener(Enable);
  }

  void Disable(){
    ToDisable.SetActive(false);
  }

  void Enable(){
    ToDisable.SetActive(true);
  }
}
