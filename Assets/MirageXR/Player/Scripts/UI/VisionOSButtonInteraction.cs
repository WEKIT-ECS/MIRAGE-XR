using System;
using UnityEngine;
using UnityEngine.Events;
using PolySpatial.Samples;

namespace MirageXR
{
    public class VisionOSButtonInteraction : SpatialUI
    {
        public UnityEvent onClick;

        public override void Press(Vector3 position)
        {
            Debug.Log("Button pressed");
            onClick?.Invoke();
        }

    }
}
