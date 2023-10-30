using System.Threading.Tasks;
using UnityEngine;

public interface IFloorManager
{
    public float floorLevel { get; }

    public bool isFloorDetected { get; }

    public Task<bool> InitializationAsync();

    public Task<bool> ResetAsync();

    public Transform CreateAnchor(Pose pose);

    public void SetFloor(PlaneId planeId, Vector3 position);
}
