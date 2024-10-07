using System.Collections.Generic;
using MirageXR;
using UnityEngine;

/// <summary>
/// Handles the aura placement
/// Follows a target object and places the aura underneath its
/// </summary>
public class UIOrigin : MonoBehaviour
{
    private static FloorManagerWithFallback floorManager => RootObject.Instance.floorManagerWithRaycastFallback;

    // the object that the placement should follow
    private Transform _followTarget;

    private Vector3 _currentPosition;

    // upwards offset so that the aura does not clip through the floor
    private float _widgetOffset = 0.03f;

    /// <summary>
    /// Singleton instance of the placement script
    /// </summary>
    public static UIOrigin Instance { get; private set; }

    // Sets the main camera as the target that the aura should follow
    private void Awake()
    {
        Instance = this;

        // Attach mixed reality camera as the follow target.
        _followTarget = Camera.main.transform;
    }

    // Updates the position of the aura
    // Calculates the floor height and puts the aura onto the floor
    private void Update()
    {
        _currentPosition.x = _followTarget.position.x;
        _currentPosition.z = _followTarget.position.z;
		_currentPosition.y = floorManager.GetFloorHeight(_followTarget.position) + _widgetOffset;

		transform.position = _currentPosition;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_followTarget.forward, Vector3.up).normalized, Vector3.up);
    }
}
