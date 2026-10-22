using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using UnityEngine;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance != null ? RootObject.Instance.FloorManager : null;

    private static ICalibrationManager calibrationManager => RootObject.Instance != null ? RootObject.Instance.CalibrationManager : null;

    [SerializeField] private PlacementGrid _gridPrefab;
    [SerializeField] private GridLines _gridLinesPrefab;
    [SerializeField] private Material _ghostMaterial;

    private PlacementGrid _grid;
    private ManipulationController _manipulationController;
    private bool _gridEnabled = false;
    private bool _snapEnabled = false;
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

    public bool gridShown => _grid.gameObject.activeInHierarchy;

    public bool gridEnabled => _gridEnabled;

    public bool snapEnabled => _snapEnabled;

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
        _gridEnabled = LearningExperienceEngine.UserSettings.showGrid;
        _snapEnabled = LearningExperienceEngine.UserSettings.snapToGrid;
        _showMesh = LearningExperienceEngine.UserSettings.showMesh;
        _cellWidth = LearningExperienceEngine.UserSettings.gridCellWidth;
        _angleStep = LearningExperienceEngine.UserSettings.gridAngleStep;
        _scaleStep = LearningExperienceEngine.UserSettings.gridScaleStep;
        _showOriginalObject = LearningExperienceEngine.UserSettings.gridShowOriginalObject;

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

        if (_gridEnabled && IsEditModeActive())
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
            float floorY = floorManager != null ? floorManager.floorLevel : -1.25f;
            calibrationManager.Anchor.position = new Vector3(0, floorY, 0);
            calibrationManager.Anchor.rotation = Quaternion.identity;
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
        LearningExperienceEngine.UserSettings.showGrid = _gridEnabled;

        if (IsEditModeActive())
        {
            ShowGrid();
        }
        UpdateMeshVisibility();
    }

    public void DisableGrid()
    {
        _gridEnabled = false;
        LearningExperienceEngine.UserSettings.showGrid = _gridEnabled;

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
        bool shouldDisplay = isEditMode && _gridEnabled && _showMesh;
        if (RootObject.Instance?.PointCloudManager != null)
        {
            RootObject.Instance.PointCloudManager.SetMeshActive(shouldDisplay);
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
            if (_gridEnabled)
            {
                ShowGrid();
            }
        }
        else
        {
            HideGrid();
        }
        UpdateMeshVisibility();
    }
}