//// Copyright 2020 The Tilt Brush Authors
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
using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace TiltBrush {

  // These names are used in our save format, so they must be protected from obfuscation
  // Do not change the names of any of them, unless they've never been released.
  [Serializable]
  public enum StencilType {
    Plane,
    Cube,
    Sphere,
    Capsule,
    Cone,
    Cylinder,
    InteriorDome,
    Pyramid,
    Ellipsoid
  }

  [Serializable]
  public struct StencilMapKey {
    public StencilType m_Type;
    public StencilWidget m_StencilPrefab;
  }

  public struct StencilContactInfo {
    public StencilWidget widget;
    public Vector3 pos;
    public Vector3 normal;
  }

  public class GrabWidgetData {
    public readonly GameObject m_WidgetObject;
    public readonly GrabWidget m_WidgetScript;

    // These fields are only valid during a call to GetNearestGrabWidget,
    // and are undefined afterwards. Do not use them.

    public bool m_NearController;
    // only valid if m_NearController == true
    public float m_ControllerScore;

    public GrabWidgetData(GrabWidget widget) {
      m_WidgetScript = widget;
      m_WidgetObject = widget.gameObject;
    }
    // Could maybe get by without this since all users of Clone() don't care if they
    // receive a plain old GrabWidgetData or not.
    public virtual GrabWidgetData Clone() {
      return new GrabWidgetData(m_WidgetScript) {
        m_NearController = m_NearController,
        m_ControllerScore = m_ControllerScore
      };
    }
  }

  public class TypedWidgetData<T> : GrabWidgetData where T : GrabWidget {
    private readonly T m_typedWidget;
    public new T WidgetScript => m_typedWidget;
    public TypedWidgetData(T widget) : base(widget) {
      m_typedWidget = widget;
    }
    public override GrabWidgetData Clone() {
      return new TypedWidgetData<T>(m_typedWidget) {
        m_NearController = m_NearController,
        m_ControllerScore = m_ControllerScore
      };
    }
  }

  public class WidgetManager : MonoBehaviour {
    static public WidgetManager m_Instance;

    [SerializeField] GameObject m_WidgetPinPrefab;
    [SerializeField] private GameObject m_CameraPathPositionKnotPrefab;
    [SerializeField] private GameObject m_CameraPathRotationKnotPrefab;
    [SerializeField] private GameObject m_CameraPathSpeedKnotPrefab;
    [SerializeField] private GameObject m_CameraPathFovKnotPrefab;
    [SerializeField] private GameObject m_CameraPathKnotSegmentPrefab;
    [SerializeField] private GrabWidgetHome m_Home;
    [SerializeField] private GameObject m_HomeHintLinePrefab;
    [SerializeField] float m_WidgetSnapAngle = 15.0f;
    [SerializeField] float m_GazeMaxAngleFromFacing = 70.0f;
    [SerializeField] private float m_PanelFocusActivationScore;
    [SerializeField] private float m_ModelVertCountScalar = 1.0f;

    [Header("Stencils")]
    [SerializeField] StencilMapKey[] m_StencilMap;
    [SerializeField] private float m_StencilAttractDist = 0.5f;
    [SerializeField] private float m_StencilAttachHysteresis = 0.1f;
    [SerializeField] private string m_StencilLayerName;
    [SerializeField] private string m_PinnedStencilLayerName;

    private bool m_WidgetsDormant;
    private bool m_InhibitGrabWhileLoading;

    private GameObject m_HomeHintLine;
    private MeshFilter m_HomeHintLineMeshFilter;
    private Vector3 m_HomeHintLineBaseScale;

    private StencilContactInfo[] m_StencilContactInfos;
    private const int m_StencilBucketSize = 16;
    private StencilWidget m_ActiveStencil;
    private bool m_StencilsDisabled;

    // All grabbable widgets should be in exactly one of these lists.
    // Widgets will be in the most specific list.
    private List<GrabWidgetData> m_GrabWidgets;
    private List<TypedWidgetData<StencilWidget>> m_StencilWidgets;

    // These lists are used by the PinTool.  They're kept in sync by the
    // widget manager, but the PinTool is responsible for their coherency.
    private List<GrabWidget> m_CanBePinnedWidgets;
    private List<GrabWidget> m_CanBeUnpinnedWidgets;
    public event Action RefreshPinAndUnpinAction;

    private TiltModels75[] m_loadingTiltModels75;
    private TiltImages75[] m_loadingTiltImages75;
    private TiltVideo[] m_loadingTiltVideos;

    private List<GrabWidgetData> m_WidgetsNearBrush;
    private List<GrabWidgetData> m_WidgetsNearWand;

    // This value is used by SketchMemoryScript to check the sketch against memory limits.
    // It's incremented when a model is registered, and decremented when a model is
    // unregistered.
    private int m_ModelVertCount;
    // Similar to above, this value is used to check against memory limits.  Images are always
    // the same number of verts, however, so this number is scaled by texture size.  It's a
    // hand-wavey calculation.
    private int m_ImageVertCount;

    // Camera path.
    [NonSerialized] public bool FollowingPath;
    private bool m_CameraPathsVisible;

    static private Dictionary<ushort, GrabWidget> sm_BatchMap = new Dictionary<ushort, GrabWidget>();

    public StencilWidget ActiveStencil {
      get { return m_ActiveStencil; }
    }

    public void ResetActiveStencil() {
      m_ActiveStencil = null;
    }

    public int StencilLayerIndex {
      get { return LayerMask.NameToLayer(m_StencilLayerName); }
    }

    public int PinnedStencilLayerIndex {
      get { return LayerMask.NameToLayer(m_PinnedStencilLayerName); }
    }

    public LayerMask PinnedStencilLayerMask {
      get { return LayerMask.GetMask(m_PinnedStencilLayerName); }
    }

    public LayerMask StencilLayerMask {
      get { return LayerMask.GetMask(m_StencilLayerName); }
    }

    public List<GrabWidgetData> WidgetsNearBrush {
      get { return m_WidgetsNearBrush; }
    }

    public List<GrabWidgetData> WidgetsNearWand {
      get { return m_WidgetsNearWand; }
    }

    public bool AnyWidgetsToPin {
      get { return m_CanBePinnedWidgets.Count > 0; }
    }

    public bool AnyWidgetsToUnpin {
      get { return m_CanBeUnpinnedWidgets.Count > 0; }
    }

    public float ModelVertCountScalar {
      get { return m_ModelVertCountScalar; }
    }

    public int ImageVertCount {
      get { return m_ImageVertCount; }
    }

    public void AdjustModelVertCount(int amount) {
      m_ModelVertCount += amount;
    }

    public void AdjustImageVertCount(int amount) {
      m_ImageVertCount += amount;
    }

    public int WidgetsVertCount {
      get { return m_ModelVertCount + m_ImageVertCount; }
    }

    public bool AnyVideoWidgetActive => false;

    // Returns the associated widget for the given batchId.
    // Returns null if key doesn't exist.
    public GrabWidget GetBatch(ushort batchId) {
      if (sm_BatchMap.ContainsKey(batchId)) {
        return sm_BatchMap[batchId];
      }
      return null;
    }

    public void AddWidgetToBatchMap(GrabWidget widget, ushort batchId) {
      Debug.Assert(!sm_BatchMap.ContainsKey(batchId));
      sm_BatchMap.Add(batchId, widget);
    }

    void Awake() {
      m_Instance = this;
    }

    public void Init() {
      m_GrabWidgets = new List<GrabWidgetData>();
      m_StencilWidgets = new List<TypedWidgetData<StencilWidget>>();

      m_CanBePinnedWidgets = new List<GrabWidget>();
      m_CanBeUnpinnedWidgets = new List<GrabWidget>();

      m_WidgetsNearBrush = new List<GrabWidgetData>();
      m_WidgetsNearWand = new List<GrabWidgetData>();

      m_Home.Init();
      m_Home.SetFixedPosition(Vector3.zero);
      m_HomeHintLine = (GameObject)Instantiate(m_HomeHintLinePrefab);
      m_HomeHintLineMeshFilter = m_HomeHintLine.GetComponent<MeshFilter>();
      m_HomeHintLineBaseScale = m_HomeHintLine.transform.localScale;
      m_HomeHintLine.transform.parent = transform;
      m_HomeHintLine.SetActive(false);

      m_StencilContactInfos = new StencilContactInfo[m_StencilBucketSize];
      m_StencilsDisabled = false;

      FollowingPath = false;
      m_CameraPathsVisible = false;
    }

    public GameObject CameraPathPositionKnotPrefab { get { return m_CameraPathPositionKnotPrefab; } }
    public GameObject CameraPathRotationKnotPrefab { get { return m_CameraPathRotationKnotPrefab; } }
    public GameObject CameraPathSpeedKnotPrefab { get { return m_CameraPathSpeedKnotPrefab; } }
    public GameObject CameraPathFovKnotPrefab { get { return m_CameraPathFovKnotPrefab; } }
    public GameObject CameraPathKnotSegmentPrefab { get { return m_CameraPathKnotSegmentPrefab; } }

    public IEnumerable<GrabWidgetData> ActiveGrabWidgets {
      get {
        if (m_InhibitGrabWhileLoading) {
          // Returns only widgets that are not part of the sketch
          return m_GrabWidgets.Where(x => x.m_WidgetObject.activeSelf);
        }
        return GetAllActiveGrabWidgets();
      }
    }

    private IEnumerable<GrabWidgetData> GetAllActiveGrabWidgets() {
      for (int i = 0; i < m_GrabWidgets.Count; ++i) {
        if (m_GrabWidgets[i].m_WidgetObject.activeSelf) {
          yield return m_GrabWidgets[i];
        }
      }
    }

    public bool HasSelectableWidgets() {
      return false;
    }

    public bool HasExportableContent() {
      return false;
    }

    public bool HasNonExportableContent() {
      return false;
    }

    public bool StencilsDisabled {
      get { return m_StencilsDisabled; }
      set {
        if (value != m_StencilsDisabled) {
          // Flip flag and visuals for all stencils.
          for (int i = 0; i < m_StencilWidgets.Count; ++i) {
            StencilWidget sw = m_StencilWidgets[i].WidgetScript;
            if (sw) {
              sw.RefreshVisibility(value);
            }
          }
          m_ActiveStencil = null;
        }
        m_StencilsDisabled = value;
        RefreshPinAndUnpinLists();
      }
    }

    private static string CanonicalizeForCompare(string path) {
      return path.ToLower().Replace("\\", "/");
    }

    // Used only at .tilt-loading time
    public void SetDataFromTilt(TiltModels75[] value) {
      m_loadingTiltModels75 = value;
    }

    // Used only at .tilt-loading time
    public void SetDataFromTilt(TiltImages75[] value) {
      m_loadingTiltImages75 = value;
    }

    public void SetDataFromTilt(TiltVideo[] value) {
      m_loadingTiltVideos = value;
    }

    public WidgetPinScript GetWidgetPin() {
      GameObject pinObj = Instantiate(m_WidgetPinPrefab);
      pinObj.transform.parent = transform;
      return pinObj.GetComponent<WidgetPinScript>();
    }

    public void DestroyWidgetPin(WidgetPinScript pin) {
      if (pin != null) {
        Destroy(pin.gameObject);
      }
    }

    // Set the position that widgets can snap to in the current environment
    public void SetHomePosition(Vector3 position) {
      m_Home.SetFixedPosition(position);
    }

    // Dormant models are still grabbable but visuals/haptics are disabled
    public bool WidgetsDormant {
      get { return m_WidgetsDormant; }
      set {
        m_WidgetsDormant = value;
        Shader.SetGlobalFloat("_WidgetsDormant", value ? 0 : 1);
      }
    }

    public float WidgetSnapAngle {
      get { return m_WidgetSnapAngle; }
    }

    public bool IsOriginHomeWithinSnapRange(Vector3 pos) {
      return m_Home.WithinRange(pos);
    }

    public Transform GetHomeXf() {
      return m_Home.transform;
    }

    public void SetHomeOwner(Transform owner) {
      m_Home.SetOwner(owner);
      m_Home.Reset();
    }

    public void ClearHomeOwner() {
      m_Home.SetOwner(null);
      m_Home.gameObject.SetActive(false);
      m_HomeHintLine.SetActive(false);
    }

    public void EnableHome(bool bEnable) {
      m_Home.gameObject.SetActive(bEnable);
      if (!bEnable) {
        m_HomeHintLine.SetActive(false);
      }
    }

    public void LoadingState(bool bEnter) {
      m_InhibitGrabWhileLoading = bEnter;
    }

    public void UpdateHomeHintLine(Vector3 vModelSnapPos) {
      if (!m_Home.WithinRange(vModelSnapPos) && m_Home.WithinHintRange(vModelSnapPos)) {
        // Enable, position, and scale hint line.
        m_HomeHintLine.SetActive(true);
        Vector3 vHomeToModel = vModelSnapPos - m_Home.transform.position;
        m_HomeHintLine.transform.position = m_Home.transform.position +
            (vHomeToModel * 0.5f);
        m_HomeHintLine.transform.up = vHomeToModel.normalized;

        Vector3 vScale = m_HomeHintLineBaseScale;
        vScale.y = vHomeToModel.magnitude * 0.5f;
        m_HomeHintLine.transform.localScale = vScale;
        
        if(App.Instance.SelectionEffect)
          App.Instance.SelectionEffect.RegisterMesh(m_HomeHintLineMeshFilter);
        m_Home.RenderHighlight();
      } else {
        // Disable the line.
        m_HomeHintLine.SetActive(false);
      }
    }

    public void MagnetizeToStencils(ref Vector3 pos, ref Quaternion rot) {
      // Early out if stencils are disabled.
      if (m_StencilsDisabled && !App.UserConfig.Flags.GuideToggleVisiblityOnly) {
        return;
      }

      Vector3 samplePos = pos;

      // If we're painting, we have a different path for magnetization that relies on the
      // previous frame.
      if (PointerManager.m_Instance.IsLineEnabled()) {
        // If we don't have an active stencil, we're done here.
        if (m_ActiveStencil == null) {
          return;
        }

        // Using the 0 index of m_StencilContactInfos as a shortcut.
        m_StencilContactInfos[0].widget = m_ActiveStencil;
        FindClosestPointOnWidgetSurface(pos, ref m_StencilContactInfos[0]);

        m_ActiveStencil.SetInUse(true);
        pos = m_StencilContactInfos[0].pos;
        rot = Quaternion.LookRotation(m_StencilContactInfos[0].normal);
      } else {
        StencilWidget prevStencil = m_ActiveStencil;
        float fPrevScore = -m_StencilAttachHysteresis;
        int iPrevIndex = -1;
        m_ActiveStencil = null;

        // Run through the overlap list and find the best stencil to stick to.
        int iPrimaryIndex = -1;
        float fBestScore = 0;
        int sIndex = 0;
        foreach (var stencil in m_StencilWidgets) {
          StencilWidget sw = stencil.WidgetScript;
          Debug.Assert(sw != null);

          // Reset tint
          sw.SetInUse(false);

          // Does a rough check to see if the stencil might overlap. OverlapSphereNonAlloc is
          // shockingly slow, which is why we don't use it.
          Collider collider = stencil.m_WidgetScript.GrabCollider;
          float centerDist = (collider.bounds.center - samplePos).sqrMagnitude;
          if (centerDist >
              (m_StencilAttractDist * m_StencilAttractDist + collider.bounds.extents.sqrMagnitude)) {
            continue;
          }
          m_StencilContactInfos[sIndex].widget = sw;

          FindClosestPointOnWidgetSurface(samplePos, ref m_StencilContactInfos[sIndex]);

          // Find out how far we are from this point and save it as a score.
          float distToSurfactPoint = (m_StencilContactInfos[sIndex].pos - samplePos).magnitude;
          float score = 1.0f - (distToSurfactPoint / m_StencilAttractDist);
          if (score > fBestScore) {
            iPrimaryIndex = sIndex;
            fBestScore = score;
            m_ActiveStencil = m_StencilContactInfos[sIndex].widget;
          }

          if (m_StencilContactInfos[sIndex].widget == prevStencil) {
            fPrevScore = score;
            iPrevIndex = sIndex;
          }

          if (++sIndex == m_StencilBucketSize) {
            break;
          }
        }

        // If we are switching between stencils, check to see if we're switching "enough".
        if (iPrevIndex != -1 && m_ActiveStencil != null && prevStencil != m_ActiveStencil) {
          if (fPrevScore + m_StencilAttachHysteresis > fBestScore) {
            m_ActiveStencil = prevStencil;
            iPrimaryIndex = iPrevIndex;
          }
        }

        // If we found a good stencil, return the surface collision transform.
        if (m_ActiveStencil != null) {
          m_ActiveStencil.SetInUse(true);
          pos = m_StencilContactInfos[iPrimaryIndex].pos;
          rot = Quaternion.LookRotation(m_StencilContactInfos[iPrimaryIndex].normal);
        }

        if (prevStencil != m_ActiveStencil) {
          PointerManager.m_Instance.DisablePointerPreviewLine();
        }
      }

      return;
    }

    bool FindClosestPointOnCollider(
        Ray rRay, Collider collider, out RaycastHit rHitInfo, float fDist) {
      rHitInfo = new RaycastHit();
      return collider.Raycast(rRay, out rHitInfo, fDist);
    }

    void FindClosestPointOnWidgetSurface(Vector3 pos, ref StencilContactInfo info) {
      info.widget.FindClosestPointOnSurface(pos, out info.pos, out info.normal);
    }

    public bool ShouldUpdateCollisions() {
      return ActiveGrabWidgets.Any(elt => elt.m_WidgetScript.IsCollisionEnabled());
    }

    public IEnumerable<StencilWidget> StencilWidgets {
      get {
        return m_StencilWidgets
          .Select(d => d == null ? null : d.WidgetScript)
          .Where(w => w != null);
      }
    }

    public StencilWidget GetStencilPrefab(StencilType type) {
      for (int i = 0; i < m_StencilMap.Length; ++i) {
        if (m_StencilMap[i].m_Type == type) {
          return m_StencilMap[i].m_StencilPrefab;
        }
      }
      throw new ArgumentException(type.ToString());
    }

    public List<GrabWidget> GetAllUnselectedActiveWidgets() {
      List<GrabWidget> widgets = new List<GrabWidget>();
      if (!m_StencilsDisabled) {
        GetUnselectedActiveWidgetsInList(m_StencilWidgets);
      }
      return widgets;

      void GetUnselectedActiveWidgetsInList<T>(List<TypedWidgetData<T>> list) where T : GrabWidget {
        for (int i = 0; i < list.Count; ++i) {
          GrabWidget w = list[i].m_WidgetScript;
          if (!w.Pinned && w.transform.parent == App.Scene.MainCanvas.transform &&
              w.gameObject.activeSelf) {
            widgets.Add(w);
          }
        }
      }
    }

    public void RefreshPinAndUnpinLists() {
      if (RefreshPinAndUnpinAction != null) {
        m_CanBePinnedWidgets.Clear();
        m_CanBeUnpinnedWidgets.Clear();
        RefreshPinUnpinWidgetList(m_StencilWidgets);

        RefreshPinAndUnpinAction();
      }

      // New in C# 7 - local functions!
      void RefreshPinUnpinWidgetList<T>(List<TypedWidgetData<T>> widgetList) where T : GrabWidget {
        foreach (var widgetData in widgetList) {
          var widget = widgetData.WidgetScript;
          if (widget.gameObject.activeSelf && widget.AllowPinning) {
            if (widget.Pinned) {
              m_CanBeUnpinnedWidgets.Add(widget);
            } else {
              m_CanBePinnedWidgets.Add(widget);
            }
          }
        }
      }
    }

    public void RegisterHighlightsForPinnableWidgets(bool pinnable) {
      List<GrabWidget> widgets = pinnable ? m_CanBePinnedWidgets : m_CanBeUnpinnedWidgets;
      for (int i = 0; i < widgets.Count; ++i) {
        GrabWidget w = widgets[i];
        // If stencils are disabled, don't highlight them, cause we can interact with 'em.
        if (WidgetManager.m_Instance.StencilsDisabled) {
          if (w is StencilWidget) {
            continue;
          }
        }
        w.RegisterHighlight();
      }
    }

    public void RegisterGrabWidget(GameObject rWidget) {
      // Find b/29514616
      if (ReferenceEquals(rWidget, null)) {
        throw new ArgumentNullException("rWidget");
      } else if (rWidget == null) {
        throw new ArgumentNullException("rWidget(2)");
      }
      GrabWidget generic = rWidget.GetComponent<GrabWidget>();
      if (generic == null) {
        throw new InvalidOperationException($"Object {rWidget.name} is not a GrabWidget");
      }

      m_GrabWidgets.Add(new GrabWidgetData(generic));

      RefreshPinAndUnpinLists();
    }

    // Returns true if a widget was removed
    static bool RemoveFrom<T>(List<T> list, GameObject rWidget)
        where T : GrabWidgetData {
      int idx = list.FindIndex((data) => data.m_WidgetObject == rWidget);
      if (idx != -1) {
        list.RemoveAt(idx);
        return true;
      }
      return false;
    }

    public void UnregisterGrabWidget(GameObject rWidget) {
      // Get this widget's batchId out of the map.
      sm_BatchMap.Remove(rWidget.GetComponent<GrabWidget>().BatchId);

      // Pull out of pin tool lists.
      RefreshPinAndUnpinLists();

      if (RemoveFrom(m_StencilWidgets, rWidget)) { return; }
      RemoveFrom(m_GrabWidgets, rWidget);
    }

    public void RefreshNearestWidgetLists(Ray currentGazeRay, int currentGazeObject) {
      m_WidgetsNearBrush.Clear();
      UpdateNearestGrabsFor(InputManager.ControllerName.Brush, currentGazeRay, currentGazeObject);
      foreach (GrabWidgetData widget in ActiveGrabWidgets) {
        if (widget.m_NearController) {
          // Deep copy.
          m_WidgetsNearBrush.Add(widget.Clone());
        }
      }

      m_WidgetsNearWand.Clear();
      UpdateNearestGrabsFor(InputManager.ControllerName.Wand, currentGazeRay, currentGazeObject);
      foreach (GrabWidgetData widget in ActiveGrabWidgets) {
        if (widget.m_NearController) {
          m_WidgetsNearWand.Add(widget.Clone());
        }
      }
    }

    // Helper for RefreshNearestWidgetLists
    void UpdateNearestGrabsFor(
        InputManager.ControllerName name, Ray currentGazeRay, int currentGazeObject) {
      // Reset hit flags.
      foreach (var elt in ActiveGrabWidgets) {
        elt.m_NearController = false;
        elt.m_ControllerScore = -1.0f;
      }

      Vector3 controllerPos = Vector3.zero;
      if (name == InputManager.ControllerName.Brush) {
        controllerPos = InputManager.m_Instance.GetBrushControllerAttachPoint().position;
      } else if (name == InputManager.ControllerName.Wand) {
        controllerPos = InputManager.m_Instance.GetWandControllerAttachPoint().position;
      } else {
        Debug.LogError("UpdateNearestGrabsFor() only supports Brush and Wand controller types.");
      }

      // Figure out if controller is in view frustum.  If it isn't, don't allow widget grabs.
      Vector3 vToController = controllerPos - currentGazeRay.origin;
      vToController.Normalize();
      if (Vector3.Angle(vToController, currentGazeRay.direction) > m_GazeMaxAngleFromFacing) {
        return;
      }

      BasePanel gazePanel = null;
      if (currentGazeObject > -1) {
        gazePanel = PanelManager.m_Instance.GetPanel(currentGazeObject);
      }

      foreach (var data in ActiveGrabWidgets) {
        if (!data.m_WidgetScript.enabled) {
          continue;
        }
        if (m_StencilsDisabled && data.m_WidgetScript is StencilWidget) {
          continue;
        }
        if (SelectionManager.m_Instance.ShouldRemoveFromSelection() &&
            !data.m_WidgetScript.CanGrabDuringDeselection()) {
          continue;
        }
        if (SelectionManager.m_Instance.IsWidgetSelected(data.m_WidgetScript)) {
          continue;
        }
        float score = data.m_WidgetScript.GetActivationScore(controllerPos, name);
        if (score < m_PanelFocusActivationScore && name == InputManager.ControllerName.Brush &&
            gazePanel && data.m_WidgetObject == gazePanel.gameObject) {
          // If the brush is pointing at a panel, make sure that the panel will be the widget grabbed
          score = m_PanelFocusActivationScore;
        }
        if (score < 0) {
          continue;
        }

        data.m_NearController = true;
        data.m_ControllerScore = score;
      }
    }

    public float DistanceToNearestWidget(Ray ray) {
      // If we're in controller mode, find the nearest colliding widget that might get in our way.
      float fNearestWidget = 99999.0f;
      foreach (var elt in ActiveGrabWidgets) {
        float fWidgetDist = 0.0f;
        if (elt.m_WidgetScript.DistanceToCollider(ray, out fWidgetDist)) {
          fNearestWidget = Mathf.Min(fNearestWidget, fWidgetDist);
        }
      }
      return fNearestWidget;
    }

    public void DestroyAllWidgets() {
      DestroyWidgetList(m_StencilWidgets);
      App.Switchboard.TriggerAllWidgetsDestroyed();

      void DestroyWidgetList<T>(List<TypedWidgetData<T>> widgetList,
          bool hideBeforeDestroy = true) where T : GrabWidget {
        while (widgetList.Count > 0) {
          GrabWidget widget = widgetList[0].m_WidgetScript;
          GameObject obj = widgetList[0].m_WidgetObject;
          if (hideBeforeDestroy) { widget.Hide(); }
          widget.OnPreDestroy();
          UnregisterGrabWidget(obj);
          Destroy(obj);
        }
      }
    }

    /// Returns true when all media widgets have finished getting created.
    /// Note that:
    /// - ImageWidgets may have low-res textures
    /// - ModelWidgets may not have a model yet (depending on Config.kModelWidgetsWaitForLoad)
    public bool CreatingMediaWidgets =>
        m_loadingTiltModels75 != null ||
        m_loadingTiltImages75 != null ||
        m_loadingTiltVideos != null;

    // Smaller is better. Invalid objects get negative values.
    static float ScoreByAngleAndDistance(GrabWidgetData data) {
      if (!data.m_WidgetScript.Showing) { return -1; }
      Transform source = ViewpointScript.Head;
      Transform dest = data.m_WidgetObject.transform;
      Vector3 delta = (dest.position - source.position);
      float dist = delta.magnitude;
      return dist / Vector3.Dot(delta.normalized, source.forward);
    }
  }
}
