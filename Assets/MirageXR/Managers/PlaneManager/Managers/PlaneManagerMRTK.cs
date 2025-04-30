using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Input;
using MirageXR;
using UnityEngine;

public class PlaneManagerMRTK : PlaneManagerBase
{
    private const string SPATIAL_AWARENESS = "Spatial Awareness";

    public override UnityEventPlaneIdVector3 onPlaneClicked => _onPlaneClicked;
    public override UnityEventPlaneIdPlaneId onPlaneRemoved => _onPlaneRemoved;

    private GlobalClickListener _globalClickListener;
    private MRTKPlaneBehavior _floorPoint;
    private int _planeLayer;
    private UnityEventPlaneIdVector3 _onPlaneClicked = new UnityEventPlaneIdVector3();
    private UnityEventPlaneIdPlaneId _onPlaneRemoved = new UnityEventPlaneIdPlaneId();

    public override Task<bool> InitializationAsync(IViewManager viewManager)
    {
        _planeLayer = LayerMask.NameToLayer(SPATIAL_AWARENESS);
        _globalClickListener = gameObject.AddComponent<GlobalClickListener>();
        _globalClickListener.enabled = false;
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        return Task.FromResult(true);
    }

    public override void OnPlaneClicked(PlaneId plane, Vector3 position)
    {
        _onPlaneClicked.Invoke(plane, position);
    }

    public override void EnablePlanes()
    {
        _globalClickListener.enabled = true;
        _globalClickListener.onClickEvent.AddListener(OnClicked);
    }

    public override void DisablePlanes()
    {
        _globalClickListener.onClickEvent.RemoveAllListeners();
        _globalClickListener.enabled = false;
    }

    public override void SelectPlane(PlaneId planeId)
    {
        /* ignore */
    }

    public override GameObject GetPlane(PlaneId planeId)
    {
        return null; /* ignore */
    }

    public override void UpdatePlanes()
    {
         /* ignore */
    }

    public override GameObject GetRandomPlane()
    {
        return null;
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

            OnPlaneClicked(new PlaneId(1, 1), pointer.Result.Details.Point);
        }
    }
}
