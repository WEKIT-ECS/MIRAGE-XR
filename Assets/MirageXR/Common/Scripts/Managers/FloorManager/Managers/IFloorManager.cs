using System;
using System.Threading.Tasks;
using UnityEngine;

public interface IFloorManager : IDisposable
{
    public bool enableColliders
    {
        get;
    }

    public bool showPlanes
    {
        get;
    }

    public float floorLevel
    {
        get;
    }

    public bool isFloorDetected
    {
        get;
    }

    Task<bool> InitializationAsync();

    Task<bool> ResetAsync();

    Transform CreateAnchor(Pose pose);

    void SetFloor(IPlaneBehaviour floor);

    void EnableFloorDetection(Action onFloorDetected);

    void DisableFloorDetection();
}
