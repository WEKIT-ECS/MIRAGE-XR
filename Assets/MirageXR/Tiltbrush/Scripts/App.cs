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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

#if USE_DOTNETZIP
using ZipSubfileReader = ZipSubfileReader_DotNetZip;
using ZipLibrary = Ionic.Zip;
#else
using ZipSubfileReader = TiltBrush.ZipSubfileReader_SharpZipLib;
using ZipLibrary = ICSharpCode.SharpZipLib.Zip;
#endif

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
namespace TiltBrush {
  public class App : MonoBehaviour {
    // ------------------------------------------------------------
    // Constants and types
    // ------------------------------------------------------------

    public const float METERS_TO_UNITS = 10f;
    public const float UNITS_TO_METERS = .1f;

    // These control the naming of various things related to the app. If you distribute your own
    // build, you need to call is something other that 'Tilt Brush', as that if a Google trademark -
    // see the BRANDING file for details.
    // As a minimum, you should change kAppDisplayName.

    // This is the name of the app, as displayed to the users running it.
    public const string kAppDisplayName = "Open Source Tilt Brush";
    // The vendor name - used for naming android builds - shouldn't have spaces.
    public const string kVendorName = "SomeCompany";
    // The vendor name - used for the company name in builds and fbx output. Can have spaces.
    public const string kDisplayVendorName = "Some Company";
    // This is the App name used when speaking to Google services
    public const string kGoogleServicesAppName = kAppDisplayName;
    // The name of the configuration file. You may want to change this if you think your users may
    // want to have a different config file for your edition of the app.
    public const string kConfigFileName = "Tilt Brush.cfg";
    // The name of the App folder (In the user's Documents folder) - you may want to share this with
    // the original Tilt Brush, or not.
    public const string kAppFolderName = "Tilt Brush";
    // The data folder used on Google Drive.
    public const string kDriveFolderName = kAppDisplayName;
    // Executable Base
    public const string kGuiBuildExecutableName = "OpenSourceTiltBrush";
    // Windows Executable
    public const string kGuiBuildWindowsExecutableName = kGuiBuildExecutableName + ".exe";
    // OSX Executable
    public const string kGuiBuildOSXExecutableName = kGuiBuildExecutableName + ".app";
    // Android Executable
    public const string kGuiBuildAndroidExecutableName =
        "com." + kVendorName + "." + kGuiBuildExecutableName + ".apk";

    public const string kPlayerPrefHasPlayedBefore = "Has played before";
    public const string kReferenceImagesSeeded = "Reference Images seeded";

    private const string kDefaultConfigPath = "DefaultConfig";

    private const int kHttpListenerPort = 40074;
    private const string kProtocolHandlerPrefix = "tiltbrush://remix/";
    private const string kFileMoveFilename = "WhereHaveMyFilesGone.txt";

    private const string kFileMoveContents =
        "All your " + kAppDisplayName + " files have been moved to\n" +
        "/sdcard/" + kAppFolderName + ".\n";

    public enum AppState {
      Error,
      LoadingBrushesAndLighting,
      FadeFromBlack,
      FirstRunIntro,
      Intro,
      Loading,
      QuickLoad,
      Standard,
      MemoryExceeded,
      Saving,
      Reset,
      Uploading,
      AutoProfiling,
      OfflineRendering,
    }

    // ------------------------------------------------------------
    // Static API
    // ------------------------------------------------------------

    private static App m_Instance;

    // Accessible at all times after config is initialized.
    public static Config Config {
      get { return m_Instance.m_Config; }
    }

    public static UserConfig UserConfig {
      get { return m_Instance.m_UserConfig; }
    }

    public static PlatformConfig PlatformConfig {
      get { return Config.PlatformConfig; }
    }

    public static VrSdk VrSdk {
      get { return m_Instance.m_VrSdk; }
    }

    public static SceneScript Scene {
      get { return m_Instance.m_SceneScript; }
    }

    public static CanvasScript ActiveCanvas {
      get { return Scene.ActiveCanvas; }
    }

    public static Switchboard Switchboard {
      get { return m_Instance.m_Switchboard; }
    }

    public static BrushColorController BrushColor {
      get { return m_Instance.m_BrushColorController; }
    }

    public static GroupManager GroupManager {
      get { return m_Instance.m_GroupManager; }
    }

    /// Returns the App instance, or null if the app has not been initialized
    /// with Awake().  Note that the App may not have had Start() called yet.
    ///
    /// Do not modify the script execution order if you only need inspector
    /// data from App.Instance. Put the inspector data in App.Config instead.
    public static App Instance {
      get { return m_Instance; }
#if UNITY_EDITOR
      // Bleh. Needed by BuildTiltBrush.cs
      set { m_Instance = value; }
#endif
    }

