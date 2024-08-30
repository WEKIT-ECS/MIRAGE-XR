using System;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using MirageXR;
using UnityEngine;

public class ManipulationController : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;

    private GridManager _gridManager;
    private GridLines _gridLines;
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

    public Action<GameObject> onManipulationStarted => _onManipulationStarted;

    public Action<GameObject> onManipulationEnded => _onManipulationEnded;

    public Action<GameObject> onRotateStarted => _onRotateStarted;

    public Action<GameObject> onRotateStopped => _onRotateStopped;

    public Action<GameObject> onScaleStarted => _onScaleStarted;

    public Action<GameObject> onScaleStopped => _onScaleStopped;

    public Action<GameObject> onTranslateStarted => _onTranslateStarted;

    public Action<GameObject> onTranslateStopped => _onTranslateStopped;

    public void Initialization(GridManager gridManager, GridLines gridLinesPrefab)
    {
        _gridManager = gridManager;

        _gridLines = Instantiate(gridLinesPrefab);

        _gridLines.transform.SetParent(_gridManager.grid.transform);
        _gridLines.transform.localPosition = Vector3.zero;
        _gridLines.transform.localRotation = Quaternion.identity;

        _onManipulationStarted = OnManipulationStarted;
        _onManipulationEnded = OnManipulationEnded;
        _onRotateStarted = OnRotateStarted;
        _onRotateStopped = OnRotateStopped;
        _onScaleStarted = OnScaleStarted;
        _onScaleStopped = OnScaleStopped;
        _onTranslateStarted = OnTranslateStarted;
        _onTranslateStopped = OnTranslateStopped;

        HideGridLines();

        EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnRotateStarted(GameObject source)
    {
        OnManipulationStarted(source);
    }

    private void OnRotateStopped(GameObject source)
    {
        OnManipulationEnded(source);
    }

    private void OnScaleStarted(GameObject source)
    {
        OnManipulationStarted(source);
    }

    private void OnScaleStopped(GameObject source)
    {
        OnManipulationEnded(source);
    }

    private void OnTranslateStarted(GameObject source)
    {
       OnManipulationStarted(source);
    }

    private void OnTranslateStopped(GameObject source)
    {
        OnManipulationEnded(source);
    }

    private void OnManipulationStarted(GameObject source)
    {
        if (!_gridManager.snapEnabled || !RootObject.Instance.FloorManager.isFloorDetected)
        {
            return;
        }

        _manipulationStarted = true;
        CreateCopy(source);
        RunCopyUpdateCoroutine(source);
        ShowGridLines(_copy);

        if (!_gridManager.showOriginalObject)
        {
            HideOriginalObject(source);
        }
    }

    private void OnManipulationUpdated(GameObject source)
    {
        if (!_gridManager.snapEnabled || !RootObject.Instance.FloorManager.isFloorDetected)
        {
            return;
        }

        if (_copy)
        {
            UpdateCopyPosition(source);
            SnapToGrid(_copy);
            UpdateGridLines(_copy);
        }
    }

    private void OnManipulationEnded(GameObject source)
    {
        if (!_manipulationStarted)
        {
            return;
        }

        ShowOriginalObject(source);
        StopObjectUpdateCoroutine();
        SnapToGrid(source);
        HideCopy();
        HideGridLines();

        _manipulationStarted = false;
    }

    private IEnumerator OnManipulationUpdatedCoroutine(GameObject source)
    {
        if (!_copy)
        {
            yield break;
        }

        while (true)
        {
            OnManipulationUpdated(source);
            yield return null;
        }
    }

    private void RunCopyUpdateCoroutine(GameObject source)
    {
        StopObjectUpdateCoroutine();
        _copyUpdateCoroutine = StartCoroutine(OnManipulationUpdatedCoroutine(source));
    }

    private void StopObjectUpdateCoroutine()
    {
        if (_copyUpdateCoroutine != null)
        {
            StopCoroutine(_copyUpdateCoroutine);
            _copyUpdateCoroutine = null;
        }
    }

    private void ShowOriginalObject(GameObject source)
    {
        var detectable = source.GetComponent<DetectableBehaviour>();
        if (detectable)
        {
            source = detectable.AttachedObject;
        }

        var renderers = source.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            meshRenderer.enabled = true;
        }

        var skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshRenderer in skinnedRenderers)
        {
            meshRenderer.enabled = true;
        }
    }

    private void HideOriginalObject(GameObject source)
    {
        var detectable = source.GetComponent<DetectableBehaviour>();
        if (detectable)
        {
            source = detectable.AttachedObject;
        }

        var renderers = source.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            meshRenderer.enabled = false;
        }

        var skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshRenderer in skinnedRenderers)
        {
            meshRenderer.enabled = false;
        }
    }

    private void CreateCopy(GameObject source)
    {
        const string helpGameObjectName = "rigRoot";
        const string copyObjectName = "CopyObject";

        var detectable = source.GetComponent<DetectableBehaviour>();
        if (detectable)
        {
            source = detectable.AttachedObject;
        }

        var copyID = source.gameObject.GetInstanceID();
        if (_copy == null || _copyID != copyID)
        {
            Destroy(_copy);
            _copy = Instantiate(source);
            _copy.name = copyObjectName;
            _copy.SetPose(source.GetPose());
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
                    materials[i] = _gridManager.ghostMaterial;
                }

                render.materials = materials;
            }

            var skinnedRenderers = _copy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var render in skinnedRenderers)
            {
                var materials = new Material[render.materials.Length];

                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = _gridManager.ghostMaterial;
                }

                render.materials = materials;
            }
        }

        _copy.SetActive(true);
    }

    private void ShowGridLines(GameObject source)
    {
        _gridLines.gameObject.SetActive(true);
        UpdateGridLines(source);
    }

    private void UpdateGridLines(GameObject source)
    {
        var bounds = source.GetComponent<BoundsControl>();
        var position = source.transform.position;

        if (_gridManager.useObjectCenter)
        {
            if (bounds && bounds.TargetBounds)
            {
                position = bounds.transform.TransformPoint(bounds.TargetBounds.center);
            }
        }

        _gridLines.DrawLines(CalculateSnapPosition(position));
    }

    private void HideGridLines()
    {
        _gridLines.gameObject.SetActive(false);
    }

    private void UpdateCopyPosition(GameObject source)
    {
        if (_copy)
        {
            _copy.SetPose(source.GetPose());
            _copy.transform.localScale = source.transform.lossyScale;
        }
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
        source.transform.rotation = CalculateSnapRotation(source.transform.rotation);
        source.transform.localScale = CalculateSnapScale(source.transform.localScale);
    }

    private Vector3 GetSnapPosition(GameObject source)
    {
        var delta = Vector3.zero;
        var position = source.transform.position;

        if (_gridManager.useObjectCenter)
        {
            var bounds = source.GetComponent<BoundsControl>();
            if (bounds && bounds.TargetBounds)
            {
                position = bounds.transform.TransformPoint(bounds.TargetBounds.center);
                delta = source.transform.position - position;
            }
        }

        position.y = Mathf.Clamp(position.y, floorManager.floorLevel, float.PositiveInfinity);

        return CalculateSnapPosition(position) + delta;
    }

    private Vector3 CalculateSnapPosition(Vector3 position)
    {
        var point = _gridManager.grid.transform.InverseTransformPoint(position);
        return _gridManager.grid.transform.TransformPoint(MirageXR.Utilities.ToClosestToStepVector3(point, _gridManager.cellWidth / 100f));
    }

    private Quaternion CalculateSnapRotation(Quaternion rotation)
    {
        var parentRotation = _gridManager.grid.transform.parent ? _gridManager.grid.transform.parent.rotation : Quaternion.identity;
        var localRotation = Quaternion.Inverse(parentRotation) * rotation;
        localRotation = Quaternion.Euler(MirageXR.Utilities.ToClosestToStepVector3(localRotation.eulerAngles, _gridManager.angleStep));
        return parentRotation * localRotation;
    }

    private Vector3 CalculateSnapScale(Vector3 scale)
    {
        return MirageXR.Utilities.ToClosestToStepVector3(scale, _gridManager.scaleStep / 100f);
    }

    public void Dispose()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        if (!value)
        {
            HideGridLines();
            if (_copy)
            {
                Destroy(_copy);
                _copy = null;
            }
        }
    }
}
