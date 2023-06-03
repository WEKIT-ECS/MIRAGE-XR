using System;
using MirageXR;
using UnityEngine;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    [SerializeField] private Grid _gridPrefab;

    private Grid _grid;
    private bool _gridEnabled = false;
    private bool _snapEnabled = false;
    private float _cellWidth = 10f;
    private float _angleStep = 10f;
    private float _scaleStep = 10f;

    public bool gridEnabled => _gridEnabled;

    public bool snapEnabled => _snapEnabled;

    public float cellWidth => _cellWidth;

    public float angleStep => _angleStep;

    public float scaleStep => _scaleStep;

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

        _grid = Instantiate(_gridPrefab);
        HideGrid();
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

    public Vector3 GetSnapPosition(Vector3 position)
    {
        var point = _grid.transform.InverseTransformPoint(position);
        point.x = ToClosestPosition(point.x, _cellWidth);
        point.y = ToClosestPosition(point.y, _cellWidth);
        point.z = ToClosestPosition(point.z, _cellWidth);
        return _grid.transform.TransformPoint(point);
    }

    private static float ToClosestPosition(float value, float step)
    {
        var stepInMeters = step / 100f;
        var entire = (int)(value / stepInMeters);
        var residue = value % stepInMeters;
        if (residue > stepInMeters * 0.5f)
        {
            entire++;
        }

        return stepInMeters * entire;
    }

    public void EnableGrid()
    {
        _gridEnabled = true;
        DBManager.showGrid = _gridEnabled;

        if (RootObject.Instance.floorManager.isFloorDetected)
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
            if (_gridEnabled)
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