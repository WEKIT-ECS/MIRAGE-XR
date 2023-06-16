using System;
using MirageXR;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GridLine : MonoBehaviour
{
    private static GridManager gridManager => RootObject.Instance.gridManager;

    [Serializable]
    private enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
    }

    [SerializeField] private Axis _axis;

    private LineRenderer _lineRenderer;
    private NativeArray<Vector3> _points;

    private void Awake()
    {
        _points = new NativeArray<Vector3>(6, Allocator.Persistent);
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPositions(_points);
    }

    public void Draw(Vector3 position)
    {
        var localPosition = gridManager.grid.transform.InverseTransformPoint(position);
        switch (_axis)
        {
            case Axis.X:
                UpdateXPoints(localPosition);
                break;
            case Axis.Y:
                UpdateYPoints(localPosition);
                break;
            case Axis.Z:
                UpdateZPoints(localPosition);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _lineRenderer.SetPositions(_points);
    }

    private void UpdateXPoints(Vector3 localPosition)
    {
        _points[0] = new Vector3(localPosition.x, localPosition.y, localPosition.z);
        _points[1] = new Vector3(localPosition.x, 0, localPosition.z);
        _points[2] = new Vector3(localPosition.x, 0, 0);
        _points[3] = new Vector3(0, 0, 0);
        _points[4] = new Vector3(0, 0, localPosition.z);
        _points[5] = new Vector3(localPosition.x, 0, localPosition.z);
    }

    private void UpdateYPoints(Vector3 localPosition)
    {
        _points[0] = new Vector3(localPosition.x, localPosition.y, localPosition.z);
        _points[1] = new Vector3(localPosition.x, localPosition.y, 0);
        _points[2] = new Vector3(0, localPosition.y, 0);
        _points[3] = new Vector3(0, 0, 0);
        _points[4] = new Vector3(localPosition.x, 0, 0);
        _points[5] = new Vector3(localPosition.x, localPosition.y, 0);
    }

    private void UpdateZPoints(Vector3 localPosition)
    {
        _points[0] = new Vector3(localPosition.x, localPosition.y, localPosition.z);
        _points[1] = new Vector3(0, localPosition.y, localPosition.z);
        _points[2] = new Vector3(0, localPosition.y, 0);
        _points[3] = new Vector3(0, 0, 0);
        _points[4] = new Vector3(0, 0, localPosition.z);
        _points[5] = new Vector3(0, localPosition.y, localPosition.z);
    }

    private void OnDestroy()
    {
        _points.Dispose();
    }
}
