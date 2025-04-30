using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public abstract class PlaneManagerBase : MonoBehaviour, IPlaneManager
{
    public abstract UnityEventPlaneIdVector3 onPlaneClicked { get; }

    public abstract UnityEventPlaneIdPlaneId onPlaneRemoved { get; }

    public abstract Task<bool> InitializationAsync(IViewManager viewManager);

    public abstract Task<bool> ResetAsync();

    public abstract void OnPlaneClicked(PlaneId planeId, Vector3 position);

    public abstract void EnablePlanes();

    public abstract void DisablePlanes();

    public abstract void SelectPlane(PlaneId planeId);

    public abstract GameObject GetPlane(PlaneId planeId);

    public abstract void UpdatePlanes();

    public abstract GameObject GetRandomPlane();

    public abstract void Dispose();
}
