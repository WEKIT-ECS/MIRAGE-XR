using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Flag enabled pointer kind enum to indicate which pointer kinds are supported by this input reader.
    /// </summary>
    [Flags]
    public enum SupportedPointerKind
    {
        None = 0,
        Touch = 1,
        IndirectPinch = 2,
        DirectPinch = 4,
    }
    
    /// <summary>
    /// Input reader that binds to visionOS touch input, and implements an input queue to ensure only one input state change occurs per frame.
    /// </summary>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_XRInputDeviceButtonReader)]
    public class SpatialTouchInputReader : MonoBehaviour, IXRInputButtonReader
    {
        [SerializeField]
        [Tooltip("Bind to primary or secondary touch input.")]
        bool m_IsPrimaryTouch = true;

        /// <summary>
        /// Bind to primary or secondary touch input.
        /// </summary>
        public bool isPrimaryTouch
        {
            get => m_IsPrimaryTouch;
            set => m_IsPrimaryTouch = value;
        }

        [SerializeField]
        [Tooltip("Time in seconds to wait to auto-release if no new touch event arrives. Can be important in volume mode since some events get cancelled incorrectly. Set to 0 to disable.")]
        float m_ReleaseTimeOutDelay = 0.1f;

        /// <summary>
        /// Time in seconds to wait to auto-release if no new touch event arrives.
        /// Can be important in volume mode since some events get cancelled incorrectly.
        /// Set to 0 to disable.
        /// </summary>
        public float releaseTimeOutDelay
        {
            get => m_ReleaseTimeOutDelay;
            set => m_ReleaseTimeOutDelay = value;
        }

        [SerializeField]
        [Tooltip("Supported pointer kinds.")]
        SupportedPointerKind m_SupportedPointerKind = SupportedPointerKind.DirectPinch | SupportedPointerKind.IndirectPinch;

        /// <summary>
        /// which pointer kinds are supported by this input reader.
        /// </summary>
        public SupportedPointerKind supportedPointerKind
        {
            get => m_SupportedPointerKind;
            set => m_SupportedPointerKind = value;
        }

        /// <summary>
        /// True if a touch event is currently active and input was not ended or cancelled.
        /// </summary>
        public IReadOnlyBindableVariable<bool> hasActiveTouch => m_HasActiveTouch;
        
        readonly BindableVariable<bool> m_HasActiveTouch = new BindableVariable<bool>();

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        bool m_IsPerformed;
        bool m_WasPerformedThisFrame;
        bool m_WasCompletedThisFrame;
#pragma warning restore CS0649

#if POLYSPATIAL_1_1_OR_NEWER
        readonly Vector3 m_HiddenPosition = new Vector3(0f, -100f, 0f);
        
        SpatialPointerInput m_SpatialPointerInput;
        bool m_InputEnabled;
        float m_TimeSinceLastTouch;

        SpatialPointerState m_LastSpatialPointerState;

        readonly Queue<SpatialPointerState> m_SpatialPointerStateQueue = new Queue<SpatialPointerState>();
#endif

        void OnEnable()
        {
#if POLYSPATIAL_1_1_OR_NEWER
            m_SpatialPointerInput ??= new SpatialPointerInput();
            if (m_SpatialPointerInput == null)
                return;
            
            m_SpatialPointerInput.Enable();

            if (m_IsPrimaryTouch)
            {
                m_SpatialPointerInput.Touch.PrimaryTouch.performed += OnTouchPerformed;
                m_SpatialPointerInput.Touch.PrimaryTouch.canceled += OnTouchPerformed;
            }
            else
            {
                m_SpatialPointerInput.Touch.SecondaryTouch.performed += OnTouchPerformed;
                m_SpatialPointerInput.Touch.SecondaryTouch.canceled += OnTouchPerformed;
            }
            m_InputEnabled = true;
#endif
        }
        
        void OnDisable()
        {
#if POLYSPATIAL_1_1_OR_NEWER
            m_SpatialPointerStateQueue.Clear();
            ExitInput();
            
            if (!m_InputEnabled)
                return;

            if (m_IsPrimaryTouch)
            {
                m_SpatialPointerInput.Touch.PrimaryTouch.performed -= OnTouchPerformed;
                m_SpatialPointerInput.Touch.PrimaryTouch.canceled -= OnTouchPerformed;
            }
            else
            {
                m_SpatialPointerInput.Touch.SecondaryTouch.performed -= OnTouchPerformed;
                m_SpatialPointerInput.Touch.SecondaryTouch.canceled -= OnTouchPerformed;
            }
                
            m_SpatialPointerInput.Disable();
            m_InputEnabled = false;
#endif
        }

        void Update()
        {
#if POLYSPATIAL_1_1_OR_NEWER
            bool hasTouch = m_SpatialPointerStateQueue.Count > 0;
            
            // Check if input has timed out
            if (!hasTouch && m_IsPerformed && m_ReleaseTimeOutDelay > 0)
            {
                m_TimeSinceLastTouch += Time.unscaledDeltaTime;
                if (m_TimeSinceLastTouch > m_ReleaseTimeOutDelay)
                    ExitInput();
            }
            
            while (m_SpatialPointerStateQueue.Count > 0)
            {
                var state = m_SpatialPointerStateQueue.Dequeue();
                if (!IsSpatialPointerKindSupported(state.Kind))
                {
                    if (m_IsPerformed)
                    {
                        ExitInput();
                        break;
                    }
                    continue;
                }
                
                m_TimeSinceLastTouch = 0f;
                bool isPhaseActive = state.phase.IsActive();
                
                // If phase is active and the input is already active, empty the queue with latest transform state
                if (m_IsPerformed && isPhaseActive)
                {
                    UpdateTransform(state);
                    continue;
                }
                // If phase does not match input state, update input state and transform, and wait a frame for more changes
                if (m_IsPerformed != isPhaseActive)
                {
                    UpdateIsPerformed(isPhaseActive);
                    if (isPhaseActive)
                        UpdateTransform(state);
                    else
                        ResetTransform();
                    break;
                }
            }
#endif
        }

#if POLYSPATIAL_1_1_OR_NEWER
        /// <summary>
        /// Tries to get the last valid pointer state.
        /// </summary>
        /// <returns>False if <see cref="hasActiveTouch"/> is false</returns>
        public bool TryGetPointerState(out SpatialPointerState pointerState)
        {
            pointerState = m_LastSpatialPointerState;
            return m_HasActiveTouch.Value;
        }

        void OnTouchPerformed(InputAction.CallbackContext context)
        {
            var device = context.ReadValue<SpatialPointerState>();
            m_SpatialPointerStateQueue.Enqueue(device);
        }
        
        void ExitInput()
        {
            UpdateIsPerformed(false);
            ResetTransform();
        }

        void ResetTransform()
        {
            transform.SetPositionAndRotation(m_HiddenPosition, Quaternion.identity);
            m_HasActiveTouch.Value = false;
        }
        
        void UpdateIsPerformed(bool inputActive)
        {
            bool wasPerformedLastFrame = m_IsPerformed;
            m_IsPerformed = inputActive;
            m_WasPerformedThisFrame = m_IsPerformed && !wasPerformedLastFrame;
            m_WasCompletedThisFrame = !m_IsPerformed && wasPerformedLastFrame;
        }

        void UpdateTransform(SpatialPointerState touchState)
        {
            // Update transform
            Vector3 targetPosition = touchState.Kind == SpatialPointerKind.Touch ? touchState.interactionPosition : touchState.inputDevicePosition;
            transform.SetPositionAndRotation(targetPosition, touchState.inputDeviceRotation);
            
            // Update last state
            m_LastSpatialPointerState = touchState;
            m_HasActiveTouch.Value = touchState.phase.IsActive();
        }

        static SupportedPointerKind MapPointerKind(SpatialPointerKind pointerKind)
        {
            return pointerKind switch
            {
                SpatialPointerKind.DirectPinch => SupportedPointerKind.DirectPinch,
                SpatialPointerKind.IndirectPinch => SupportedPointerKind.IndirectPinch,
                SpatialPointerKind.Touch => SupportedPointerKind.Touch,
                _ => SupportedPointerKind.None,
            };
        }

        bool IsSpatialPointerKindSupported(SpatialPointerKind pointerKind) => IsSupportedPointerKind(MapPointerKind(pointerKind));
        
        bool IsSupportedPointerKind(SupportedPointerKind pointerKind) => (m_SupportedPointerKind & pointerKind) != 0;
#endif

        public float ReadValue() => m_IsPerformed ? 1f : 0f;

        public bool TryReadValue(out float value)
        {
            value = ReadValue();
            return true;
        }

        public bool ReadIsPerformed() => m_IsPerformed;

        public bool ReadWasPerformedThisFrame() => m_WasPerformedThisFrame;

        public bool ReadWasCompletedThisFrame() => m_WasCompletedThisFrame;
    }
}