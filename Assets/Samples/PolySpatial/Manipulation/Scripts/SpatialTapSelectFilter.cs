using System.Collections;
using System.Collections.Generic;
using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
#if UNITY_INCLUDE_XRI

using UnityEngine.XR.Interaction.Toolkit.Filtering;
#endif
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

#if UNITY_INCLUDE_XRI
public class SpatialTapSelectFilter : MonoBehaviour, IXRSelectFilter
{

    public bool Process(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor, UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
    {

        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count > 0)
        {
            var primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouches[0]);

            return primaryTouchData.Kind == SpatialPointerKind.IndirectPinch || primaryTouchData.Kind == SpatialPointerKind.DirectPinch;
        }

        return false;
    }

    public bool canProcess => isActiveAndEnabled;
}
#endif