    public static AppState CurrentState {
      get {
        return m_Instance == null ? AppState.Loading : m_Instance.m_CurrentAppState;
      }
    }

    // ------------------------------------------------------------
    // Events
    // ------------------------------------------------------------

    public event Action<AppState, AppState> StateChanged;

    // ------------------------------------------------------------
    // Inspector data
    // ------------------------------------------------------------
    // Unless otherwise stated, intended to be read-only even if public

    [Header("External References")]
    [SerializeField] VrSdk m_VrSdk;
    [SerializeField] SceneScript m_SceneScript;
    [SerializeField] Config m_Config;

    [Header("General inspector")]
    [SerializeField] float m_FadeFromBlackDuration;
    [SerializeField] float m_QuickLoadHintDelay = 2f;

    [SerializeField] GpuIntersector m_GpuIntersector;

    public TiltBrushManifest m_Manifest;
#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
    [SerializeField] private TiltBrushManifest m_ManifestExperimental;
#endif

    [SerializeField] private SelectionEffect m_SelectionEffect;

    /// The root object for the "Room" coordinate system
    public Transform m_RoomTransform { get { return transform; } }
    /// The root object for the "Scene" coordinate system ("/SceneParent")
    public Transform m_SceneTransform;
    /// The root object for the "Canvas" coordinate system ("/SceneParent/Canvas")
    /// TODO: remove, in favor of .ActiveCanvas.transform
    public Transform m_CanvasTransform;
    /// The object "/SceneParent/EnvironmentParent"
    public Transform m_EnvironmentTransform;
    [SerializeField] GameObject m_SketchSurface;
    [SerializeField] GameObject m_ErrorDialog;
    [SerializeField] GameObject m_OdsPrefab;
    GameObject m_OdsPivot;

    [Header("Intro")]
    [SerializeField] float m_IntroSketchFadeInDuration = 5.0f;
    [SerializeField] float m_IntroSketchFadeOutDuration = 1.5f;
    [SerializeField] float m_IntroSketchMobileFadeInDuration = 3.0f;
    [SerializeField] float m_IntroSketchMobileFadeOutDuration = 1.5f;

    [SerializeField] FrameCountDisplay m_FrameCountDisplay;

    [SerializeField] private GameObject m_ShaderWarmup;

    // ------------------------------------------------------------
    // Private data
    // ------------------------------------------------------------

    /// Use C# event in preference to Unity callbacks because
    /// Unity doesn't send callbacks to disabled objects
    public event Action AppExit;

    private Queue m_RequestedTiltFileQueue = Queue.Synchronized(new Queue());

    private SketchSurfacePanel m_SketchSurfacePanel;
    private UserConfig m_UserConfig;

    private Switchboard m_Switchboard;
    private BrushColorController m_BrushColorController;
    private GroupManager m_GroupManager;

    /// Time origin of sketch in seconds for case when drawing is not sync'd to media.
    private double m_sketchTimeBase = 0;
    private bool m_QuickLoadInputWasValid;
    private bool m_QuickLoadEatInput;
    private AppState m_CurrentAppState;
    // Temporary: to narrow down b/37256058
    private AppState m_DesiredAppState_;
    private AppState m_DesiredAppState {
      get { return m_DesiredAppState_; }
      set {
        if (m_DesiredAppState_ != value) {
          Console.WriteLine("State <- {0}", value);
        }
        m_DesiredAppState_ = value;
      }
    }
    private int m_TargetFrameRate;
    private float m_RoomRadius;
    private bool? m_ShowControllers;
    private int m_QuickloadStallFrames;

    private float m_IntroFadeTimer;

    private bool m_FirstRunExperience;

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    /// Time spent in current sketch, in seconds.
    /// On load, this is restored to the timestamp of the last stroke.
    /// Updated per-frame.
    public double CurrentSketchTime {
      // Unity's Time.time has useful precision probably <= 1ms, and unknown
      // drift/accuracy. It is a single (but is a double, internally), so its
      // raw precision drops to ~2ms after ~4 hours and so on.
      // Time.timeSinceLevelLoad is also an option.
      //
      // C#'s DateTime API has low-ish precision (10+ ms depending on OS)
      // but likely the highest accuracy with respect to wallclock, since
      // it's reading from an RTC.
      //
      // High-precision timers are the opposite: high precision, but are
      // subject to drift.
      //
      // For realtime sync, Time.time is probably the best thing to use.
      // For postproduction sync, probably C# DateTime.
      get {
        // If you change this, also modify SketchTimeToLevelLoadTime
        return Time.timeSinceLevelLoad - m_sketchTimeBase;
      }
      set {
        if (value < 0) { throw new ArgumentException("negative"); }
        m_sketchTimeBase = Time.timeSinceLevelLoad - value;
      }
    }

