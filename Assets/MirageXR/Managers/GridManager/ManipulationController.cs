using System;
using System.Collections;
using MirageXR;
using UnityEngine;

public class ManipulationController : MonoBehaviour, IDisposable
{
    private static FloorManagerWrapper floorManager => RootObject.Instance != null ? RootObject.Instance.FloorManager : null;

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
    private bool _isHandleManipulation = false;
    private GameObject _currentSource;

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

        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
        }
    }

    private void OnRotateStarted(GameObject source)
    {
        OnManipulationStarted(source, isHandleManipulation: true);
    }

    private void OnRotateStopped(GameObject source)
    {
        OnManipulationEnded(source, snap: false);
    }

    private void OnScaleStarted(GameObject source)
    {
        OnManipulationStarted(source, isHandleManipulation: true);
    }

    private void OnScaleStopped(GameObject source)
    {
        OnManipulationEnded(source, snap: false);
    }

    private void OnTranslateStarted(GameObject source)
    {
       OnManipulationStarted(source, isHandleManipulation: false);
    }

    private void OnTranslateStopped(GameObject source)
    {
        OnManipulationEnded(source, snap: true);
    }

    private void OnManipulationStarted(GameObject source)
    {
        OnManipulationStarted(source, isHandleManipulation: false);
    }

    private void OnManipulationStarted(GameObject source, bool isHandleManipulation)
    {
        if ((!_gridManager.snapEnabled && !_gridManager.snapToSurface) || floorManager == null || !floorManager.isFloorDetected)
        {
            return;
        }

        _manipulationStarted = true;
        _isHandleManipulation = isHandleManipulation;
        _currentSource = source;
        _gridManager.BeginSurfaceSnap();
        CreateCopy(source);
        OnManipulationUpdated(source);
        RunCopyUpdateCoroutine(source);
    }

    private void OnManipulationUpdated(GameObject source)
    {
        if ((!_gridManager.snapEnabled && !_gridManager.snapToSurface) || floorManager == null || !floorManager.isFloorDetected)
        {
            return;
        }

        if (_copy)
        {
            UpdateCopyPosition(source);
            bool hasSnapTarget = SnapToGrid(_copy);
            if (hasSnapTarget)
            {
                _copy.SetActive(true);
                _gridLines.gameObject.SetActive(true);
                UpdateGridLines(_copy.transform.position);
                if (_isHandleManipulation || !_gridManager.showOriginalObject)
                {
                    HideOriginalObject(source);
                }
                else
                {
                    ShowOriginalObject(source);
                }
            }
            else if (_isHandleManipulation)
            {
                _copy.SetActive(true);
                HideGridLines();
                HideOriginalObject(source);
            }
            else
            {
                HideCopy();
                HideGridLines();
                ShowOriginalObject(source);
            }
        }
    }

    private void OnManipulationEnded(GameObject source)
    {
        OnManipulationEnded(source, snap: true);
    }

    private void OnManipulationEnded(GameObject source, bool snap)
    {
        if (!_manipulationStarted)
        {
            return;
        }

        ShowOriginalObject(source);
        StopObjectUpdateCoroutine();
        if (snap)
        {
            SnapToGrid(source);
        }
        HideCopy();
        HideGridLines();
        _gridManager.EndSurfaceSnap();

        _manipulationStarted = false;
        _isHandleManipulation = false;
        _currentSource = null;
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

        bool isStepSource = source.GetComponent<MirageXR.View.StepView>() != null && source.GetComponent<MirageXR.View.ContentView>() == null;

        var renderers = source.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            if (meshRenderer.GetComponentInParent<Canvas>() != null) continue;
            if (isStepSource && meshRenderer.GetComponentInParent<MirageXR.View.ContentView>() != null) continue;
            meshRenderer.enabled = true;
        }

        var skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshRenderer in skinnedRenderers)
        {
            if (meshRenderer.GetComponentInParent<Canvas>() != null) continue;
            if (isStepSource && meshRenderer.GetComponentInParent<MirageXR.View.ContentView>() != null) continue;
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

        bool isStepSource = source.GetComponent<MirageXR.View.StepView>() != null && source.GetComponent<MirageXR.View.ContentView>() == null;

        var renderers = source.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            if (meshRenderer.GetComponentInParent<Canvas>() != null) continue;
            if (isStepSource && meshRenderer.GetComponentInParent<MirageXR.View.ContentView>() != null) continue;
            meshRenderer.enabled = false;
        }

        var skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshRenderer in skinnedRenderers)
        {
            if (meshRenderer.GetComponentInParent<Canvas>() != null) continue;
            if (isStepSource && meshRenderer.GetComponentInParent<MirageXR.View.ContentView>() != null) continue;
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

        bool isStepSource = source.GetComponent<MirageXR.View.StepView>() != null && source.GetComponent<MirageXR.View.ContentView>() == null;

        var copyID = source.gameObject.GetInstanceID();
        if (_copy == null || _copyID != copyID)
        {
            Destroy(_copy);
            _copy = Instantiate(source);
            _copy.name = copyObjectName;
            _copy.SetPose(source.GetPose());
            _copyID = copyID;

            if (isStepSource)
            {
                var childContents = _copy.GetComponentsInChildren<MirageXR.View.ContentView>(true);
                foreach (var cv in childContents)
                {
                    if (cv != null)
                    {
                        cv.gameObject.SetActive(false);
                        Destroy(cv.gameObject);
                    }
                }
            }

            var helpGameObject = _copy.transform.Find(helpGameObjectName);
            if (helpGameObject)
            {
                Destroy(helpGameObject.gameObject);
            }

            var boundingBoxes = _copy.GetComponentsInChildren<SimpleBoundingBox>(true);
            foreach (var bb in boundingBoxes)
            {
                Destroy(bb);
            }

            var handles = _copy.GetComponentsInChildren<BoundingBoxHandle>(true);
            foreach (var h in handles)
            {
                if (h != null) Destroy(h.gameObject);
            }

            var lineRenderers = _copy.GetComponentsInChildren<LineRenderer>(true);
            foreach (var lr in lineRenderers)
            {
                if (lr != null) Destroy(lr);
            }

            var canvases = _copy.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                if (c != null)
                {
                    c.gameObject.SetActive(false);
                    Destroy(c.gameObject);
                }
            }

            var tmpTexts = _copy.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var t in tmpTexts)
            {
                if (t != null)
                {
                    var r = t.GetComponent<Renderer>();
                    if (r != null) r.enabled = false;
                    Destroy(t);
                }
            }

            var colliders = _copy.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                    Destroy(col);
                }
            }

            var rigidbodies = _copy.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rigidbodies)
            {
                if (rb != null) Destroy(rb);
            }

            var monoBehaviour = _copy.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var behaviour in monoBehaviour)
            {
                behaviour.enabled = false;
            }

            var renderers = _copy.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var render in renderers)
            {
                if (render.GetComponent<TMPro.TMP_Text>() != null ||
                    render.GetComponentInParent<Canvas>() != null ||
                    (isStepSource && render.GetComponentInParent<MirageXR.View.ContentView>() != null))
                {
                    continue;
                }

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
                if (render.GetComponentInParent<Canvas>() != null ||
                    (isStepSource && render.GetComponentInParent<MirageXR.View.ContentView>() != null))
                {
                    continue;
                }

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
        UpdateGridLines(source.transform.position);
    }

    private void UpdateGridLines(Vector3 snappedPosition)
    {
        _gridLines.DrawLines(snappedPosition);
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

            var bb = source.GetComponentInChildren<SimpleBoundingBox>();
            if (bb != null && bb.Target != null && bb.Target != source.transform)
            {
                var copyTarget = FindMatchingChild(source.transform, bb.Target, _copy.transform);
                if (copyTarget != null)
                {
                    copyTarget.localScale = bb.Target.localScale;
                    copyTarget.localRotation = bb.Target.localRotation;
                }
            }
        }
    }

    private static Transform FindMatchingChild(Transform sourceRoot, Transform target, Transform copyRoot)
    {
        if (target == null || copyRoot == null) return null;
        if (target == sourceRoot) return copyRoot;

        var stack = new System.Collections.Generic.Stack<int>();
        var curr = target;
        while (curr != null && curr != sourceRoot)
        {
            stack.Push(curr.GetSiblingIndex());
            curr = curr.parent;
        }

        if (curr == sourceRoot)
        {
            var copyCurr = copyRoot;
            while (stack.Count > 0)
            {
                var idx = stack.Pop();
                if (idx >= 0 && idx < copyCurr.childCount)
                {
                    copyCurr = copyCurr.GetChild(idx);
                }
                else
                {
                    copyCurr = null;
                    break;
                }
            }
            if (copyCurr != null) return copyCurr;
        }

        var children = copyRoot.GetComponentsInChildren<Transform>(true);
        foreach (var c in children)
        {
            if (c.name == target.name)
            {
                return c;
            }
        }

        return null;
    }

    private void HideCopy()
    {
        if (_copy)
        {
            _copy.SetActive(false);
        }
    }

    private bool SnapToGrid(GameObject source)
    {
        bool hasSnapTarget = TryGetSnapPosition(source, out Vector3 snappedPos);
        if (hasSnapTarget)
        {
            source.transform.position = snappedPos;
        }

        var refSource = _currentSource != null ? _currentSource : source;
        bool isStepView = refSource != null && refSource.GetComponent<MirageXR.View.StepView>() != null;
        if (_gridManager.snapEnabled && !isStepView)
        {
            source.transform.rotation = CalculateSnapRotation(source.transform.rotation);
            source.transform.localScale = CalculateSnapScale(source.transform.localScale);
        }

        return hasSnapTarget;
    }

    private bool TryGetSnapPosition(GameObject source, out Vector3 snappedPosition)
    {
        var delta = Vector3.zero;
        var position = source.transform.position;
        var refSource = _currentSource != null ? _currentSource : source;

        if (_gridManager.useObjectCenter && refSource != null)
        {
            var bounds = refSource.GetComponent<SimpleBoundingBox>() ?? refSource.GetComponentInChildren<SimpleBoundingBox>();
            if (bounds != null && bounds.TargetBounds != null)
            {
                var centerWorld = bounds.transform.TransformPoint(bounds.TargetBounds.center);
                delta = refSource.transform.position - centerWorld;
                position = source.transform.position - delta;
            }
        }

        Vector3 clampedForGrid = position;
        if (floorManager != null)
        {
            clampedForGrid.y = Mathf.Clamp(clampedForGrid.y, floorManager.floorLevel, float.PositiveInfinity);
        }

        Vector3 gridPos = CalculateSnapPosition(clampedForGrid);
        float distToGrid = Vector3.Distance(position, gridPos);
        float cellWidthMeters = Mathf.Max(0.01f, _gridManager.cellWidth / 100f);

        float maxSurfaceDist = _gridManager.snapEnabled
            ? Mathf.Min(distToGrid, cellWidthMeters * 0.9f)
            : Mathf.Max(0.04f, cellWidthMeters * 0.5f);

        Transform ignoreRoot = refSource != null ? refSource.transform : source.transform;
        if (_gridManager.snapToSurface &&
            _gridManager.TryGetSurfaceSnapPosition(position, maxSurfaceDist, ignoreRoot, out var surfacePos, out var surfaceDist) &&
            (!_gridManager.snapEnabled || surfaceDist <= distToGrid))
        {
            snappedPosition = surfacePos + delta;
            return true;
        }

        if (_gridManager.snapEnabled)
        {
            snappedPosition = gridPos + delta;
            return true;
        }

        snappedPosition = source.transform.position;
        return false;
    }

    private Vector3 CalculateSnapPosition(Vector3 position)
    {
        return _gridManager.grid != null ? _gridManager.grid.GetNearestPointOnGrid(position) : position;
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
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditModeChanged;
        }
    }

    private void OnEditModeChanged(bool value)
    {
        if (!value)
        {
            HideGridLines();
            _gridManager?.EndSurfaceSnap();
            if (_copy)
            {
                Destroy(_copy);
                _copy = null;
            }
        }
    }
}
