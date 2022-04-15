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

// Pros and cons for MonoBehaviour (component) and ScriptableObject (asset):
//
// Component:
// - PRO: Runtime changes take place immediately
// - CON: Runtime changes do not persist after the game exits
// - PRO: Can reference objects in the scene
// - CON: Changes go into Main.unity; harder to review
//
// Asset:
// - CON: Runtime changes are visible only after the .asset hot-reloads
// - PRO: Runtime changes persist after the game exits
// - CON: Cannot reference objects in the scene; only prefabs and other assets
// - PRO: Changes go into their own .asset; easier to review

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TiltBrush {

// These names are used in our analytics, so they must be protected from obfuscation.
// Do not change the names of any of them, unless they've never been released.
[Serializable]
public enum SdkMode {
  Unset = -1,
  Oculus = 0,
  SteamVR,
  Cardboard_Deprecated,
  Monoscopic,
  Gvr,
  Mrtk
}

// These names are used in our analytics, so they must be protected from obfuscation.
// Do not change the names of any of them, unless they've never been released.
// This enum should be "VrHeadsetHardware".  Controller type is not necessarily
// implied by headset type.
[Serializable]
public enum VrHardware {
  Unset,
  None,
  Rift,
  Vive,
  Daydream,
  Wmr,
  Quest,
  Hololens2
}

/// These are not used in analytics. They indicate the type of tool tip description that will appear
/// on a UI component.
public enum DescriptionType {
  None = -1,
  Button = 0,
  Slider,
  PreviewCube,
}

/// Script Ordering:
/// - does not need to come after anything
/// - must come before everything that uses App.Config (ie, all scripts)
///
/// Used to store global configuration data and constants.
/// For per-platform data, see PlatformConfig.cs
/// Despite being public, all this data should be considered read-only
///
public class Config : MonoBehaviour {
  // When set, ModelWidget creation waits for Poly models to be loaded into memory.
  // When not set, ModelWidgets may be created with "dummy" Models which are automatically
  // replaced with the real Model once it's loaded.
  public readonly bool kModelWidgetsWaitForLoad = true;

  [System.Serializable]
  public class BrushReplacement {
    [BrushDescriptor.AsStringGuid] public string FromGuid;
    [BrushDescriptor.AsStringGuid] public string ToGuid;
  }

  private class UserConfigChange {
    public FieldInfo section;
    public MemberInfo setting;
    public object value;
  }

    public bool disableTutorials = false;
    public bool skipIntro = false;
    public bool disableEnvironment = false;
    public bool disableAudio = false;
    public bool disableAutosave = false;

  [Header("Startup")]
  public string m_FakeCommandLineArgsInEditor;

  [Header("Overwritten by build process")]
  [SerializeField] private PlatformConfig m_PlatformConfig;

  // True for experimental mode.
  // Cannot be ifdef'd out, because it is modified during the build process.
  // Public to allow App.cs and BuildTiltBrush.cs to access it; do not use it otherwise.
  public bool m_IsExperimental;

  // The sdk mode indicates which SDK (OVR, SteamVR, etc.) that we're using to drive the display.
  public SdkMode m_SdkMode;

  // Whether or not to just do an automatic profile and then exit.
  public bool m_AutoProfile;
  // How long to wait before starting to profile.
  public float m_AutoProfileWaitTime = 10f;

  [Header("App")]
  public SecretsConfig Secrets;
  public string[] m_SketchFiles = new string[0];
  [NonSerialized] public bool m_QuickLoad = true;

  public SecretsConfig.ServiceAuthData GoogleSecrets => Secrets[SecretsConfig.Service.Google];
  public SecretsConfig.ServiceAuthData SketchfabSecrets => Secrets[SecretsConfig.Service.Sketchfab];
  public SecretsConfig.ServiceAuthData OculusSecrets => Secrets[SecretsConfig.Service.Oculus];
  public SecretsConfig.ServiceAuthData OculusMobileSecrets => Secrets[SecretsConfig.Service.OculusMobile];

  // This indicates which hardware (Rift or Vive) is being used. This is distinct from which SDK
  // is being used (Oculus VR, Steam's Open VR, Monoscopic, etc.).
  public VrHardware VrHardware {
    // This is set lazily the first time VrHardware is accesssed.
    get {
      if (m_VrHardware == TiltBrush.VrHardware.Unset) {
        if (m_SdkMode == SdkMode.Oculus) {
          if (App.Config.IsMobileHardware) {
            m_VrHardware = VrHardware.Quest;
          } else {
            m_VrHardware = VrHardware.Rift;
          }
        } 
        #if STEAMVR_SUPPORTED
        else if (m_SdkMode == SdkMode.SteamVR) {
          // If SteamVR fails for some reason we will discover it here.
          try {
            if (Valve.VR.OpenVR.System == null) {
              m_VrHardware = VrHardware.None;
              return m_VrHardware;
            }
          } catch (Exception) {
            m_VrHardware = VrHardware.None;
            return m_VrHardware;
          }

          // GetHwTrackedInSteamVr relies on headset detection, so controllers don't have to be on.
          m_VrHardware = GetHwTrackedInSteamVr();
        } 
        #endif
        else if (m_SdkMode == SdkMode.Gvr) {
          m_VrHardware = TiltBrush.VrHardware.Daydream;
        } 
          else if (m_SdkMode == SdkMode.Mrtk){
            if(IsMobileHardware){
              m_VrHardware = VrHardware.Hololens2;
            }
            else{
              m_VrHardware = VrHardware.Wmr;
            }

          }
        else {
          m_VrHardware = VrHardware.None;
        }
      }

      return m_VrHardware;
    }
  }

  public String HeadsetModelName {
    get {
      if(string.IsNullOrEmpty(m_HeadsetModelName)) {
        m_HeadsetModelName = UnityEngine.XR.XRSettings.loadedDeviceName;
      }
      return m_HeadsetModelName;
    }
  }

  /// Return a value kinda sorta half-way between "building for Android" and "running on Android"
  /// In order of increasing strictness, here are the in-Editor semantics of various methods
  /// of querying the platform. All of these methods return true when running on-device.
  /// Note that each level is a strict subset of the level(s) above:
  ///
  /// 1. true if build target is Android
  ///      #if UNITY_ANDROID / #endif
  ///      EditorUserBuildSetings.activeBuildTarget == BuildTarget.Android
  /// 2. true if build target is Android AND if SpoofMobileHardware.MobileHardware is set
  ///      Config.IsMobileHardware
  /// 3. never true in Editor; only true on-device
  ///      Application.platform == RuntimePlatform.Android
  ///      App.Config.IsMobileHardware && !SpoofMobileHardware.MobileHardware:
  ///
  /// TODO: Can we get away with just #1 and #3, and remove #2? That would let us remove
  /// SpoofMobileHardware.MobileHardware too.
  public bool IsMobileHardware {
    // Only sadness will ensue if the user tries to set Override.MobileHardware=true
    // but their editor platform is still set to Windows.
#if UNITY_EDITOR && UNITY_ANDROID
    get { return Application.platform == RuntimePlatform.Android
              || SpoofMobileHardware.MobileHardware; }
#else
    get { return Application.platform == RuntimePlatform.Android; }
#endif
  }

  [Header("Ods")]
  public int m_OdsNumFrames = 0;
  public float m_OdsFps = 30;
  public string m_OdsOutputPath = "";
  public string m_OdsOutputPrefix = "";
  [NonSerialized] public bool m_OdsPreview = false;
  [NonSerialized] public bool m_OdsCollapseIpd = true;
  [NonSerialized] public float m_OdsTurnTableDegrees = 0.0f;

#if UNITY_EDITOR
  [Header("Editor-only")]
  // Force use of a particular controller geometry, for testing
  [Tooltip("Set this to a prefab in Assets/Prefabs/VrSystems/VrControllers/OVR")]
  public GameObject m_ControlsPrefabOverrideOvr;
  [Tooltip("Set this to a prefab in Assets/Prefabs/VrSystems/VrControllers/SteamVr")]
  public GameObject m_ControlsPrefabOverrideSteamVr;
#endif

  [Header("Versioning")]
  public string m_VersionNumber;  // eg "17.0b", "18.3"
  public string m_BuildStamp;     // eg "f73783b61", "f73783b61-exp", "(menuitem)"

  [Header("Misc")]
  public GameObject m_SteamVrRenderPrefab;
  public bool m_UseBatchedBrushes;
  // Delete Batch's GeometryPool after about a second.
  public bool m_EnableBatchMemoryOptimization;
  public string m_MediaLibraryReadme;
  public DropperTool m_Dropper;
  public bool m_AxisManipulationIsResize;
  public GameObject m_LabsButtonOverlayPrefab;
  public bool m_GpuIntersectionEnabled = true;
  public bool m_AutosaveRestoreEnabled = false;
  public bool m_AllowWidgetPinning;
  public bool m_DebugWebRequest;
  public bool m_ToggleProfileOnAppButton = false;

  [Header("Global Shaders")]
  public Shader m_BlitToComputeShader;

  [Header("Upload and Export")]
  // Some brushes put a birth time in the vertex attributes; because we export
  // this data (we really shouldn't) it's helpful to disable it when one needs
  // deterministic export.
  public bool m_ForceDeterministicBirthTimeForExport;
  [NonSerialized] public List<string> m_FilePatternsToExport;
  [NonSerialized] public string m_ExportPath;
  [NonSerialized] public string m_VideoPathToRender;
  // TODO: m22 ripcord; remove for m23
  public bool m_EnableReferenceModelExport;
  // TODO: m22 ripcord; remove for m23
  public bool m_EnableGlbVersion2;
  [Tooltip("Causes the temporary Upload directory to be kept around (Editor only)")]
  public bool m_DebugUpload;
  public TiltBrushToolkit.TbtSettings m_TbtSettings;

  [Header("Loading")]
  public bool m_ReplaceBrushesOnLoad;
  [SerializeField] List<BrushReplacement> m_BrushReplacementMap;
  public string m_IntroSketchUsdFilename;
  [Range(0.001f, 4)]
  public float m_IntroSketchSpeed = 1.0f;
  public bool m_IntroLooped = false;

  [Header("Shader Warmup")]
  public bool CreateShaderWarmupList;

  [Header("Description Prefabs")]
  [SerializeField] GameObject m_ButtonDescriptionOneLinePrefab;
  [SerializeField] GameObject m_ButtonDescriptionTwoLinesPrefab;
  [SerializeField] GameObject m_ButtonDescriptionThreeLinesPrefab;
  [SerializeField] GameObject m_SliderDescriptionOneLinePrefab;
  [SerializeField] GameObject m_SliderDescriptionTwoLinesPrefab;
  [SerializeField] GameObject m_PreviewCubeDescriptionOneLinePrefab;
  [SerializeField] GameObject m_PreviewCubeDescriptionTwoLinesPrefab;

  public GameObject CreateDescriptionFor(DescriptionType type, int numberOfLines) {
    switch (type) {
    case DescriptionType.None:
      return null;
    case DescriptionType.Button:
      switch (numberOfLines) {
      case 1:
        return Instantiate(m_ButtonDescriptionOneLinePrefab);
      case 2:
        return Instantiate(m_ButtonDescriptionTwoLinesPrefab);
      case 3:
        return Instantiate(m_ButtonDescriptionThreeLinesPrefab);
      default:
        throw new Exception($"{type} description does not have a ${numberOfLines} line variant");
      }
    case DescriptionType.Slider:
      switch (numberOfLines) {
      case 1:
        return Instantiate(m_SliderDescriptionOneLinePrefab);
      case 2:
        return Instantiate(m_SliderDescriptionTwoLinesPrefab);
      default:
        throw new Exception($"{type} description does not have a ${numberOfLines} line variant");
      }
    case DescriptionType.PreviewCube:
      switch (numberOfLines) {
      case 1:
        return Instantiate(m_PreviewCubeDescriptionOneLinePrefab);
      case 2:
        return Instantiate(m_PreviewCubeDescriptionTwoLinesPrefab);
      default:
        throw new Exception($"{type} description does not have a ${numberOfLines} line variant");
      }
    default:
      throw new Exception($"Unknown description type: {type}");
    }
  }

  public bool OfflineRender {
    get {
      return !string.IsNullOrEmpty(m_VideoPathToRender);
    }
  }

  public PlatformConfig PlatformConfig {
    get {
      return m_PlatformConfig;
    }
  }

  // ------------------------------------------------------------
  // Private data
  // ------------------------------------------------------------
  private VrHardware m_VrHardware = VrHardware.Unset;  // This should not be used outside of
                                                       // VrHardware as it is lazily set inside
                                                       // VrHardware.
  private Dictionary<Guid, Guid> m_BrushReplacement = null;
  private List<UserConfigChange> m_UserConfigChanges = new List<UserConfigChange>();
  private string m_HeadsetModelName;

  // ------------------------------------------------------------
  // Yucky externals
  // ------------------------------------------------------------

  public Guid GetReplacementBrush(Guid original) {
    Guid replacement;
    if (m_BrushReplacement.TryGetValue(original, out replacement)) {
      return replacement;
    }
    return original;
  }

  // ------------------------------------------------------------
  // Yucky internals
  // ------------------------------------------------------------

#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
  public static bool IsExperimental {
    get { return App.Config.m_IsExperimental; }
  }
#endif

  void Awake() {
    m_BrushReplacement = new Dictionary<Guid, Guid>();
#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
    if (IsExperimental) {
      foreach (var brush in m_BrushReplacementMap) {
        m_BrushReplacement.Add(new Guid(brush.FromGuid), new Guid(brush.ToGuid));
      }
    }
#endif
  }

  #if STEAMVR_SUPPORTED
  private string GetSteamVrDeviceStringProperty(Valve.VR.ETrackedDeviceProperty property) {
    uint index = 0; // Index 0 is always the headset
    var system = Valve.VR.OpenVR.System;
    // If system == null, then somehow, the SteamVR SDK was not properly loaded in.
    Debug.Assert(system != null, "OpenVR System not found, check \"Virtual Reality Supported\"");

    var error = Valve.VR.ETrackedPropertyError.TrackedProp_Success;

    var capacity = system.GetStringTrackedDeviceProperty(index, property, null, 0, ref error);
    System.Text.StringBuilder buffer = new System.Text.StringBuilder((int)capacity);
    system.GetStringTrackedDeviceProperty(index, property, buffer, capacity, ref error);
    if (error == Valve.VR.ETrackedPropertyError.TrackedProp_Success) {
      return buffer.ToString();
    } else {
      Debug.LogErrorFormat("GetStringTrackedDeviceProperty error {0}", error.ToString());
      return null;
    }
  }
  #else

  #endif

  #if STEAMVR_SUPPORTED
  // Checking what kind of hardware (Rift, Vive, of WMR) is being used in SteamVR.
  private VrHardware GetHwTrackedInSteamVr() {
    string manufacturer = GetSteamVrDeviceStringProperty(
        Valve.VR.ETrackedDeviceProperty.Prop_ManufacturerName_String);

    if (string.IsNullOrEmpty(manufacturer)) {
      OutputWindowScript.Error("Could not determine VR Headset manufacturer.");
      return VrHardware.Vive;
    } else if (manufacturer.Contains("Oculus")) {
      return VrHardware.Rift;
    } else if (manufacturer.Contains("WindowsMR")) {
      return VrHardware.Wmr;
    } else {
      return VrHardware.Vive;
    }
  }
  #endif

  /// Apply any changes specified on the command line to a user config object
  public void ApplyUserConfigOverrides(UserConfig userConfig) {
    foreach (var change in m_UserConfigChanges) {
      var section = change.section.GetValue(userConfig);
      if (section == null) {
        Debug.LogWarningFormat("Weird - could not access UserConfig.{0}.", change.section.Name);
        continue;
      }
      if (change.setting is FieldInfo) {
        ((FieldInfo) change.setting).SetValue(section, change.value);
      } else {
        ((PropertyInfo) change.setting).SetValue(section, change.value, null);
      }
      change.section.SetValue(userConfig, section);
    }
    foreach (var replacement in userConfig.Testing.BrushReplacementMap) {
      m_BrushReplacement.Add(replacement.Key, replacement.Value);
    }

    // Report deprecated members to users.
    if (userConfig.Flags.HighResolutionSnapshots) {
      OutputWindowScript.Error("HighResolutionSnapshots is deprecated.");
      OutputWindowScript.Error("Use SnapshotHeight and SnapshotWidth.");
    }
  }
}

}  // namespace TiltBrush
