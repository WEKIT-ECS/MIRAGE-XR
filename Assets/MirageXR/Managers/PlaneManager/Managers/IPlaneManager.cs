using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public interface IPlaneManager : IDisposable
{
    public UnityEventPlaneIdVector3 onPlaneClicked { get; }

    public UnityEventPlaneIdPlaneId onPlaneRemoved { get; }

    public Task<bool> InitializationAsync(IViewManager viewManager);

    public Task<bool> ResetAsync();

    public void OnPlaneClicked(PlaneId planeId, Vector3 position);

    public void EnablePlanes();

    public void DisablePlanes();

    public void SelectPlane(PlaneId planeId);

    public GameObject GetPlane(PlaneId planeId);

    public void UpdatePlanes();

    public GameObject GetRandomPlane();
}
