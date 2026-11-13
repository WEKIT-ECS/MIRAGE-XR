using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace JetXR.VisionUI
{
    public class Grabber : MonoBehaviour, IDragHandler
    {
        [SerializeField] private Transform windowTransform;
        [SerializeField] private float translateSpeed = 2f;
        [SerializeField] private float rotateSpeed = 50f;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
        private float rayDistance;
        private float windowAngleOnSelect;
        private float additiveRotation;
        private bool xrEnabled;
        private ActionBasedController actionBasedController;

        private bool isDirectTouch;
        private Vector3 initialInteractorPosition;
        private Vector3 initialWindowPosition;
        private Vector3 touchOffsetFromHand;
        private Vector3 grabOffsetLocal;

        private void Awake()
        {
            if (windowTransform == null)
            {
                windowTransform = transform.parent != null ? transform.parent : transform;
            }
        }

        private void OnEnable()
        {
            xrEnabled = XRSettings.enabled;

            if (!xrEnabled)
                return;

            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnSelectEntered);
                interactable.selectExited.AddListener(OnSelectExited);
            }
        }

        private void OnDisable()
        {
            if (!xrEnabled || interactable == null)
                return;

            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void TranslateAction_performed(InputAction.CallbackContext obj)
        {
            var value = obj.action.ReadValue<Vector2>();
            rayDistance += value.y * Time.deltaTime * translateSpeed;
            rayDistance = Mathf.Max(0.2f, rayDistance);
        }

        private void RotateAction_performed(InputAction.CallbackContext obj)
        {
            var value = obj.action.ReadValue<Vector2>();
            additiveRotation += value.x * Time.deltaTime * rotateSpeed;
        }

        public void OnSelectEntered(SelectEnterEventArgs eventData)
        {
            if (!xrEnabled || windowTransform == null)
                return;

            interactor = eventData.interactorObject?.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
            if (interactor == null)
                return;

            var baseInputInteractor = interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
            actionBasedController = baseInputInteractor != null ? baseInputInteractor.xrController as ActionBasedController : null;

            if (actionBasedController != null)
            {
                actionBasedController.translateAnchorAction.action.performed += TranslateAction_performed;
                actionBasedController.rotateAnchorAction.action.performed += RotateAction_performed;
            }

            additiveRotation = 0;
            isDirectTouch = CheckIsDirectTouch(interactor);

            if (isDirectTouch)
            {
                initialInteractorPosition = interactor.transform.position;
                initialWindowPosition = windowTransform.position;
                touchOffsetFromHand = windowTransform.position - interactor.transform.position;
            }
            else
            {
                Vector3 contactPoint = GetInteractionPoint(interactor);
                grabOffsetLocal = windowTransform.InverseTransformVector(windowTransform.position - contactPoint);

                Vector3 rayOrigin = interactor.transform.position;
                rayDistance = Vector3.Distance(rayOrigin, contactPoint);
                if (rayDistance < 0.1f)
                {
                    rayDistance = Vector3.Distance(rayOrigin, windowTransform.position);
                }

                Vector3 rayDirHorizontal = contactPoint - rayOrigin;
                rayDirHorizontal.y = 0;
                if (rayDirHorizontal.sqrMagnitude > 0.001f)
                {
                    windowAngleOnSelect = windowTransform.rotation.eulerAngles.y - Quaternion.LookRotation(rayDirHorizontal).eulerAngles.y;
                }
                else
                {
                    windowAngleOnSelect = 0;
                }
            }
        }

        public void OnSelectExited(SelectExitEventArgs eventData)
        {
            if (actionBasedController != null)
            {
                actionBasedController.translateAnchorAction.action.performed -= TranslateAction_performed;
                actionBasedController.rotateAnchorAction.action.performed -= RotateAction_performed;
                actionBasedController = null;
            }

            interactor = null;
        }

        private void Update()
        {
            if (!xrEnabled || interactable == null || !interactable.isSelected || interactor == null || windowTransform == null)
                return;

            if (isDirectTouch)
            {
                Vector3 interactorDelta = interactor.transform.position - initialInteractorPosition;
                windowTransform.position = initialWindowPosition + interactorDelta;
            }
            else
            {
                Vector3 rayOrigin = interactor.transform.position;
                Vector3 rayForward = interactor.transform.forward;

                Vector3 targetRayPoint = rayOrigin + rayForward * rayDistance;

                Vector3 toTargetHorizontal = targetRayPoint - rayOrigin;
                toTargetHorizontal.y = 0;

                Quaternion targetRotation = windowTransform.rotation;
                if (toTargetHorizontal.sqrMagnitude > 0.001f)
                {
                    targetRotation = Quaternion.LookRotation(toTargetHorizontal) * Quaternion.Euler(0, windowAngleOnSelect + additiveRotation, 0);
                    windowTransform.rotation = targetRotation;
                }

                windowTransform.position = targetRayPoint + targetRotation * grabOffsetLocal;
            }
        }

        private bool CheckIsDirectTouch(XRBaseInteractor targetInteractor)
        {
            if (targetInteractor is XRDirectInteractor)
                return true;

            if (targetInteractor is XRPokeInteractor)
                return true;

            if (targetInteractor is NearFarInteractor nearFar &&
                nearFar.selectionRegion.Value == NearFarInteractor.Region.Near)
                return true;

            float distToGrabber = Vector3.Distance(targetInteractor.transform.position, transform.position);
            return distToGrabber < 0.25f;
        }

        private Vector3 GetInteractionPoint(XRBaseInteractor targetInteractor)
        {
            if (targetInteractor is XRRayInteractor rayInteractor && rayInteractor.TryGetCurrent3DRaycastHit(out var hit))
            {
                return hit.point;
            }

            if (targetInteractor is IXRRayProvider rayProvider)
            {
                var endPoint = rayProvider.rayEndPoint;
                if (endPoint != Vector3.zero)
                {
                    return endPoint;
                }
            }

            Ray ray = new Ray(targetInteractor.transform.position, targetInteractor.transform.forward);
            Plane plane = new Plane(windowTransform.forward, transform.position);
            if (plane.Raycast(ray, out float enter) && enter > 0.01f && enter < 50f)
            {
                return ray.GetPoint(enter);
            }

            return transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (xrEnabled || windowTransform == null)
                return;

            windowTransform.localPosition += (Vector3)eventData.delta;
        }

        public void SetReferences(Transform windowTransform)
        {
            this.windowTransform = windowTransform;
        }
    }
}
