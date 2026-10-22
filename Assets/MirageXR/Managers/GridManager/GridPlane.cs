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
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int Surface = Shader.PropertyToID("_Surface");
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
        ResolveCamera();
        _renderer = GetComponent<MeshRenderer>();
        if (_renderer != null && _renderer.material != null)
        {
            var mat = _renderer.material;
            if (mat.HasProperty(Surface))
            {
                mat.SetFloat(Surface, 1.0f); // Ensure transparent
            }
            if (mat.HasProperty(BaseMap) && mat.HasProperty(MainTex))
            {
                var mainTex = mat.GetTexture(MainTex);
                if (mainTex != null)
                {
                    mat.SetTexture(BaseMap, mainTex);
                }
            }
            if (mat.HasProperty(BaseColor) && mat.HasProperty(ColorProperty))
            {
                mat.SetColor(BaseColor, mat.GetColor(ColorProperty));
            }
        }
        UpdateCellSize();
        _startLastPosition = transform.localPosition;
        if (_cameraTransform != null && _renderer != null)
        {
            UpdateShaderPositionValue();
        }
    }

    private void OnEnable()
    {
        ResolveCamera();
        if (_cameraTransform != null && _renderer != null)
        {
            UpdateShaderPositionValue();
        }
    }

    private void ResolveCamera()
    {
        if (_cameraTransform == null)
        {
            if (RootObject.Instance != null && RootObject.Instance.BaseCamera != null)
            {
                _cameraTransform = RootObject.Instance.BaseCamera.transform;
            }
            else if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }
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
        ResolveCamera();
        if (_cameraTransform != null) return _lastPosition != _cameraTransform.position;
        return false;
    }

    private bool IsScaleUpdated()
    {
        if (transform != null) return _lastScale != transform.localScale;
        return false;
    }

    private void UpdateShaderPositionValue()
    {
        if (_cameraTransform == null || _renderer == null || _renderer.material == null) return;
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
        if (_renderer.material.HasProperty(BaseMap))
        {
            _renderer.material.SetTextureScale(BaseMap, tilling);
        }
        _renderer.material.SetTextureScale(SecondTex, tilling);
        _renderer.material.SetTextureScale(BorderTex, borderTilling);

        _lastScale = scale;
    }
}
