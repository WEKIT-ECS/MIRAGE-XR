using System;
using System.Collections.Generic;
using MirageXR;
using UnityEngine;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    [SerializeField] private Grid _gridPrefab;
    [SerializeField] private GridLines _gridLinesPrefab;
    [SerializeField] private Material _ghostMaterial;

    private Grid _grid;
    private ManipulationController _manipulationController;
    private bool _gridEnabled = false;
    private bool _snapEnabled = false;
    private float _cellWidth = 10f;
    private float _angleStep = 10f;
    private float _scaleStep = 10f;

    private readonly List<string> _optionsCellSize = new List<string> { "5 cm", "6.25 cm", "10 cm", "12.5 cm", "15 cm", "25 cm" };
    private readonly List<float> _valuesCellSize = new List<float> { 5f, 6.25f, 10f, 12.5f, 15f, 25f };
    private readonly List<string> _optionsAngleStep = new List<string> { "5°", "10°", "15°" };
    private readonly List<float> _valuesAngleStep = new List<float> { 5f, 10f, 15f };
    private readonly List<string> _optionsScaleStep = new List<string> { "5%", "10%", "15%" };
    private readonly List<float> _valuesScaleStep = new List<float> { 5f, 10f, 15f };

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

    public void Initialization()
    {
        _gridEnabled = DBManager.showGrid;
        _snapEnabled = DBManager.snapToGrid;
        _cellWidth = DBManager.gridCellWidth;
        _angleStep = DBManager.gridAngleStep;
        _scaleStep = DBManager.gridScaleStep;


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
        HideGrid();

        _manipulationController = gameObject.AddComponent<ManipulationController>();
        _manipulationController.Initialization(this, _gridLinesPrefab);

        _grid.Initialization(_cellWidth);

        EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    public void ShowGrid()
    {
        var anchor = calibrationManager.anchor;

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
        DBManager.showGrid = _gridEnabled;

        if (floorManager.isFloorDetected)
        {
            ShowGrid();
        }
    }

    public void DisableGrid()
    {
        _gridEnabled = false;
        DBManager.showGrid = _gridEnabled;

        HideGrid();
    }

    public void EnableSnapToGrid()
    {
        _snapEnabled = true;
        DBManager.snapToGrid = _snapEnabled;
    }

    public void DisableSnapToGrid()
    {
        _snapEnabled = false;
        DBManager.snapToGrid = _snapEnabled;
    }

    public void SetCellWidth(float value)
    {
        _cellWidth = value;
        DBManager.gridCellWidth = value;
        _grid.SetCellWidth(_cellWidth);
    }

    public void SetAngleStep(float value)
    {
        _angleStep = value;
        DBManager.gridAngleStep = value;
    }

    public void SetScaleStep(float value)
    {
        _scaleStep = value;
        DBManager.gridScaleStep = value;
    }

    public void Dispose()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
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