    public float RoomRadius {
      get { return m_RoomRadius; }
    }

    public SelectionEffect SelectionEffect {
      get { return m_SelectionEffect; } set { m_SelectionEffect = value; }
    }
    public bool IsFirstRunExperience { get { return m_FirstRunExperience; } }
    public bool HasPlayedBefore {
      get;
      private set;
    }

    public bool StartupError { get; set; }

    public bool ShowControllers {
      get { return m_ShowControllers.GetValueOrDefault(true); }
      set {
        InputManager.m_Instance.ShowControllers(value);
        m_ShowControllers = value;
      }
    }

    public GpuIntersector GpuIntersector {
      get { return m_GpuIntersector; }
    }

    public TrTransform OdsHeadPrimary { get; set; }
    public TrTransform OdsScenePrimary { get; set; }

    public TrTransform OdsHeadSecondary { get; set; }
    public TrTransform OdsSceneSecondary { get; set; }

    public FrameCountDisplay FrameCountDisplay {
      get { return m_FrameCountDisplay; }
    }

    // ------------------------------------------------------------
    // Implementation
    // ------------------------------------------------------------

    public double SketchTimeToLevelLoadTime(double sketchTime) {
      return sketchTime + m_sketchTimeBase;
    }

    // Tilt Brush code assumes the current directory is next to the Support/
    // folder. Enforce that assumption
    static void SetCurrentDirectoryToApplication() {
      // dataPath is:
      //   Editor  - <project folder>/Assets
      //   Windows - TiltBrush_Data/
      //   Linux   - TiltBrush_Data/
      //   OSX     - TiltBrush.app/Contents/
#if UNITY_STANDALONE_WIN
      string oldDir = Directory.GetCurrentDirectory();
      string dataDir = UnityEngine.Application.dataPath;
      string appDir = Path.GetDirectoryName(dataDir);
      try {
        Directory.SetCurrentDirectory(appDir);
      } catch (Exception e) {
        Debug.LogErrorFormat("Couldn't set dir to {0}: {1}", appDir, e);
      }
      string curDir = Directory.GetCurrentDirectory();
      Debug.LogFormat("Dir {0} -> {1}", oldDir, curDir);
#endif
    }

    public void Init() {
      m_Instance = this;
      m_UserConfig = new UserConfig();

      // Begone, physics! You were using 0.3 - 1.3ms per frame on Quest!
      Physics.autoSimulation = false;

      // See if this is the first time
      HasPlayedBefore = PlayerPrefs.GetInt(kPlayerPrefHasPlayedBefore, 0) == 1;

      // Copy files into Support directory
      CopySupportFiles();

      SetCurrentDirectoryToApplication();
      Coords.Init(this);
      Scene.Init();

      CameraConfig.Init();
      if (!string.IsNullOrEmpty(m_UserConfig.Profiling.SketchToLoad)) {
        Config.m_SketchFiles = new string[] { m_UserConfig.Profiling.SketchToLoad };
      }

      if (m_UserConfig.Testing.FirstRun) {
        PlayerPrefs.DeleteKey(kPlayerPrefHasPlayedBefore);
        PlayerPrefs.DeleteKey(kReferenceImagesSeeded);
        PlayerPrefs.DeleteKey(PanelManager.kPlayerPrefAdvancedMode);
        AdvancedPanelLayouts.ClearPlayerPrefs();
        PointerManager.ClearPlayerPrefs();
        HasPlayedBefore = false;
      }
      // Cache this variable for the length of the play session.  HasPlayedBefore will be updated,
      // but m_FirstRunExperience should not.
      m_FirstRunExperience = !HasPlayedBefore && !Config.skipIntro;

      m_Switchboard = new Switchboard();
      m_GroupManager = new GroupManager();

      m_BrushColorController = GetComponent<BrushColorController>();

      // Tested on Windows. I hope they don't change the names of these preferences.
      PlayerPrefs.DeleteKey("Screenmanager Is Fullscreen mode");
      PlayerPrefs.DeleteKey("Screenmanager Resolution Height");
      PlayerPrefs.DeleteKey("Screenmanager Resolution Width");

      // if (DevOptions.I.UseAutoProfiler) {
      //   gameObject.AddComponent<AutoProfiler>();
      // }

      m_Manifest = GetMergedManifest(consultUserConfig: true);
    }

