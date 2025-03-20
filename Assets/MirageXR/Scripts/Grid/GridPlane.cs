using MirageXR;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GridPlane : MonoBehaviour
{
    //[Serializable]
    //private enum Direction
    //{
    //    x,
    //    y,
    //    z
    //}

    private const float PLANE_WIDTH = 10f;

    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int SecondTex = Shader.PropertyToID("_SecondTex");
    private static readonly int BorderTex = Shader.PropertyToID("_BorderTex");
    private static readonly int Position = Shader.PropertyToID("_Position");

    //[SerializeField] private Direction _direction;

    private Vector3 _startLastPosition;
    private Vector3 _lastPosition;
    private Vector3 _lastScale;
    private Transform _cameraTransform;
    private MeshRenderer _renderer;
    private float _cellWidth = 10f;

    public void Initialization()
    {
        _cameraTransform = RootObject.Instance.BaseCamera.transform;
        _renderer = GetComponent<MeshRenderer>();
        UpdateCellSize();
        _startLastPosition = transform.localPosition;
    }

    public void SetCellWidth(float width)
    {
        _cellWidth = width;
        UpdateCellSize();
    }

    private void Update()
    {
        if (IsPositionsUpdated())
        {
            UpdateShaderPositionValue();
        }

        if (IsScaleUpdated())
        {
            UpdateLocalPosition();
        }
    }

    private bool IsPositionsUpdated()
    {
        return _lastPosition != _cameraTransform.position;
    }

    private bool IsScaleUpdated()
    {
        return _lastScale != transform.localScale;
    }

    private void UpdateShaderPositionValue()
    {
        var position = _cameraTransform.position;
        _renderer.material.SetVector(Position, position);
        _lastPosition = position;
    }


    private void UpdateLocalPosition()
    {
        var scale = transform.localScale;
        /*Vector3 localPosition;
        switch (_direction)
        {
            case Direction.x:
                localPosition = new Vector3(PLANE_WIDTH * 0.5f * scale.x, 0, PLANE_WIDTH * 0.5f * scale.z);
                break;
            case Direction.y:
                localPosition = new Vector3(PLANE_WIDTH * 0.5f * scale.x, PLANE_WIDTH * 0.5f * scale.z, 0);
                break;
            case Direction.z:
                localPosition = new Vector3(0, PLANE_WIDTH * 0.5f * scale.x, PLANE_WIDTH * 0.5f * scale.z);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }*/

        transform.localPosition = new Vector3(_startLastPosition.x * scale.x, _startLastPosition.y * scale.y, _startLastPosition.z * scale.z);
    }


    private void UpdateCellSize()
    {
        const float planeInCm = 1000f;

        var scale = transform.localScale;
        var tilling = new Vector2(scale.x * planeInCm / _cellWidth, scale.z * planeInCm / _cellWidth);
        var borderTilling = new Vector2(scale.x * planeInCm / 10 / _cellWidth, scale.z * planeInCm / 10f / _cellWidth);
        _renderer.material.SetTextureScale(MainTex, tilling);
        _renderer.material.SetTextureScale(SecondTex, tilling);
        _renderer.material.SetTextureScale(BorderTex, borderTilling);

        _lastScale = scale;
    }
}
