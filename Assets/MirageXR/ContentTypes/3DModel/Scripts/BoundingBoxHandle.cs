using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirageXR
{
    public class BoundingBoxHandle : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum HandleType
        {
            Scale,
            Rotate
        }

        public HandleType Type;
        public Vector3 Axis; // For non-uniform scale or rotation axis
        public Action<BoundingBoxHandle, Vector3> OnDragHandle;
        public Action<BoundingBoxHandle> OnDownHandle;
        public Action<BoundingBoxHandle> OnUpHandle;

        private Vector3 _lastScreenPosition;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            if (_cam == null) _cam = FindObjectOfType<Camera>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _lastScreenPosition = eventData.position;
            OnDownHandle?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Simple screen space drag delta for now, passing to controller to interpret
            // Or better: calculate world movement?
            // Let's pass the raw event delta or handle calculation in controller.
            
            // Getting world delta from screen delta
            if (_cam == null) return;
            
            // Current depth of handle
            float depth = _cam.WorldToScreenPoint(transform.position).z;
            
            Vector3 curScreenPoint = new Vector3(eventData.position.x, eventData.position.y, depth);
            Vector3 lastScreenPoint = new Vector3(eventData.position.x - eventData.delta.x, eventData.position.y - eventData.delta.y, depth);

            Vector3 worldPos = _cam.ScreenToWorldPoint(curScreenPoint);
            Vector3 lastWorldPos = _cam.ScreenToWorldPoint(lastScreenPoint);
            
            Vector3 worldDelta = worldPos - lastWorldPos;

            OnDragHandle?.Invoke(this, worldDelta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUpHandle?.Invoke(this);
        }
    }
}