    void Start() {
      if (!VrSdk.IsHmdInitialized()) {
        Debug.Log("VR HMD was not initialized on startup.");
        StartupError = true;
        CreateErrorDialog();
      } else {
        Debug.LogFormat("Sdk mode: {0} XRDevice.model: {1}",
                        App.Config.m_SdkMode, UnityEngine.XR.XRSettings.loadedDeviceName);
      }

      m_TargetFrameRate = VrSdk.GetHmdTargetFrameRate();
      if (VrSdk.GetHmdDof() == TiltBrush.VrSdk.DoF.None) {
        Application.targetFrameRate = m_TargetFrameRate;
      }

      if (VrSdk.HasRoomBounds()) {
        Vector3 extents = VrSdk.GetRoomExtents();
        m_RoomRadius = Mathf.Min(Mathf.Abs(extents.x), Mathf.Abs(extents.z));
      }

      //these guys don't need to be alive just yet
      PointerManager.m_Instance.EnablePointerStrokeGeneration(false);

      QualityControls.m_Instance.SetupCameraEffects();
      bool bVR = VrSdk.GetHmdDof() != TiltBrush.VrSdk.DoF.None;
      InputManager.m_Instance.AllowVrControllers = bVR;
      PointerManager.m_Instance.UseSymmetryWidget(bVR);

      switch (VrSdk.GetControllerDof()) {
        case TiltBrush.VrSdk.DoF.Six:
          // Vive, Rift + Touch
          SketchControlsScript.m_Instance.ActiveControlsType =
                                            SketchControlsScript.ControlsType.SixDofControllers;
          break;
        case TiltBrush.VrSdk.DoF.None:
          SketchControlsScript.m_Instance.ActiveControlsType =
                                            SketchControlsScript.ControlsType.ViewingOnly;
          break;
        case TiltBrush.VrSdk.DoF.Two:
          // Monoscopic
          SketchControlsScript.m_Instance.ActiveControlsType =
                                            SketchControlsScript.ControlsType.KeyboardMouse;
          break;
      }

      m_CurrentAppState = AppState.Standard;
      m_DesiredAppState = AppState.LoadingBrushesAndLighting;
      if (StartupError) {
        m_DesiredAppState = AppState.Error;
      }

      m_SketchSurfacePanel = m_SketchSurface.GetComponent<SketchSurfacePanel>();

      ShowControllers = App.UserConfig.Flags.ShowControllers;

      SwitchState();

      if (Config.m_AutoProfile || m_UserConfig.Profiling.AutoProfile) {
        StateChanged += AutoProfileOnStartAndQuit;
      }

    }

    private void AutoProfileOnStartAndQuit(AppState oldState, AppState newState) {
      if (newState == AppState.Standard) {
        Invoke("AutoProfileAndQuit", Config.m_AutoProfileWaitTime);
        StateChanged -= AutoProfileOnStartAndQuit;
      }
    }

    private void AutoProfileAndQuit() {
      SketchControlsScript.m_Instance.IssueGlobalCommand(
          SketchControlsScript.GlobalCommands.DoAutoProfileAndQuit);
    }

    public void SetDesiredState(AppState rDesiredState) {
      m_DesiredAppState = rDesiredState;
    }

    void Update() {
      //look for state change
      if (m_CurrentAppState != m_DesiredAppState) {
        SwitchState();
      }

      if (InputManager.m_Instance.GetCommand(InputManager.SketchCommands.Activate)) {
        //kinda heavy-handed, but whatevs
        InitCursor();
      }

      //update state
      switch (m_CurrentAppState) {
        case AppState.LoadingBrushesAndLighting: {
            if (!BrushCatalog.m_Instance.IsLoading
                && !EnvironmentCatalog.m_Instance.IsLoading
                && !m_ShaderWarmup.activeInHierarchy) {
              if (AppAllowsCreation()) {
                BrushController.m_Instance.SetBrushToDefault();
                BrushColor.SetColorToDefault();
              } else {
                PointerManager.m_Instance.SetBrushForAllPointers(BrushCatalog.m_Instance.DefaultBrush);
              }
              
              SketchControlsScript.m_Instance.RequestPanelsVisibility(true);
              PointerManager.m_Instance.EnablePointerStrokeGeneration(true);
              m_DesiredAppState = AppState.Standard;
            }
            break;
          }
        case AppState.QuickLoad: {
            // Allow extra frames to complete fade to black.
            // Required for OVR to position the overlay because it only does so once the transition
            // is complete.
            if (m_QuickloadStallFrames-- < 0) {
              bool bContinueDrawing = SketchMemoryScript.m_Instance.ContinueDrawingFromMemory();
              if (!bContinueDrawing) {
                FinishLoading();
              }
            }
            break;
          }
        case AppState.Uploading:
          SketchControlsScript.m_Instance.UpdateControlsForUploading();
          break;
        case AppState.MemoryExceeded:
          SketchControlsScript.m_Instance.UpdateControlsForMemoryExceeded();
          break;
        case AppState.Standard:
          // Logic for fading out intro sketches.
          if (m_IntroFadeTimer > 0 &&
              !PanelManager.m_Instance.IntroSketchbookMode){
              PanelManager.m_Instance.ReviveFloatingPanelsForStartup();
          }

          // Continue edit-time playback, if any.
          SketchMemoryScript.m_Instance.ContinueDrawingFromMemory();
          if (PanelManager.m_Instance.SketchbookActiveIncludingTransitions() &&
              PanelManager.m_Instance.IntroSketchbookMode) {
            // Limit controls if the user hasn't exited from the sketchbook post intro.
            SketchControlsScript.m_Instance.UpdateControlsPostIntro();
          } else {
            SketchControlsScript.m_Instance.UpdateControls();
          }
          break;
        case AppState.Reset:
          SketchControlsScript.m_Instance.UpdateControls();
          if (!PointerManager.m_Instance.IsMainPointerCreatingStroke() &&
              !PointerManager.m_Instance.IsMainPointerProcessingLine()) {
            StartReset();
          }
          break;
      }
    }

