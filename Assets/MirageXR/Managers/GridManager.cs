using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using UnityEngine;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;

    private static ICalibrationManager calibrationManager => RootObject.Instance.CalibrationManager;

    [SerializeField] private Grid _gridPrefab;
    [SerializeField] private GridLines _gridLinesPrefab;
    [SerializeField] private Material _ghostMaterial;

    private Grid _grid;
    private ManipulationController _manipulationController;
    private bool _gridEnabled = false;
    private bool _snapEnabled = false;
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

    public Grid grid => _grid;

    public Material ghostMaterial => _ghostMaterial;

    public bool gridShown => _grid.gameObject.activeInHierarchy;

    public bool gridEnabled => _gridEnabled;

    public bool snapEnabled => _snapEnabled;

    public bool showOriginalObject => _showOriginalObject;

    public bool useObjectCenter => _useObjectCenter;

    public float cellWidth => _cellWidth;

    public float angleStep => _angleStep;

    public float scaleStep => _scaleStep;

    public Action<GameObject> onManipulationStarted => _manipulationController.onManipulationStarted;

    public Action<GameObject> onManipulationEnded => _manipulationController.onManipulationEnded;

    public Action<GameObject> onRotateStarted => _manipulationController.onRotateStarted;

    public Action<GameObject> onRotateStopped => _manipulationController.onRotateStopped;

    public Action<GameObject> onScaleStarted => _manipulationController.onScaleStarted;

    public Action<GameObject> onScaleStopped => _manipulationController.onScaleStopped;

    public Action<GameObject> onTranslateStarted => _manipulationController.onTranslateStarted;

    public Action<GameObject> onTranslateStopped => _manipulationController.onTranslateStopped;

    public async void Initialization()
    {
        
        var obj = await Resources.LoadAsync<AssetsBundle>("MirageXRAssetsBundle") as AssetsBundle;
        
        _gridEnabled = LearningExperienceEngine.UserSettings.showGrid;
        _snapEnabled = LearningExperienceEngine.UserSettings.snapToGrid;
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

        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    public void ShowGrid()
    {
        var anchor = calibrationManager.Anchor;

        _grid.transform.SetParent(anchor);

        _grid.transform.localPosition = Vector3.zero;
        _grid.transform.localRotation = Quaternion.identity;

        var position = _grid.transform.position;
        position.y = floorManager.floorLevel;
        _grid.transform.position = position;

        _grid.gameObject.SetActive(true);
    }

    public void HideGrid()
    {
        _grid.gameObject.SetActive(false);
    }

    public void EnableGrid()
    {
        _gridEnabled = true;
        LearningExperienceEngine.UserSettings.showGrid = _gridEnabled;

        var activityManager = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        if (floorManager.isFloorDetected && activityManager.EditModeActive)
        {
            ShowGrid();
        }
    }

    public void DisableGrid()
    {
        _gridEnabled = false;
        LearningExperienceEngine.UserSettings.showGrid = _gridEnabled;

        HideGrid();
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
        _manipulationController.Dispose();
    }

    private void OnEditModeChanged(bool value)
    {
        if (value)
        {
            if (_gridEnabled && floorManager.isFloorDetected)
            {
                ShowGrid();
            }
        }
        else
        {
            HideGrid();
        }
    }
}