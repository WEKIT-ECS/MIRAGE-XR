using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using MirageXR;
using UnityEngine;

public class GridManager : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    [SerializeField] private Grid _gridPrefab;
    [SerializeField] private Material _ghostMaterial;

    private Grid _grid;
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
    private Action<ManipulationEventData> _onManipulationStarted;
    private Action<ManipulationEventData> _onManipulationEnded;

    public List<string> optionsCellSize => _optionsCellSize;

    public List<float> valuesCellSize => _valuesCellSize;

    public List<string> optionsAngleStep => _optionsAngleStep;

    public List<float> valuesAngleStep => _valuesAngleStep;

    public List<string> optionsScaleStep => _optionsScaleStep;

    public List<float> valuesScaleStep => _valuesScaleStep;

    public bool gridShown => _grid.gameObject.activeInHierarchy;

    public bool gridEnabled => _gridEnabled;

    public bool snapEnabled => _snapEnabled;

    public float cellWidth => _cellWidth;

    public float angleStep => _angleStep;

    public float scaleStep => _scaleStep;

    public Action<ManipulationEventData> onManipulationStarted => _onManipulationStarted;

    public Action<ManipulationEventData> onManipulationEnded => _onManipulationEnded;

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

        _onManipulationStarted = OnManipulationStarted;
        _onManipulationEnded = OnManipulationEnded;

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

    private static float ToClosestValue(float value, float step)
    {
        var stepInMeters = step;
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

    private void OnManipulationStarted(ManipulationEventData eventData)
    {
        if (!gridShown || !gridEnabled || !snapEnabled)
        {
            return;
        }

        var source = eventData.ManipulationSource;
        CreateCopy(source);
        RunCopyUpdateCoroutine(eventData);
    }

    private void OnManipulationUpdated(ManipulationEventData eventData)
    {
        if (!gridShown || !gridEnabled || !snapEnabled)
        {
            return;
        }

        var source = eventData.ManipulationSource;
        _copy.SetPose(source.GetPose());
        _copy.transform.localScale = source.transform.lossyScale;
        SnapToGrid(_copy);
    }

    private void OnManipulationEnded(ManipulationEventData eventData)
    {
        if (!gridShown || !gridEnabled || !snapEnabled)
        {
            return;
        }

        var source = eventData.ManipulationSource;
        StopObjectUpdateCoroutine();
        SnapToGrid(source);
        HideCopy();
    }

    private IEnumerator OnManipulationUpdatedCoroutine(ManipulationEventData eventData)
    {
        if (!_copy)
        {
            yield break;
        }

        while (true)
        {
            OnManipulationUpdated(eventData);
            yield return null;
        }
    }

    private void RunCopyUpdateCoroutine(ManipulationEventData eventData)
    {
        StopObjectUpdateCoroutine();
        _copyUpdateCoroutine = StartCoroutine(OnManipulationUpdatedCoroutine(eventData));
    }

    private void StopObjectUpdateCoroutine()
    {
        if (_copyUpdateCoroutine != null)
        {
            StopCoroutine(_copyUpdateCoroutine);
            _copyUpdateCoroutine = null;
        }
    }

    private void CreateCopy(GameObject source)
    {
        const string helpGameObjectName = "rigRoot";
        const string CopyObjectName = "CopyObject";

        var copyID = source.gameObject.GetInstanceID();
        if (_copy == null || _copyID != copyID)
        {
            Destroy(_copy);
            _copy = Instantiate(source);
            _copy.name = CopyObjectName;
            _copyID = copyID;

            var helpGameObject = _copy.transform.Find(helpGameObjectName);
            if (helpGameObject)
            {
                Destroy(helpGameObject.gameObject);
            }

            var monoBehaviour = _copy.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var behaviour in monoBehaviour)
            {
                behaviour.enabled = false;
            }

            var renderers = _copy.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var render in renderers)
            {
                var materials = new Material[render.materials.Length];

                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = _ghostMaterial;
                }

                render.materials = materials;
            }
        }

        _copy.SetActive(true);
    }

    private void HideCopy()
    {
        if (_copy)
        {
            _copy.SetActive(false);
        }
    }

    private void SnapToGrid(GameObject source)
    {
        source.transform.position = GetSnapPosition(source);
    }

    private Vector3 GetSnapPosition(GameObject source)
    {
        var delta = Vector3.zero;
        var bounds = source.GetComponent<BoundsControl>();
        var position = source.transform.position;

        if (bounds)
        {
            position = bounds.transform.TransformPoint(bounds.TargetBounds.center);
            delta = bounds.transform.position - position;
        }

        position.y = Mathf.Clamp(position.y, floorManager.floorLevel, float.PositiveInfinity);

        return CalculateSnapPosition(position) + delta;
    }

    private Vector3 CalculateSnapPosition(Vector3 position)
    {
        var point = _grid.transform.InverseTransformPoint(position);
        point.x = ToClosestValue(point.x, _cellWidth / 100f);
        point.y = ToClosestValue(point.y, _cellWidth / 100f);
        point.z = ToClosestValue(point.z, _cellWidth / 100f);
        return _grid.transform.TransformPoint(point);
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