    public void ExitIntroSketch() {
      PanelManager.m_Instance.SetInIntroSketchbookMode(false);
      PointerManager.m_Instance.IndicateBrushSize = true;
      PointerManager.m_Instance.PointerColor = PointerManager.m_Instance.PointerColor;
    }

    private void StartReset() {
      // Switch to paint tool if not already there.
      SketchSurfacePanel.m_Instance.EnableDefaultTool();

      // Disable preview line.
      PointerManager.m_Instance.AllowPointerPreviewLine(false);

      // Switch to the default brush type and size.
      BrushController.m_Instance.SetBrushToDefault();

      // Reset to the default brush color.
      BrushColor.SetColorToDefault();

      // Clear saved colors.
      CustomColorPaletteStorage.m_Instance.ClearAllColors();

      // Turn off straightedge
      PointerManager.m_Instance.StraightEdgeModeEnabled = false;

      // Turn off straightedge ruler.
      if (PointerManager.m_Instance.StraightEdgeGuide.IsShowingMeter()) {
        PointerManager.m_Instance.StraightEdgeGuide.FlipMeter();
      }

      // Close any panel menus that might be open (e.g. Sketchbook)
      if (PanelManager.m_Instance.SketchbookActive()) {
        PanelManager.m_Instance.ToggleSketchbookPanels();
      } else if (PanelManager.m_Instance.SettingsActive()) {
        PanelManager.m_Instance.ToggleSettingsPanels();
      } else if (PanelManager.m_Instance.MemoryWarningActive()) {
        PanelManager.m_Instance.ToggleMemoryWarningMode();
      } else if (PanelManager.m_Instance.BrushLabActive()) {
        PanelManager.m_Instance.ToggleBrushLabPanels();
      }

      // Hide all panels.
      SketchControlsScript.m_Instance.RequestPanelsVisibility(false);

      // Reset all panels.
      SketchControlsScript.m_Instance.IssueGlobalCommand(
          SketchControlsScript.GlobalCommands.ResetAllPanels);

      // Rotate want panels to default orientation (color picker).
      PanelManager.m_Instance.ResetWandPanelRotation();

      // Close Twitch widget.
      if (SketchControlsScript.m_Instance.IsCommandActive(SketchControlsScript.GlobalCommands.IRC)) {
        SketchControlsScript.m_Instance.IssueGlobalCommand(SketchControlsScript.GlobalCommands.IRC);
      }

      // Close Youtube Chat widget.
      if (SketchControlsScript.m_Instance.IsCommandActive(
          SketchControlsScript.GlobalCommands.YouTubeChat)) {
        SketchControlsScript.m_Instance.IssueGlobalCommand(
            SketchControlsScript.GlobalCommands.YouTubeChat);
      }

      // Hide the pointer and reticle.
      PointerManager.m_Instance.RequestPointerRendering(false);
      SketchControlsScript.m_Instance.ForceShowUIReticle(false);
    }

    private void FinishReset() {
      // Switch to the default environment.
      SceneSettings.m_Instance.SetDesiredPreset(EnvironmentCatalog.m_Instance.DefaultEnvironment);

      // Clear the sketch and reset the scene transform.
      SketchControlsScript.m_Instance.NewSketch(fade: false);

      // Disable mirror.
      PointerManager.m_Instance.SetSymmetryMode(PointerManager.SymmetryMode.None);

      // Reset mirror position.
      PointerManager.m_Instance.ResetSymmetryToHome();

      // Show the wand panels.
      SketchControlsScript.m_Instance.RequestPanelsVisibility(true);

      // Show the pointer.
      PointerManager.m_Instance.RequestPointerRendering(true);
      PointerManager.m_Instance.EnablePointerStrokeGeneration(true);

      // Forget command history.
      SketchMemoryScript.m_Instance.ClearMemory();
    }

