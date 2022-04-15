// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace TiltBrush {
public class ControllerGeometry : MonoBehaviour {
  // -------------------------------------------------------------------------------------------- //
  // Inspector Data
  // -------------------------------------------------------------------------------------------- //
  [SerializeField] private ControllerStyle m_ControllerStyle = ControllerStyle.None;

  [SerializeField] private Transform m_PointerAttachAnchor;
  [SerializeField] private Transform m_PointerAttachPoint;
  [SerializeField] private Transform m_ToolAttachAnchor;
  [SerializeField] private Transform m_ToolAttachPoint;
  [SerializeField] private Transform m_PinCushionSpawn;
  [SerializeField] private Transform m_MainAxisAttachPoint;
  [SerializeField] private Transform m_CameraAttachPoint;
  [SerializeField] private Transform m_ConsoleAttachPoint;
  [SerializeField] private Transform m_BaseAttachPoint;
  [SerializeField] private Transform m_GripAttachPoint;
  [SerializeField] private Transform m_DropperDescAttachPoint;

  [SerializeField] private Renderer m_MainMesh;
  [SerializeField] private Renderer m_TriggerMesh;
  [SerializeField] private Renderer[] m_OtherMeshes;
  [SerializeField] private Renderer m_LeftGripMesh;
  [SerializeField] private Renderer m_RightGripMesh;
  [SerializeField] private Transform m_PadTouchLocator;
  [SerializeField] private Transform m_TriggerAnchor;
  [SerializeField] private Renderer m_TransformVisualsRenderer;
  [SerializeField] private GameObject m_ActivateEffectPrefab;
  [SerializeField] private GameObject m_HighlightEffectPrefab;
  [SerializeField] private GameObject m_XRayVisuals;

  [Header("Controller Animations")]
  [Tooltip("Range of rotation for TriggerAnchor, in degrees. Rotation is about the right axis")]
  [SerializeField] private Vector2 m_TriggerRotation;
  [SerializeField] private float m_TouchLocatorTranslateScale = 0.27f;
  [SerializeField] private float m_TouchLocatorTranslateClamp = 0.185f;
  [SerializeField] private Material m_GripReadyMaterial;
  [SerializeField] private Material m_GrippedMaterial;
  [SerializeField] private Vector3 m_LeftGripPopInVector;
  [SerializeField] private Vector3 m_LeftGripPopOutVector;


  [Header("Pad Controls")]
  // This number is difficult to tune if there is any offset between the anchor and its child,
  // because the scale amplifies that offset. The easiest thing to do is to have no offset
  // which means the height of the popup is purely specified by PadPopUpAmount.
  // Note that this value is currently used for both the thumbstick and touchpad popups.
  [SerializeField] float m_PadPopUpAmount;
  [SerializeField] float m_PadScaleAmount;
  [SerializeField] float m_PadSpeed;

  [Header("Haptics")]
  [SerializeField] float m_HapticPulseOn;
  [SerializeField] float m_HapticPulseOff;

  [Header("Vive Pad")]
  [SerializeField] private Transform m_PadAnchor;
  [SerializeField] private Renderer m_PadMesh;

  [Header("Oculus Touch Buttons")]
  [SerializeField] private Transform m_Joystick;
  [SerializeField] private Renderer m_JoystickMesh;
  [SerializeField] private Renderer m_JoystickPad;
  [SerializeField] private Renderer m_Button01Mesh;
  [SerializeField] private Renderer m_Button02Mesh;

  [Header("Wmr Button")]
  [SerializeField] private Renderer m_PinCushion;

  [Header("Brush objects")]
  [SerializeField] private LineRenderer m_GuideLine;
  [SerializeField] private GameObject m_SelectionHintButton;
  [SerializeField] private GameObject m_DeselectionHintButton;

  // -------------------------------------------------------------------------------------------- //
  // Public Properties
  // -------------------------------------------------------------------------------------------- //
  public Vector2 TriggerRotation { get { return m_TriggerRotation; } }
  public float TouchLocatorTranslateScale { get { return m_TouchLocatorTranslateScale; } }
  public float TouchLocatorTranslateClamp { get { return m_TouchLocatorTranslateClamp; } }
  public Material GripReadyMaterial { get { return m_GripReadyMaterial; } }
  public Material GrippedMaterial { get { return m_GrippedMaterial; } }
  public Material BaseGrippedMaterial { get { return m_BaseGrippedMaterial; } }
  public Vector3 LeftGripPopInVector { get { return m_LeftGripPopInVector; } }
  public Vector3 LeftGripPopOutVector { get { return m_LeftGripPopOutVector; } }

