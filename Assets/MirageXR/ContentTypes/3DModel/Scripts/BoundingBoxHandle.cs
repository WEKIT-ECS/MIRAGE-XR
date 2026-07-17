using System;
using Unity.PolySpatial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MirageXR
{
    [RequireComponent(typeof(VisionOSHoverEffect))]
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

        private Camera _cam;
        private XRSimpleInteractable _xrInteractable;
        private Vector3 _lastXRAttachPosition;
        private bool _pointerPressed;
        private bool _xrSelected;

        private bool IsInteracting => _pointerPressed || _xrSelected;

        private void Awake()
        {
            _xrInteractable = GetComponent<XRSimpleInteractable>();
            if (_xrInteractable == null)
            {
                _xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
            }
            _xrInteractable.selectMode = InteractableSelectMode.Single;
            _xrInteractable.selectEntered.AddListener(OnXRSelectEntered);
            _xrInteractable.selectExited.AddListener(OnXRSelectExited);
        }

        private void Start()
        {
            _cam = Camera.main;
            if (_cam == null) _cam = FindObjectOfType<Camera>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            bool wasInteracting = IsInteracting;
            _pointerPressed = true;
            if (!wasInteracting)
            {
                OnDownHandle?.Invoke(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_xrSelected || _cam == null) return;
            
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
            _pointerPressed = false;
            EndInteractionIfNeeded();
        }

        private void LateUpdate()
        {
            if (!_xrSelected || _xrInteractable == null || _xrInteractable.interactorsSelecting.Count == 0)
            {
                return;
            }

            var interactor = _xrInteractable.interactorsSelecting[0];
            Vector3 currentPosition = interactor.GetAttachTransform(_xrInteractable).position;
            Vector3 worldDelta = currentPosition - _lastXRAttachPosition;
            _lastXRAttachPosition = currentPosition;
            if (worldDelta.sqrMagnitude > Mathf.Epsilon)
            {
                OnDragHandle?.Invoke(this, worldDelta);
            }
        }

        private void OnXRSelectEntered(SelectEnterEventArgs args)
        {
            bool wasInteracting = IsInteracting;
            _xrSelected = true;
            _lastXRAttachPosition = args.interactorObject.GetAttachTransform(_xrInteractable).position;
            if (!wasInteracting)
            {
                OnDownHandle?.Invoke(this);
            }
        }

        private void OnXRSelectExited(SelectExitEventArgs args)
        {
            _xrSelected = false;
            EndInteractionIfNeeded();
        }

        private void EndInteractionIfNeeded()
        {
            if (!IsInteracting)
            {
                OnUpHandle?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            bool wasInteracting = IsInteracting;
            _pointerPressed = false;
            _xrSelected = false;
            if (wasInteracting)
            {
                OnUpHandle?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            if (_xrInteractable == null) return;

            _xrInteractable.selectEntered.RemoveListener(OnXRSelectEntered);
            _xrInteractable.selectExited.RemoveListener(OnXRSelectExited);
        }
    }
}