    void FinishLoading() {
      //if we just released the button, kick a fade out
      if (m_QuickLoadInputWasValid) {
        App.VrSdk.PauseRendering(false);
      }

      m_DesiredAppState = AppState.Standard;
      if (VrSdk.GetControllerDof() == TiltBrush.VrSdk.DoF.Six) {
        float holdDelay = (m_CurrentAppState == AppState.QuickLoad) ? 1.0f : 0.0f;
        StartCoroutine(DelayedSketchLoadedCard(holdDelay));
      } else {
        OutputWindowScript.m_Instance.AddNewLine(
          OutputWindowScript.LineType.Special, "Sketch Loaded!");
      }

      OnPlaybackComplete();
      m_SketchSurfacePanel.EnableRenderer(true);

      SketchControlsScript.m_Instance.RequestPanelsVisibility(true);
      SketchSurfacePanel.m_Instance.EatToolsInput();
      SketchSurfacePanel.m_Instance.RequestHideActiveTool(false);
      SketchControlsScript.m_Instance.RestoreFloatingPanels();
      PointerManager.m_Instance.RequestPointerRendering(
          SketchSurfacePanel.m_Instance.ShouldShowPointer());
      PointerManager.m_Instance.RestoreBrushInfo();
      WidgetManager.m_Instance.LoadingState(false);
      WidgetManager.m_Instance.WidgetsDormant = true;
      SketchControlsScript.m_Instance.EatGrabInput();

      SketchMemoryScript.m_Instance.SanitizeMemoryList();

      if (Config.OfflineRender) {
        SketchControlsScript.m_Instance.IssueGlobalCommand(
          SketchControlsScript.GlobalCommands.RenderCameraPath);
      }
    }

    private IEnumerator<Timeslice> DelayedSketchLoadedCard(float delay) {
      float stall = delay;
      while (stall >= 0.0f) {
        stall -= Time.deltaTime;
        yield return null;
      }

      OutputWindowScript.m_Instance.CreateInfoCardAtController(
          InputManager.ControllerName.Brush, "Sketch Loaded!");
    }

    void SwitchState() {
      switch (m_CurrentAppState) {
        case AppState.LoadingBrushesAndLighting:
          if (VrSdk.GetControllerDof() == VrSdk.DoF.Two) {
            // Sketch surface tool is not properly loaded because
            // it is the default tool.
            SketchSurfacePanel.m_Instance.ActiveTool.EnableTool(false);
            SketchSurfacePanel.m_Instance.ActiveTool.EnableTool(true);
          }
          break;
        case AppState.Reset:
          // Demos should reset to the standard state only.
          Debug.Assert(m_DesiredAppState == AppState.Standard);
          FinishReset();
          break;
        case AppState.AutoProfiling:
        case AppState.OfflineRendering:
          InputManager.m_Instance.EnablePoseTracking(true);
          break;
        case AppState.MemoryExceeded:
          SketchSurfacePanel.m_Instance.EnableDefaultTool();
          PanelManager.m_Instance.ToggleMemoryWarningMode();
          PointerManager.m_Instance.RequestPointerRendering(
              SketchSurfacePanel.m_Instance.ShouldShowPointer());
          break;
      }

      switch (m_DesiredAppState) {
        case AppState.LoadingBrushesAndLighting:
          BrushCatalog.m_Instance.BeginReload();
          break;
        case AppState.QuickLoad:
          SketchMemoryScript.m_Instance.QuickLoadDrawingMemory();
          break;
        case AppState.MemoryExceeded:
          if (!PanelManager.m_Instance.MemoryWarningActive()) {
            PanelManager.m_Instance.ToggleMemoryWarningMode();
          }
          SketchSurfacePanel.m_Instance.EnableSpecificTool(BaseTool.ToolType.EmptyTool);
          break;
        case AppState.Standard:
          PointerManager.m_Instance.DisablePointerPreviewLine();
          // Refresh the tinting on the controllers
          PointerManager.m_Instance.PointerColor = PointerManager.m_Instance.PointerColor;
          break;
        case AppState.Reset:
          PointerManager.m_Instance.EnablePointerStrokeGeneration(false);
          PointerManager.m_Instance.AllowPointerPreviewLine(false);
          PointerManager.m_Instance.EatLineEnabledInput();
          PointerManager.m_Instance.EnableLine(false);
          break;
        case AppState.AutoProfiling:
        case AppState.OfflineRendering:
          InputManager.m_Instance.EnablePoseTracking(false);
          break;
      }

      var oldState = m_CurrentAppState;
      m_CurrentAppState = m_DesiredAppState;
      if (StateChanged != null) {
        StateChanged(oldState, m_CurrentAppState);
      }
    }