  public Transform PointerAttachAnchor { get { return m_PointerAttachAnchor; } }
  public Transform PointerAttachPoint { get { return m_PointerAttachPoint; } }
  public Transform ToolAttachAnchor { get { return m_ToolAttachAnchor; } }
  public Transform ToolAttachPoint { get { return m_ToolAttachPoint; } }
  public Transform PinCushionSpawn { get { return m_PinCushionSpawn; } }
  public Transform MainAxisAttachPoint { get { return m_MainAxisAttachPoint; } }
  public Transform CameraAttachPoint { get { return m_CameraAttachPoint; } }
  public Transform ConsoleAttachPoint { get { return m_ConsoleAttachPoint; } }
  public Transform BaseAttachPoint { get { return m_BaseAttachPoint; } }
  public Transform GripAttachPoint { get { return m_GripAttachPoint; } }
  public Transform DropperDescAttachPoint { get { return m_DropperDescAttachPoint; } }

  public Renderer MainMesh { get { return m_MainMesh; } }
  public Renderer TriggerMesh { get { return m_TriggerMesh; } }
  public Renderer[] OtherMeshes { get { return m_OtherMeshes; } }
  public Renderer LeftGripMesh { get { return m_LeftGripMesh; } }
  public Renderer RightGripMesh { get { return m_RightGripMesh; } }
  public Transform PadTouchLocator { get { return m_PadTouchLocator; } }
  public Transform TriggerAnchor { get { return m_TriggerAnchor; } }
  public GameObject SelectionHintButton { get { return m_SelectionHintButton; } }
  public GameObject DeselectionHintButton { get { return m_DeselectionHintButton; } }
  public Renderer TransformVisualsRenderer { get { return m_TransformVisualsRenderer; } }
  public GameObject ActivateEffectPrefab { get { return m_ActivateEffectPrefab; } }
  public GameObject HighlightEffectPrefab { get { return m_HighlightEffectPrefab; } }
  public GameObject XRayVisuals { get { return m_XRayVisuals; } }

  // Vive controller components.
  public Transform PadAnchor { get { return m_PadAnchor; } }
  public Renderer PadMesh { get { return m_PadMesh; } }

  // Rift & Knuckles controller components.
  public Transform Joystick { get { return m_Joystick; } }
  public Renderer JoystickMesh { get { return m_JoystickMesh; } }
  public Renderer JoystickPad { get { return m_JoystickPad; } }
  public Renderer Button01Mesh { get {return m_Button01Mesh; } }
  public Renderer Button02Mesh { get { return m_Button02Mesh; } }

  // Wmr controller components.
  public Renderer PinCushionMesh { get { return m_PinCushion; } }

  // Brush objects
  public LineRenderer GuideLine { get { return m_GuideLine; } }

  public bool PadEnabled { get; set; }

  public BaseControllerBehavior Behavior { get { return m_Behavior; } }

  public InputManager.ControllerName ControllerName { get { return m_ControllerName; } }

  public ControllerStyle Style {
    get { return m_ControllerStyle; }
  }

  // Style is meant to be read-only and immutable, but there is currently one situation
  // that requires it to be writable. TODO: remove when possible?
  public ControllerStyle TempWritableStyle {
    set {
      if (m_ControllerStyle == value) { /* no warning */ }
      // This is kind of a hack, because the same prefab is used for both "empty geometry"
      // and "initializing steam vr". In all other cases, m_ControllerStyle is expected
      // to be set properly in the prefab. Perhaps we can remove this last mutable case
      // and detect the initializing case differently.
      else if (m_ControllerStyle == ControllerStyle.None &&
               value == ControllerStyle.InitializingSteamVR) {
        /* no warning */
      } else {
        Debug.LogWarningFormat(
            "Unity bug? Prefab had incorrect m_ControllerStyle {0} != {1}; try re-importing it.",
            m_ControllerStyle, value);
      }
      m_ControllerStyle = value;
    }
  }

  /// Returns null if the ControllerName is invalid, or the requested controller does not exist.
  public ControllerInfo ControllerInfo { get { return m_Behavior.ControllerInfo; } }

  private bool EmptyGeometry {
    get {
      return (m_ControllerStyle == ControllerStyle.None ||
              m_ControllerStyle == ControllerStyle.InitializingSteamVR); 
    }
  }

  // -------------------------------------------------------------------------------------------- //
  // Private Fields
  // -------------------------------------------------------------------------------------------- //

  class PopupAnimState {
    public readonly VrInput input;
    public readonly Transform anchor;
    public readonly float initialY;
    public readonly float initialScale;
    public float current;

    public PopupAnimState(Transform anchor, VrInput input) {
      this.anchor = anchor;
      this.input = input;
      this.current = 0;
      if (anchor != null) {
        this.initialY = anchor.localPosition.y;
        this.initialScale = anchor.localScale.x;
      }
    }
  }

  private PopupAnimState m_JoyAnimState;
  private PopupAnimState m_PadAnimState;
  private int m_LastPadButton;
  private Material m_BaseGrippedMaterial;
  private float m_LogitechPenHandednessHysteresis = 10.0f;
  // True if we're the default orientation, false if we need to be rotated 180 degrees.
  private bool m_LogitechPenHandedness;

  // Cached value of transform.parent.GetComponent<BaseControllerBehavior>()
  private BaseControllerBehavior m_Behavior;
  // Cached value of transform.parent.GetComponent<BaseControllerBehavior>().ControllerName
  private InputManager.ControllerName m_ControllerName;

