using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEngine;

namespace MirageXR
{
    public class TapRecognizerService : IService, IMixedRealityPointerHandler
    {
        public event EventHandler<TapEventArgs> TapRecognized;
        public event EventHandler<TapEventArgs> DoubleTapRecognized;

        public float doubleTapThreshold = 0.8f;

        private float lastTapTime = -1f;

        public void Initialize(IServiceManager owner)
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
        }

        public void Cleanup()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        { }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        { }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        { }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            if (eventData.MixedRealityInputAction.Description == "Select")
            {
                TapEventArgs eventArgs = new TapEventArgs(
                    eventData.Pointer.Result.CurrentPointerTarget,
                    eventData.Pointer.Result.Details.Point);
                TapRecognized?.Invoke(this, eventArgs);

                if (lastTapTime > 0 && Time.time - lastTapTime < doubleTapThreshold)
                {
                    DoubleTapRecognized?.Invoke(this, eventArgs);
                    // do not recognize three subsequent clicks as two double clicks
                    lastTapTime = -1;
                }
                else
                {
                    lastTapTime = Time.time;
                }
            }
        }
    }

    public class TapEventArgs : EventArgs
    {
        public GameObject SelectedObject { get; }

        public Vector3 HitPoint { get; }

        public TapEventArgs(GameObject selectedObject, Vector3 hitPoint)
        {
            this.SelectedObject = selectedObject;
            HitPoint = hitPoint;
        }
    }
}