    public bool ShouldTintControllers() {
      return m_DesiredAppState == AppState.Standard && !PanelManager.m_Instance.IntroSketchbookMode;
    }

    public bool IsInStateThatAllowsPainting() {
      return CurrentState == AppState.Standard &&
          !PanelManager.m_Instance.IntroSketchbookMode;
    }

    public bool IsInStateThatAllowsAnyGrabbing() {
      return !PanelManager.m_Instance.IntroSketchbookMode &&
          (CurrentState == AppState.Standard || CurrentState == AppState.Loading) &&
          !SelectionManager.m_Instance.IsAnimatingTossFromGrabbingGroup;
    }

    public bool IsLoading() {
      return CurrentState == AppState.Loading || CurrentState == AppState.QuickLoad;
    }

    void UpdateQuickLoadLogic() {
      if (CurrentState == AppState.Loading && AppAllowsCreation()) {
        //require the user to stop holding the trigger before pulling it again to speed load
        if (m_QuickLoadEatInput) {
          if (!InputManager.m_Instance.GetCommand(InputManager.SketchCommands.Panic)) {
            m_QuickLoadEatInput = false;
          }
        } else {
          if (InputManager.m_Instance.GetCommand(InputManager.SketchCommands.Panic) &&
              !SketchControlsScript.m_Instance.IsUserInteractingWithAnyWidget() &&
              !SketchControlsScript.m_Instance.IsUserGrabbingWorld() &&
              (!VrSdk.IsAppFocusBlocked())) {
            //if we just pressed the button, kick a fade in
            if (!m_QuickLoadInputWasValid) {
              App.VrSdk.PauseRendering(true);
              InputManager.m_Instance.TriggerHaptics(InputManager.ControllerName.Wand, 0.05f);
            }

            m_QuickLoadInputWasValid = true;
            if (m_CurrentAppState != AppState.QuickLoad) {
              m_QuickloadStallFrames = 1;
              m_DesiredAppState = AppState.QuickLoad;
              m_SketchSurfacePanel.EnableRenderer(false);
              InputManager.m_Instance.TriggerHaptics(InputManager.ControllerName.Wand, 0.1f);
            }
          } else {
            //if we just released the button, kick a fade out
            if (m_QuickLoadInputWasValid) {
              App.VrSdk.PauseRendering(false);
            }
            m_QuickLoadInputWasValid = false;
          }
        }
      }
    }

    void OnIntroComplete() {
      PointerManager.m_Instance.EnablePointerStrokeGeneration(true);
      SketchControlsScript.m_Instance.RequestPanelsVisibility(true);

      // If the user chooses to skip the intro, assume they've done the tutorial before.
      PlayerPrefs.SetInt(App.kPlayerPrefHasPlayedBefore, 1);

      m_DesiredAppState = AppState.Standard;
    }

    void InitCursor() {
      if (StartupError) {
        return;
      }
      if (VrSdk.GetHmdDof() == TiltBrush.VrSdk.DoF.None) {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
      }
    }

    public static T DeserializeObjectWithWarning<T>(string text, out string warning) {
      // Try twice, once to catch "unknown key" warnings, once to actually get a result.
      warning = null;
      try {
        return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings {
          MissingMemberHandling = MissingMemberHandling.Error
        });
      } catch (JsonSerializationException e) {
        warning = e.Message;
        return JsonConvert.DeserializeObject<T>(text);
      }
    }

