using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public interface IFloorManager
{
    public event Action<float> OnFloorLevelChanged;

    public float floorLevel { get; }

    public bool isFloorDetected { get; }

    public bool isManualFloor { get; }

    public Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager);

    public Task<bool> ResetAsync();

    public Transform CreateAnchor(Pose pose);

    public void SetFloor(PlaneId planeId, Vector3 position);

    public void ForceDetectFloor();
}