  // -------------------------------------------------------------------------------------------- //
  // Unity Events
  // -------------------------------------------------------------------------------------------- //

  private void Awake() {
    if (LeftGripMesh != null) {
      m_BaseGrippedMaterial = LeftGripMesh.material;
    }
    m_JoyAnimState = new PopupAnimState(Joystick, VrInput.Thumbstick);
    m_PadAnimState = new PopupAnimState(PadAnchor, VrInput.Touchpad);
  }

  // -------------------------------------------------------------------------------------------- //
  // Private Helper Methods & Properties
  // -------------------------------------------------------------------------------------------- //

  // Quick access to the Material Catalog.
  private ControllerMaterialCatalog Materials {
    get { return ControllerMaterialCatalog.m_Instance; }
  }

  // Returns the ratio of the given controller input.
  private float GetPadRatio(VrInput input) {
    return SketchControlsScript.m_Instance.GetControllerPadShaderRatio(ControllerName, input);
  }

  // Animates the pad popping out of the controller.
  // Pass:
  //   active   if null, drives the animation to zero (presumably because there is nothing
  //            to put on the pad)
  private void UpdatePadAnimation(PopupAnimState state, Material active) {
    if (EmptyGeometry || state.anchor == null) { return; }

    float target = 0.0f;

    InputManager.ControllerName name = ControllerName;
    if (active != null && PadEnabled &&
        SketchControlsScript.m_Instance.ShouldRespondToPadInput(name) &&
        ControllerInfo.GetVrInputTouch(state.input)) {
      target = 1.0f;
    }

    if (target > state.current) {
      if (state.current == 0) {
        InputManager.m_Instance.TriggerHaptics(name, m_HapticPulseOn);  // Leaving 0
      }
      state.current = Mathf.Min(target, state.current + m_PadSpeed * Time.deltaTime);
    } else if (target < state.current) {
      state.current = Mathf.Max(target, state.current - m_PadSpeed * Time.deltaTime);
      if (state.current == 0) {
        InputManager.m_Instance.TriggerHaptics(name, m_HapticPulseOff);  // Arriving at 0
      }
    } else {
      return;  // No real need to mess with the transform
    }

    Vector3 vPos = state.anchor.localPosition;
    vPos.y = state.initialY + (state.current * m_PadPopUpAmount);
    state.anchor.localPosition = vPos;

    state.anchor.localScale =
        Vector3.one * (state.initialScale + (state.current * m_PadScaleAmount));
  }

  private void UpdateLogitechPadHandedness(Transform padXf) {
    Vector3 headUp = ViewpointScript.Head.up;
    Vector3 controllerRight = transform.right;
    float angle = Vector3.Angle(headUp, controllerRight);
    float flipAngle = m_LogitechPenHandedness ?
        90.0f - m_LogitechPenHandednessHysteresis :
        90.0f + m_LogitechPenHandednessHysteresis;
    m_LogitechPenHandedness = angle > flipAngle;
  }

  // Returns the active material when the pad is touched, else returns inactive.
  private Material SelectPadTouched(Material active, Material inactive) {
    return SelectIfTouched(VrInput.Touchpad, active, inactive);
  }

  // Returns the active material when the thumbstick is activated, else returns inactive material.
  private Material SelectThumbStickTouched(Material active, Material inactive) {
    return SelectIfTouched(VrInput.Thumbstick, active, inactive);
  }

  // Returns the active parameter when the pad is down/clicked, else returns inactive.
  private Material SelectBasedOn(Material active, Material inactive) {
    var info = ControllerInfo;
    // TODO: we should remove this MenuContextClick command in favor of calling it Button04;
    // (and potentially rename button04 to something more descriptive). The extra indirection isn't
    // buying us anything, and it prevents us from using GetVrInputTouch(button04) which is
    // actually what we mean here.
    if (info != null && info.GetCommand(InputManager.SketchCommands.MenuContextClick)) {
      return active;
    } else {
      return inactive;
    }
  }

  // Returns the active material when the input is touched, else returns inactive.
  private T SelectIfTouched<T>(VrInput input, T active, T inactive) {
    var info = ControllerInfo;
    if (info != null && info.GetVrInputTouch(input)) {
      return active;
    } else {
      return inactive;
    }
  }

  // -------------------------------------------------------------------------------------------- //
  // Public Event API
  // -------------------------------------------------------------------------------------------- //

  // Called after the behavior associated with this geometry (ie, its transform.parent) changes.
  // Only really for use by BaseControllerBehavior.
  public void OnBehaviorChanged() {
    m_Behavior = transform.parent.GetComponent<BaseControllerBehavior>();

    // Cache ControllerName, since we use it pretty much everywhere.
    if (m_Behavior == null) {
      Debug.LogWarning("Unexpected: Geometry has no behavior");
      m_ControllerName = InputManager.ControllerName.None;
    } else {
      m_ControllerName = m_Behavior.ControllerName;
    }
  }
}
}  // namespace TiltBrush