    public void CreateErrorDialog(string msg = null) {
      GameObject dialog = Instantiate(m_ErrorDialog);
      var textXf = dialog.transform.Find("Text");
      var textMesh = textXf.GetComponent<TextMesh>();
      if (msg == null) {
        msg = "Failed to detect VR";
      }
      textMesh.text = string.Format(@"        Tiltasaurus says...
                   {0}", msg);
    }

    static public bool AppAllowsCreation() {
      // TODO: this feels like it should be an explicit part of Config,
      // not something based on VR hardware...
      return App.VrSdk.GetControllerDof() != TiltBrush.VrSdk.DoF.None;
    }

    static public string PlatformPath() {
      if (!Application.isEditor && Application.platform == RuntimePlatform.OSXPlayer) {
        return System.IO.Directory.GetParent(Application.dataPath).Parent.ToString();
      } else if (Application.platform == RuntimePlatform.Android) {
        return Application.persistentDataPath;
      }

      return System.IO.Directory.GetParent(Application.dataPath).ToString();
    }

    static public string SupportPath() {
      return Path.Combine(PlatformPath(), "Support");
    }

    /// Returns a parent of UserPath; used to figure out how much path
    /// is necessary to display to the user when giving feedback. We
    /// assume this is the "boring" portion of the path that they can infer.
    public static string DocumentsPath() {
      switch (Application.platform) {
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.WindowsEditor:
        case RuntimePlatform.OSXPlayer:
        case RuntimePlatform.OSXEditor:
        case RuntimePlatform.LinuxPlayer:
          return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        case RuntimePlatform.Android:
        case RuntimePlatform.IPhonePlayer:
        default:
          return Application.persistentDataPath;
      }
    }

    public static bool InitDirectoryAtPath(string path) {
      if (Directory.Exists(path)) {
        return true;
      }
      if (!FileUtils.InitializeDirectoryWithUserError(path)) {
        return false;
      }
      return true;
    }

    public static string ShortenForDescriptionText(string desc) {
      desc = desc.Split('\n')[0];
      if (desc.Length > 33) {
        desc = desc.Substring(0, 30) + "...";
      }
      return desc;
    }

    void OnApplicationQuit() {
      if (AppExit != null) {
        AppExit();
      }
    }

    void OnPlaybackComplete() {
      SaveLoadScript.m_Instance.SignalPlaybackCompletion();
      if (SketchControlsScript.m_Instance.SketchPlaybackMode !=
          SketchMemoryScript.PlaybackMode.Timestamps) {

        // For non-timestamp playback mode, adjust current time to last stroke in drawing.
        try {
          this.CurrentSketchTime = SketchMemoryScript.m_Instance.GetApproximateLatestTimestamp();
        } catch (InvalidOperationException) {
          // Can happen as an edge case, eg if we try to load a file that doesn't exist.
          this.CurrentSketchTime = 0;
        }
      }
    }

    public TiltBrushManifest GetMergedManifest(bool consultUserConfig) {
      var manifest = m_Manifest;
#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
      if (Config.IsExperimental) {
        // At build time, we don't want the user config to affect the build output.
        if (consultUserConfig
            && m_UserConfig.Flags.ShowDangerousBrushes
            && m_ManifestExperimental != null) {
          manifest = Instantiate(m_Manifest);
          manifest.AppendFrom(m_ManifestExperimental);
        }
      }
#endif
      return manifest;
    }

#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
    public bool IsBrushExperimental(BrushDescriptor brush) {
      return m_ManifestExperimental.Brushes.Contains(brush);
    }
#endif

    DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null) {
#if !UNITY_ANDROID
      var filePath = assembly.Location;
      const int c_PeHeaderOffset = 60;
      const int c_LinkerTimestampOffset = 8;

      var buffer = new byte[2048];

      using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        stream.Read(buffer, 0, 2048);

      var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
      var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
      var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
      return linkTimeUtc.ToLocalTime();
#else
    return DateTime.Now;
#endif
    }

    /// This copies the support files from inside the Streaming Assets folder to the support folder.
    /// This only happens on Android. The files have to be extracted directly from the .apk.
    private static void CopySupportFiles() {
      if (Application.platform != RuntimePlatform.Android) {
        return;
      }
      if (!Directory.Exists(SupportPath())) {
        Directory.CreateDirectory(SupportPath());
      }

      Func<string, int> GetIndexOfEnd = (s) => Application.streamingAssetsPath.IndexOf(s) + s.Length;

      // Find the apk file
      int apkIndex = GetIndexOfEnd("file://");
      int fileIndex = Application.streamingAssetsPath.IndexOf("!/");
      string apkFilename = Application.streamingAssetsPath.Substring(apkIndex, fileIndex - apkIndex);

      const string supportBeginning = "assets/Support/";

      try {
        using (Stream zipFile = File.Open(apkFilename, FileMode.Open, FileAccess.Read)) {
          ZipLibrary.ZipFile zip = new ZipLibrary.ZipFile(zipFile);
          foreach (ZipLibrary.ZipEntry entry in zip) {
            if (entry.IsFile && entry.Name.StartsWith(supportBeginning)) {
              // Create the directory if needed.
              string fullPath = Path.Combine(App.SupportPath(),
                                             entry.Name.Substring(supportBeginning.Length));
              string directory = Path.GetDirectoryName(fullPath);
              if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
              }

              // Copy the data over to a file.
              using (Stream entryStream = zip.GetInputStream(entry)) {
                using (FileStream fileStream = File.Create(fullPath)) {
                  byte[] buffer = new byte[16 * 1024]; // Do it in 16k chunks
                  while (true) {
                    int size = entryStream.Read(buffer, 0, buffer.Length);
                    if (size > 0) {
                      fileStream.Write(buffer, 0, size);
                    } else {
                      break;
                    }
                  }
                }
              }

            }
          }
          zip.Close();
        }
      } catch (Exception ex) {
        Debug.LogException(ex);
      }
    }

  }  // class App
}  // namespace TiltBrush
