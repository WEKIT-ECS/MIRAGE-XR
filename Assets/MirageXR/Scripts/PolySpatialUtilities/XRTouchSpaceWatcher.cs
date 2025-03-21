using UnityEngine;
using UnityEngine.InputSystem;

#if POLYSPATIAL_SDK_AVAILABLE && VISION_OS
using System.Reflection;
using Unity.PolySpatial.XR.Input;
using UnityEngine.EventSystems;
#endif

namespace MirageXR.MirageXR
{
    public class XRTouchSpaceWatcher : MonoBehaviour
    {
        [SerializeField] private InputActionReference inputActionReference;

#if POLYSPATIAL_SDK_AVAILABLE && VISION_OS
        private const string M_SPATIALPOINTER = "m_SpatialPointer";

        private void Awake()
        {
            AddXRTouchSpaceToAllUIElements(transform);
            Hacks.InstantiateExtensions.OnInstantiated.AddListener(OnInstantiate);
        }

        private void OnInstantiate(Object obj)
        {
            if (obj is not GameObject go)
            {
                return;
            }

            AddXRTouchSpaceToAllUIElements(go.transform);
        }

        private void AddXRTouchSpaceToAllUIElements(Transform changedTransform)
        {
            foreach (var clickHandler in changedTransform.GetComponentsInChildren<IPointerClickHandler>(true))
            {
                if (clickHandler is MonoBehaviour monoBehaviour && 
                    monoBehaviour.GetComponent<XRSpatialPointerInteractor>() == null)
                {
                    var interactor = monoBehaviour.gameObject.AddComponent<XRSpatialPointerInteractor>();
                    SetPrivateField(interactor, M_SPATIALPOINTER, inputActionReference);
                }
            }
        }

        private static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogError($"Field '{fieldName}' not found in {obj.GetType()}");
            }
        }
#endif
    }
}