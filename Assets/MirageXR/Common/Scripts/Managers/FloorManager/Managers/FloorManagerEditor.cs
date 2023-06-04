using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using Action = System.Action;

public class FloorManagerEditor : FloorManagerBase
{
    private GameObject _prefabPlane;
    private GameObject _prefabAnchor;
    private EditorPlaneBehaviour _plane;
    private Vector3 _floorLevel;
    private bool _enableColliders;
    private bool _showPlanes;
    private bool _isFloorDetected;
    private GameObject _anchor;

    public override bool enableColliders => _enableColliders;

    public override bool showPlanes => _showPlanes;

    public override float floorLevel => _floorLevel.y;

    public override bool isFloorDetected => _isFloorDetected;

    public GameObject prefabPlane
    {
        get => _prefabPlane;
        set => _prefabPlane = value;
    }

    public GameObject prefabAnchor
    {
        get => _prefabAnchor;
        set => _prefabAnchor = value;
    }

    public override Task<bool> InitializationAsync()
    {
        _isFloorDetected = false;
        EventManager.OnEditModeChanged += OnEditModeChanged;
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        _floorLevel = Vector3.zero;
        _isFloorDetected = false;
        _onFloorDetected = null;
        return Task.FromResult(true);
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_anchor)
        {
            Destroy(_anchor);
            _anchor = null;
        }

        _anchor = Instantiate(_prefabAnchor, pose.position, pose.rotation);
        return _anchor.transform;
    }

    public override void SetFloor(IPlaneBehaviour floor)
    {
        var editorFloor = floor as EditorPlaneBehaviour;

        _floorLevel = editorFloor.transform.position;

        _isFloorDetected = true;

        _onFloorDetected?.Invoke();
        UpdatePlanes();
    }

    public override void EnableFloorDetection(Action onFloorDetected)
    {
        if (!_plane)
        {
            var obj = Instantiate(_prefabPlane);
            _plane = obj.GetComponent<EditorPlaneBehaviour>();
        }

        _enableColliders = true;
        _onFloorDetected = onFloorDetected;

        UpdatePlanes();
        _onFloorDetected = onFloorDetected;
    }

    public override void DisableFloorDetection()
    {
        _onFloorDetected = null;
        _enableColliders = false;
        UpdatePlanes();
    }

    public override void Dispose()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        _showPlanes = value;
        UpdatePlanes();
    }

    private void UpdatePlanes()
    {
        if (_plane)
        {
            _plane.UpdateState();
        }
    }
}
