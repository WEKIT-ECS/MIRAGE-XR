using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace TiltBrush
{
  public class BrushSizeSlider : MonoBehaviour
  {
    public PinchSlider slider;

    void Awake()
    {
      slider.SliderValue = PointerManager.m_Instance.GetPointerBrushSize01(InputManager.ControllerName.Brush);
      slider.OnValueUpdated.AddListener(SetBrushSize);
    }

    public void SetBrushSize(SliderEventData data){
      PointerManager.m_Instance.SetAllPointersBrushSize01(data.NewValue);
    }
  }

}