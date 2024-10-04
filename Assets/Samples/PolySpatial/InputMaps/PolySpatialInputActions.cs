//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.8.1
//     from Assets/Samples/PolySpatial/InputMaps/PolySpatialInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

public partial class @PolySpatialInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PolySpatialInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PolySpatialInputActions"",
    ""maps"": [
        {
            ""name"": ""Touch"",
            ""id"": ""2dbea638-e09d-420b-a7ef-e4343b622ecb"",
            ""actions"": [
                {
                    ""name"": ""PrimaryTouch"",
                    ""type"": ""PassThrough"",
                    ""id"": ""8e5af3df-c5f8-4a1e-b7d2-cf8465df9849"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Tap"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""PrimaryTouchPhase"",
                    ""type"": ""Value"",
                    ""id"": ""8266884d-aa39-4b83-9830-3a4973efe355"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SecondaryWorldTouch"",
                    ""type"": ""PassThrough"",
                    ""id"": ""24123604-9580-4671-9bb7-5d0ec0c949f9"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SecondaryWorldTouchPhase"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a778d40f-2ad1-4fe5-8cfb-091ada00625b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""74c3f666-c665-47f5-b043-4050eb5ec865"",
                    ""path"": ""<SpatialPointerDevice>/primarySpatialPointer"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""662f407b-9b9f-4544-8200-6179984169ad"",
                    ""path"": ""<OpenXRHandTracking>{RightHand}/trackingState"",
                    ""interactions"": ""Press(behavior=2),Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""067e925b-b5a4-4b2a-9627-3c9157459842"",
                    ""path"": ""<OpenXRHandTracking>/trackingState"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""46e2aa62-d6a8-4c60-85ec-aab4c1b87c94"",
                    ""path"": ""<SpatialPointerDevice>/primarySpatialPointer/phase"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryTouchPhase"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0e375c3e-07f5-416e-8b94-58215655bbbc"",
                    ""path"": ""<SpatialPointerDevice>/spatialPointer1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryWorldTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2e11b458-5c32-40de-9271-e22e7b43d67b"",
                    ""path"": ""<SpatialPointerDevice>/spatialPointer1/phase"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryWorldTouchPhase"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""XRHMD"",
            ""id"": ""e3f3d8dc-6dc0-4d1e-8f2e-82d31170ffa0"",
            ""actions"": [
                {
                    ""name"": ""DevicePosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""70607691-0df9-44ae-8259-710c37de4a43"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DeviceRotation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""2d13d7ad-83e7-420a-aa3d-12f49acaa812"",
                    ""expectedControlType"": ""Quaternion"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""00932a8c-cd37-4f33-abb1-2d4afb22dcda"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0d6427d7-8ec4-4722-af22-80204f05c36f"",
                    ""path"": ""<PolySpatialXRHMD>/devicePosition"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DevicePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e830475f-8161-40b2-90ab-1dc682b02018"",
                    ""path"": ""<PolySpatialXRHMD>/deviceRotation"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeviceRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dd0e4433-350e-476d-9a66-1b349bc0207f"",
                    ""path"": ""<OpenXRHandTracking>/{PrimaryTrigger}"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Touch
        m_Touch = asset.FindActionMap("Touch", throwIfNotFound: true);
        m_Touch_PrimaryTouch = m_Touch.FindAction("PrimaryTouch", throwIfNotFound: true);
        m_Touch_PrimaryTouchPhase = m_Touch.FindAction("PrimaryTouchPhase", throwIfNotFound: true);
        m_Touch_SecondaryWorldTouch = m_Touch.FindAction("SecondaryWorldTouch", throwIfNotFound: true);
        m_Touch_SecondaryWorldTouchPhase = m_Touch.FindAction("SecondaryWorldTouchPhase", throwIfNotFound: true);
        // XRHMD
        m_XRHMD = asset.FindActionMap("XRHMD", throwIfNotFound: true);
        m_XRHMD_DevicePosition = m_XRHMD.FindAction("DevicePosition", throwIfNotFound: true);
        m_XRHMD_DeviceRotation = m_XRHMD.FindAction("DeviceRotation", throwIfNotFound: true);
        m_XRHMD_Click = m_XRHMD.FindAction("Click", throwIfNotFound: true);
    }

    ~@PolySpatialInputActions()
    {
        Debug.Assert(!m_Touch.enabled, "This will cause a leak and performance issues, PolySpatialInputActions.Touch.Disable() has not been called.");
        Debug.Assert(!m_XRHMD.enabled, "This will cause a leak and performance issues, PolySpatialInputActions.XRHMD.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Touch
    private readonly InputActionMap m_Touch;
    private List<ITouchActions> m_TouchActionsCallbackInterfaces = new List<ITouchActions>();
    private readonly InputAction m_Touch_PrimaryTouch;
    private readonly InputAction m_Touch_PrimaryTouchPhase;
    private readonly InputAction m_Touch_SecondaryWorldTouch;
    private readonly InputAction m_Touch_SecondaryWorldTouchPhase;
    public struct TouchActions
    {
        private @PolySpatialInputActions m_Wrapper;
        public TouchActions(@PolySpatialInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @PrimaryTouch => m_Wrapper.m_Touch_PrimaryTouch;
        public InputAction @PrimaryTouchPhase => m_Wrapper.m_Touch_PrimaryTouchPhase;
        public InputAction @SecondaryWorldTouch => m_Wrapper.m_Touch_SecondaryWorldTouch;
        public InputAction @SecondaryWorldTouchPhase => m_Wrapper.m_Touch_SecondaryWorldTouchPhase;
        public InputActionMap Get() { return m_Wrapper.m_Touch; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TouchActions set) { return set.Get(); }
        public void AddCallbacks(ITouchActions instance)
        {
            if (instance == null || m_Wrapper.m_TouchActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_TouchActionsCallbackInterfaces.Add(instance);
            @PrimaryTouch.started += instance.OnPrimaryTouch;
            @PrimaryTouch.performed += instance.OnPrimaryTouch;
            @PrimaryTouch.canceled += instance.OnPrimaryTouch;
            @PrimaryTouchPhase.started += instance.OnPrimaryTouchPhase;
            @PrimaryTouchPhase.performed += instance.OnPrimaryTouchPhase;
            @PrimaryTouchPhase.canceled += instance.OnPrimaryTouchPhase;
            @SecondaryWorldTouch.started += instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouch.performed += instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouch.canceled += instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouchPhase.started += instance.OnSecondaryWorldTouchPhase;
            @SecondaryWorldTouchPhase.performed += instance.OnSecondaryWorldTouchPhase;
            @SecondaryWorldTouchPhase.canceled += instance.OnSecondaryWorldTouchPhase;
        }

        private void UnregisterCallbacks(ITouchActions instance)
        {
            @PrimaryTouch.started -= instance.OnPrimaryTouch;
            @PrimaryTouch.performed -= instance.OnPrimaryTouch;
            @PrimaryTouch.canceled -= instance.OnPrimaryTouch;
            @PrimaryTouchPhase.started -= instance.OnPrimaryTouchPhase;
            @PrimaryTouchPhase.performed -= instance.OnPrimaryTouchPhase;
            @PrimaryTouchPhase.canceled -= instance.OnPrimaryTouchPhase;
            @SecondaryWorldTouch.started -= instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouch.performed -= instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouch.canceled -= instance.OnSecondaryWorldTouch;
            @SecondaryWorldTouchPhase.started -= instance.OnSecondaryWorldTouchPhase;
            @SecondaryWorldTouchPhase.performed -= instance.OnSecondaryWorldTouchPhase;
            @SecondaryWorldTouchPhase.canceled -= instance.OnSecondaryWorldTouchPhase;
        }

        public void RemoveCallbacks(ITouchActions instance)
        {
            if (m_Wrapper.m_TouchActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ITouchActions instance)
        {
            foreach (var item in m_Wrapper.m_TouchActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_TouchActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public TouchActions @Touch => new TouchActions(this);

    // XRHMD
    private readonly InputActionMap m_XRHMD;
    private List<IXRHMDActions> m_XRHMDActionsCallbackInterfaces = new List<IXRHMDActions>();
    private readonly InputAction m_XRHMD_DevicePosition;
    private readonly InputAction m_XRHMD_DeviceRotation;
    private readonly InputAction m_XRHMD_Click;
    public struct XRHMDActions
    {
        private @PolySpatialInputActions m_Wrapper;
        public XRHMDActions(@PolySpatialInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @DevicePosition => m_Wrapper.m_XRHMD_DevicePosition;
        public InputAction @DeviceRotation => m_Wrapper.m_XRHMD_DeviceRotation;
        public InputAction @Click => m_Wrapper.m_XRHMD_Click;
        public InputActionMap Get() { return m_Wrapper.m_XRHMD; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(XRHMDActions set) { return set.Get(); }
        public void AddCallbacks(IXRHMDActions instance)
        {
            if (instance == null || m_Wrapper.m_XRHMDActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_XRHMDActionsCallbackInterfaces.Add(instance);
            @DevicePosition.started += instance.OnDevicePosition;
            @DevicePosition.performed += instance.OnDevicePosition;
            @DevicePosition.canceled += instance.OnDevicePosition;
            @DeviceRotation.started += instance.OnDeviceRotation;
            @DeviceRotation.performed += instance.OnDeviceRotation;
            @DeviceRotation.canceled += instance.OnDeviceRotation;
            @Click.started += instance.OnClick;
            @Click.performed += instance.OnClick;
            @Click.canceled += instance.OnClick;
        }

        private void UnregisterCallbacks(IXRHMDActions instance)
        {
            @DevicePosition.started -= instance.OnDevicePosition;
            @DevicePosition.performed -= instance.OnDevicePosition;
            @DevicePosition.canceled -= instance.OnDevicePosition;
            @DeviceRotation.started -= instance.OnDeviceRotation;
            @DeviceRotation.performed -= instance.OnDeviceRotation;
            @DeviceRotation.canceled -= instance.OnDeviceRotation;
            @Click.started -= instance.OnClick;
            @Click.performed -= instance.OnClick;
            @Click.canceled -= instance.OnClick;
        }

        public void RemoveCallbacks(IXRHMDActions instance)
        {
            if (m_Wrapper.m_XRHMDActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IXRHMDActions instance)
        {
            foreach (var item in m_Wrapper.m_XRHMDActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_XRHMDActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public XRHMDActions @XRHMD => new XRHMDActions(this);
    public interface ITouchActions
    {
        void OnPrimaryTouch(InputAction.CallbackContext context);
        void OnPrimaryTouchPhase(InputAction.CallbackContext context);
        void OnSecondaryWorldTouch(InputAction.CallbackContext context);
        void OnSecondaryWorldTouchPhase(InputAction.CallbackContext context);
    }
    public interface IXRHMDActions
    {
        void OnDevicePosition(InputAction.CallbackContext context);
        void OnDeviceRotation(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
    }
}
