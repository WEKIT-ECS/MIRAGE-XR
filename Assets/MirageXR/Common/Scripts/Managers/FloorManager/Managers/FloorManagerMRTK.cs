using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Input;
using MirageXR;
using UnityEngine;
using Action = System.Action;

public class FloorManagerMRTK : FloorManagerBase
{
    private GameObject _anchor;
    private GlobalClickListener _globalClickListener;
    private Action _onFloorDetected;
    private MRTKPlaneBehavior _floorPoint;
    private int _planeLayer;

    public override bool enableColliders => false;

    public override bool showPlanes => false;

    public override float floorLevel => _floorPoint ? _floorPoint.GetPosition().y : _DEFAULT_FLOOR_LEVEL;

    public override bool isFloorDetected => _floorPoint != null;

    public override Task<bool> InitializationAsync()
    {
        _planeLayer = LayerMask.NameToLayer("Spatial Awareness");
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        return Task.FromResult(true);
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_anchor)
        {
            Destroy(_anchor);
            _anchor = null;
        }

        _anchor = new GameObject("ARAnchor");
        _anchor.SetPose(pose);

        return _anchor.transform;
    }

    public override void SetFloor(IPlaneBehaviour floor)
    {
        if (_floorPoint)
        {
            Destroy(_floorPoint);
        }

        _floorPoint = (MRTKPlaneBehavior)floor;
        _onFloorDetected?.Invoke();
    }

    public override void EnableFloorDetection(Action onFloorDetected)
    {
        _onFloorDetected = onFloorDetected;

        if (!_globalClickListener)
        {
            _globalClickListener = gameObject.AddComponent<GlobalClickListener>();
        }

        _globalClickListener.onClickEvent.AddListener(OnClicked);
    }

    public override void DisableFloorDetection()
    {
        _globalClickListener.onClickEvent.RemoveAllListeners();
        if (_globalClickListener)
        {
            Destroy(_globalClickListener);
        }
    }

    public override void Dispose()
    {
        if (_floorPoint)
        {
            Destroy(_floorPoint);
        }

        if (_globalClickListener)
        {
            Destroy(_globalClickListener);
        }
    }

    private void OnClicked(InputEventData eventData)
    {
        if (eventData?.InputSource?.Pointers == null)
        {
            return;
        }

        foreach (var pointer in eventData.InputSource.Pointers)
        {
            if (pointer?.Result?.CurrentPointerTarget == null)
            {
                continue;
            }

            var layer = pointer.Result.CurrentPointerTarget.layer;

            if (layer != _planeLayer)
            {
                continue;
            }

            if (pointer.Result?.Details == null)
            {
                continue;
            }

            var position = pointer.Result.Details.Point;
            var obj = new GameObject("floorPoint");
            var floorPoint = obj.AddComponent<MRTKPlaneBehavior>();
            floorPoint.transform.position = position;
            SetFloor(floorPoint);
        }
    }
}
