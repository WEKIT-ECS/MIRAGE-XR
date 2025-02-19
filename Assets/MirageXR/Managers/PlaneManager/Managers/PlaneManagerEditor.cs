using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class PlaneManagerEditor : PlaneManagerBase
{
    public override UnityEventPlaneIdVector3 onPlaneClicked => _onPlaneClicked;

    public override UnityEventPlaneIdPlaneId onPlaneRemoved => _onPlaneRemoved;

    private GameObject _prefabPlane;
    private EditorPlaneBehaviour _plane;
    private Vector3 _floorLevel;
    private bool _enableColliders;
    private bool _showPlanes;
    private bool _isFloorDetected;
    private GameObject _anchor;
    private UnityEventPlaneIdVector3 _onPlaneClicked = new UnityEventPlaneIdVector3();
    private UnityEventPlaneIdPlaneId _onPlaneRemoved = new UnityEventPlaneIdPlaneId();

    public GameObject prefabPlane
    {
        get => _prefabPlane;
        set => _prefabPlane = value;
    }

    public override Task<bool> InitializationAsync()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        return Task.FromResult(true);
    }

    public override void OnPlaneClicked(PlaneId planeId, Vector3 position)
    {
        _onPlaneClicked.Invoke(planeId, position);
    }

    public override void EnablePlanes()
    {
        if (!_plane)
        {
            var obj = Instantiate(_prefabPlane);
            _plane = obj.GetComponent<EditorPlaneBehaviour>();
        }

        _enableColliders = true;
        _showPlanes = true;

        UpdatePlanes();
    }

    public override void DisablePlanes()
    {
        _enableColliders = false;
        _showPlanes = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;

        UpdatePlanes();
    }

    public override void SelectPlane(PlaneId planeId)
    {
        EditorPlaneBehaviour.SetSelectedPlane(planeId);
        UpdatePlanes();
    }

    public override GameObject GetPlane(PlaneId planeId)
    {
        return _plane.gameObject;
    }

    public override void UpdatePlanes()
    {
        EditorPlaneBehaviour.UpdatePlanesState(_showPlanes, _enableColliders, OnPlaneClicked);
        if (_plane)
        {
            _plane.UpdateState();
        }
    }

    public override GameObject GetRandomPlane()
    {
        return null;
    }

    public override void Dispose()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        _showPlanes = value || _enableColliders;
        UpdatePlanes();
    }
}
