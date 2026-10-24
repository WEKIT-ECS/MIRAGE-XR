using MirageXR;
using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] private GridPlane[] _gridPlanes;
    [SerializeField] private float _cellWidth;

    private Transform _cameraTransform;
    private Vector3 _lastPostion;

    private void Update()
    {
        if (IsPositionsUpdated())
        {
            UpdatePosition();
        }
    }

    public void Initialization(float cellWidth)
    {
        foreach (var gridPlane in _gridPlanes)
        {
            gridPlane.Initialization();
        }

        _cellWidth = cellWidth;
        _cameraTransform = RootObject.Instance.BaseCamera.transform;
        UpdateCellWidth();
    }

    public void SetCellWidth(float value)
    {
        _cellWidth = value;
        UpdateCellWidth();
    }

    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        if (_cellWidth <= 0f) return position;
        var point = transform.InverseTransformPoint(position);
        return transform.TransformPoint(MirageXR.Utilities.ToClosestToStepVector3(point, _cellWidth / 100f));
    }

    private bool IsPositionsUpdated()
    {
        if (_cameraTransform != null) return _lastPostion != _cameraTransform.position;
#if UNITY_EDITOR
        Debug.LogWarning("CameraTransform is null in PlacementGrid.IsPositionsUpdated!");
#endif
        return false;
    }

    private void UpdatePosition()
    {
        if (!transform.parent)
        {
            return;
        }

        var cameraPosition = _cameraTransform.position;
        var localCameraPosition = transform.parent.InverseTransformPoint(cameraPosition);
        var scale = transform.localScale;
        var position = transform.position;

        scale.x = localCameraPosition.x < 0 ? -1f : 1f;
        scale.y = 1f;
        scale.z = localCameraPosition.z < 0 ? -1f : 1f;

        transform.localScale = scale;
        _lastPostion = position;
    }

    private void UpdateCellWidth()
    {
        foreach (var gridPlane in _gridPlanes)
        {
            gridPlane.SetCellWidth(_cellWidth);
        }
    }
}
