using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance != null ? RootObject.Instance.FloorManager : null;

    private static ICalibrationManager calibrationManager => RootObject.Instance != null ? RootObject.Instance.CalibrationManager : null;

    [SerializeField] private PlacementGrid _gridPrefab;
    [SerializeField] private GridLines _gridLinesPrefab;
    [SerializeField] private Material _ghostMaterial;

    private PlacementGrid _grid;
    private ManipulationController _manipulationController;
    private bool _gridEnabled = true;
    private bool _snapEnabled = true;
    private bool _snapToSurface = false;
    private bool _showMesh = true;
    private bool _showOriginalObject = false;
    private bool _useObjectCenter = false;
    private float _cellWidth = 10f;
    private float _angleStep = 10f;
    private float _scaleStep = 10f;

    private readonly List<string> _optionsCellSize = new List<string> { "1 cm", "5 cm", "6.25 cm", "10 cm", "12.5 cm", "15 cm", "25 cm", "50 cm", "1 m" };
    private readonly List<float> _valuesCellSize = new List<float> { 1f, 5f, 6.25f, 10f, 12.5f, 15f, 25f, 50f, 100f };
    private readonly List<string> _optionsAngleStep = new List<string> { "5°", "10°", "15°", "30°", "45°", "90°" };
    private readonly List<float> _valuesAngleStep = new List<float> { 5f, 10f, 15f, 30f, 45f, 90f };
    private readonly List<string> _optionsScaleStep = new List<string> { "5%", "10%", "15%", "30%" };
    private readonly List<float> _valuesScaleStep = new List<float> { 5f, 10f, 15f, 30f };

    private GameObject _copy;
    private int _copyID;
    private Coroutine _copyUpdateCoroutine;
    private Action<GameObject> _onManipulationStarted;
    private Action<GameObject> _onManipulationEnded;
    private Action<GameObject> _onRotateStarted;
    private Action<GameObject> _onRotateStopped;
    private Action<GameObject> _onScaleStarted;
    private Action<GameObject> _onScaleStopped;
    private Action<GameObject> _onTranslateStarted;
    private Action<GameObject> _onTranslateStopped;
    private bool _manipulationStarted = false;

    public List<string> optionsCellSize => _optionsCellSize;

    public List<float> valuesCellSize => _valuesCellSize;

    public List<string> optionsAngleStep => _optionsAngleStep;

    public List<float> valuesAngleStep => _valuesAngleStep;

    public List<string> optionsScaleStep => _optionsScaleStep;

    public List<float> valuesScaleStep => _valuesScaleStep;

    public PlacementGrid grid => _grid;

    public Material ghostMaterial => _ghostMaterial;

    public bool gridShown => _grid != null && _grid.gameObject.activeInHierarchy;

    public bool gridEnabled => _gridEnabled;

    public bool snapEnabled => _snapEnabled;

    public bool snapToSurface => _snapToSurface;

    public bool showMesh => _showMesh;

    public bool showOriginalObject => _showOriginalObject;

    public bool useObjectCenter => _useObjectCenter;

    public float cellWidth => _cellWidth;

    public float angleStep => _angleStep;

    public float scaleStep => _scaleStep;

    public Action<GameObject> onManipulationStarted => _manipulationController?.onManipulationStarted;

    public Action<GameObject> onManipulationEnded => _manipulationController?.onManipulationEnded;

    public Action<GameObject> onRotateStarted => _manipulationController?.onRotateStarted;

    public Action<GameObject> onRotateStopped => _manipulationController?.onRotateStopped;

    public Action<GameObject> onScaleStarted => _manipulationController?.onScaleStarted;

    public Action<GameObject> onScaleStopped => _manipulationController?.onScaleStopped;

    public Action<GameObject> onTranslateStarted => _manipulationController?.onTranslateStarted;

    public Action<GameObject> onTranslateStopped => _manipulationController?.onTranslateStopped;

    public void Initialization()
    {
        UnityEngine.Debug.Log("Initializing [GridManager] <--");
        _gridEnabled = true;
        _snapEnabled = LearningExperienceEngine.UserSettings.snapToGrid;
        _snapToSurface = LearningExperienceEngine.UserSettings.snapToSurface;
        _showMesh = LearningExperienceEngine.UserSettings.showMesh;
        _cellWidth = LearningExperienceEngine.UserSettings.gridCellWidth;
        _angleStep = LearningExperienceEngine.UserSettings.gridAngleStep;
        _scaleStep = LearningExperienceEngine.UserSettings.gridScaleStep;
        _showOriginalObject = LearningExperienceEngine.UserSettings.gridShowOriginalObject;
        _useObjectCenter = LearningExperienceEngine.UserSettings.gridUseObjectCenter;

        if (!_gridPrefab)
        {
            Debug.Log("_gridPrefab is null");
            return;
        }

        if (!_gridLinesPrefab)
        {
            Debug.Log("_gridLinesPrefab is null");
            return;
        }

        _grid = Instantiate(_gridPrefab);
        _grid.Initialization(_cellWidth);
        HideGrid();

        _manipulationController = gameObject.AddComponent<ManipulationController>();
        _manipulationController.Initialization(this, _gridLinesPrefab);

        UpdateAnchorFallbackFloorLevel();

        if (floorManager != null)
        {
            floorManager.OnFloorLevelChanged += OnFloorLevelChanged;
        }

        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
        }

        if (IsEditModeActive())
        {
            ShowGrid();
        }

        UpdateMeshVisibility();

        UnityEngine.Debug.Log("Initializing [GridManager] -->");
    }

    public bool IsEditModeActive()
    {
        bool editMode = false;
        if (LearningExperienceEngine.LearningExperienceEngine.Instance?.ActivityManagerOld != null)
        {
            editMode = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;
        }

        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            editMode |= RootObject.Instance.LEE.ActivityManager.IsEditorMode;
        }

        return editMode;
    }

    private void UpdateAnchorFallbackFloorLevel()
    {
        if (calibrationManager != null && !calibrationManager.IsCalibrated && calibrationManager.Anchor != null)
        {
            float floorY = floorManager != null ? floorManager.floorLevel : 0f;
            var pos = calibrationManager.Anchor.position;
            calibrationManager.Anchor.position = new Vector3(pos.x, floorY, pos.z);
        }
    }

    private void OnFloorLevelChanged(float floorY)
    {
        UpdateAnchorFallbackFloorLevel();
        if (_grid != null && _grid.gameObject.activeInHierarchy)
        {
            UpdateGridTransform();
        }
    }

    private void UpdateGridTransform()
    {
        if (_grid == null) return;
        var anchor = calibrationManager?.Anchor ?? transform;
        _grid.transform.SetParent(anchor);
        _grid.transform.localPosition = Vector3.zero;
        _grid.transform.localRotation = Quaternion.identity;

        if (floorManager != null)
        {
            var position = _grid.transform.position;
            position.y = floorManager.floorLevel;
            _grid.transform.position = position;
        }
    }

    public void ShowGrid()
    {
        if (_grid == null) return;

        floorManager?.ForceDetectFloor();
        UpdateAnchorFallbackFloorLevel();
        UpdateGridTransform();

        _grid.gameObject.SetActive(true);
    }

    public void HideGrid()
    {
        if (_grid != null)
        {
            _grid.gameObject.SetActive(false);
        }
    }

    public void EnableGrid()
    {
        _gridEnabled = true;
        if (IsEditModeActive())
        {
            ShowGrid();
        }
        UpdateMeshVisibility();
    }

    public void DisableGrid()
    {
        _gridEnabled = false;
        HideGrid();
        UpdateMeshVisibility();
    }

    public void SetShowMesh(bool value)
    {
        _showMesh = value;
        LearningExperienceEngine.UserSettings.showMesh = _showMesh;
        UpdateMeshVisibility();
    }

    public void UpdateMeshVisibility()
    {
        bool isEditMode = IsEditModeActive();
        bool shouldDisplay = isEditMode && _showMesh;
        if (RootObject.Instance?.PointCloudManager != null)
        {
            RootObject.Instance.PointCloudManager.SetMeshActive(shouldDisplay);
        }
        if (RootObject.Instance?.PlaneManager != null)
        {
            RootObject.Instance.PlaneManager.UpdatePlanes();
        }
    }

    public void EnableSnapToGrid()
    {
        _snapEnabled = true;
        LearningExperienceEngine.UserSettings.snapToGrid = _snapEnabled;
    }

    public void DisableSnapToGrid()
    {
        _snapEnabled = false;
        LearningExperienceEngine.UserSettings.snapToGrid = _snapEnabled;
    }

    public void SetSnapToSurface(bool value)
    {
        _snapToSurface = value;
        LearningExperienceEngine.UserSettings.snapToSurface = _snapToSurface;
    }

    public void BeginSurfaceSnap()
    {
        if (!_snapToSurface) return;
        RootObject.Instance?.PointCloudManager?.SetSurfaceCollidersActive(true);
        ARFoundationPlaneBehaviour.SetSurfaceSnapCollidersActive(true);
    }

    public void EndSurfaceSnap()
    {
        RootObject.Instance?.PointCloudManager?.SetSurfaceCollidersActive(false);
        ARFoundationPlaneBehaviour.SetSurfaceSnapCollidersActive(false);
    }

    private static readonly Vector3[] SurfaceSampleDirections = CreateSurfaceSampleDirections();

    private static Vector3[] CreateSurfaceSampleDirections()
    {
        var dirs = new List<Vector3>(26);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    dirs.Add(new Vector3(x, y, z).normalized);
                }
            }
        }
        return dirs.ToArray();
    }

    public bool TryGetSurfaceSnapPosition(Vector3 targetWorldPos, Transform ignoreRoot, out Vector3 snappedWorldPos)
    {
        float defaultMaxDist = Mathf.Max(0.04f, (_cellWidth / 100f) * 0.5f);
        return TryGetSurfaceSnapPosition(targetWorldPos, defaultMaxDist, ignoreRoot, out snappedWorldPos, out _);
    }

    public bool TryGetSurfaceSnapPosition(Vector3 targetWorldPos, float maxDistance, Transform ignoreRoot, out Vector3 snappedWorldPos, out float surfaceDistance)
    {
        snappedWorldPos = targetWorldPos;
        surfaceDistance = float.MaxValue;
        if (!_snapToSurface || maxDistance <= 0f)
        {
            return false;
        }

        const int surfaceLayerMask = (1 << 0) | (1 << 2) | (1 << 31);
        float searchRayDist = maxDistance * 1.15f;
        bool found = false;
        float bestDistSq = maxDistance * maxDistance;
        Vector3 bestPoint = targetWorldPos;
        Vector3 bestNormal = Vector3.up;

        // 1. Bilateral short-range 26-direction raycast fan centered on targetWorldPos
        for (int i = 0; i < SurfaceSampleDirections.Length; i++)
        {
            Vector3 dir = SurfaceSampleDirections[i];
            Vector3 rayOrigin = targetWorldPos - dir * searchRayDist;
            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, searchRayDist * 2f, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (IsValidSurfaceCollider(hit.collider, ignoreRoot))
                {
                    float dSq = (hit.point - targetWorldPos).sqrMagnitude;
                    if (dSq <= bestDistSq)
                    {
                        bestDistSq = dSq;
                        bestPoint = hit.point;
                        bestNormal = hit.normal;
                        found = true;
                    }
                }
            }
        }

        // 2. Short segment along camera-to-target ray centered on targetWorldPos
        var activeCamera = RootObject.Instance?.BaseCamera != null ? RootObject.Instance.BaseCamera : Camera.main;
        if (activeCamera != null)
        {
            Vector3 camToTarget = targetWorldPos - activeCamera.transform.position;
            float camDist = camToTarget.magnitude;
            if (camDist > 0.01f)
            {
                Vector3 camDir = camToTarget / camDist;
                Vector3 rayStart = targetWorldPos - camDir * maxDistance;
                var hits = Physics.RaycastAll(rayStart, camDir, maxDistance * 2f, surfaceLayerMask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (IsValidSurfaceCollider(hits[i].collider, ignoreRoot))
                    {
                        float dSq = (hits[i].point - targetWorldPos).sqrMagnitude;
                        if (dSq <= bestDistSq)
                        {
                            bestDistSq = dSq;
                            bestPoint = hits[i].point;
                            bestNormal = hits[i].normal;
                            found = true;
                        }
                    }
                }
            }
        }

        // 3. Refine to orthogonal surface projection along the detected triangle normal
        if (found && bestNormal.sqrMagnitude > 0.001f)
        {
            Vector3 normal = bestNormal.normalized;
            Vector3 orthoOrigin = targetWorldPos + normal * searchRayDist;
            if (Physics.Raycast(orthoOrigin, -normal, out RaycastHit orthoHit, searchRayDist * 2f, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (IsValidSurfaceCollider(orthoHit.collider, ignoreRoot))
                {
                    float dSq = (orthoHit.point - targetWorldPos).sqrMagnitude;
                    if (dSq <= bestDistSq)
                    {
                        bestDistSq = dSq;
                        bestPoint = orthoHit.point;
                    }
                }
            }
        }

        if (found)
        {
            snappedWorldPos = bestPoint;
            surfaceDistance = Mathf.Sqrt(bestDistSq);
            return true;
        }

        return false;
    }

    private static bool IsValidSurfaceCollider(Collider col, Transform ignoreRoot)
    {
        if (col == null) return false;
        if (ignoreRoot != null && (col.transform == ignoreRoot || col.transform.IsChildOf(ignoreRoot)))
        {
            return false;
        }

        if (col.GetComponentInParent<MirageXR.View.ContentView>() != null ||
            col.GetComponentInParent<MirageXR.View.StepView>() != null ||
            col.GetComponentInParent<BoundingBoxHandle>() != null ||
            col.gameObject.name == "CopyObject")
        {
            return false;
        }

        return col.GetComponent<ARPlane>() != null ||
               col.GetComponent<ARFoundationPlaneBehaviour>() != null ||
               col.GetComponentInParent<ARMeshManager>() != null ||
               col.gameObject.layer == 2 ||
               col.gameObject.layer == 31;
    }

    public void SetShowOriginalObject(bool value)
    {
        _showOriginalObject = value;
        LearningExperienceEngine.UserSettings.gridShowOriginalObject = _showOriginalObject;
    }

    public void SetUseObjectCenter(bool value)
    {
        _useObjectCenter = value;
        LearningExperienceEngine.UserSettings.gridUseObjectCenter = _useObjectCenter;
    }

    public void SetCellWidth(float value)
    {
        _cellWidth = value;
        LearningExperienceEngine.UserSettings.gridCellWidth = value;
        _grid.SetCellWidth(_cellWidth);
    }

    public void SetAngleStep(float value)
    {
        _angleStep = value;
        LearningExperienceEngine.UserSettings.gridAngleStep = value;
    }

    public void SetScaleStep(float value)
    {
        _scaleStep = value;
        LearningExperienceEngine.UserSettings.gridScaleStep = value;
    }

    public void Dispose()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditModeChanged;
        }
        if (floorManager != null)
        {
            floorManager.OnFloorLevelChanged -= OnFloorLevelChanged;
        }
        _manipulationController?.Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    private void OnEditModeChanged(bool value)
    {
        if (value)
        {
            ShowGrid();
        }
        else
        {
            HideGrid();
        }
        UpdateMeshVisibility();
    }
}