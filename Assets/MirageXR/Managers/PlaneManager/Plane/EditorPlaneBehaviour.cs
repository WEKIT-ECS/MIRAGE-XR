using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer))]
public class EditorPlaneBehaviour : MonoBehaviour, IPlaneBehaviour, IMixedRealityPointerHandler
{
    private static bool _isActive = false;
    private static bool _isCollidersEnabled = false;
    private static Action<PlaneId, Vector3> _onClicked;
    private static PlaneId _specialPlaneId = PlaneId.InvalidId;

    [SerializeField] private Color _colorDefault = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color _colorFloor = new Color(0.03f, 1f, 0.09f, 0.7f);

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;
    private PlaneId _planeId = new PlaneId(1, 1);

    public static void SetSelectedPlane(PlaneId planeId)
    {
        _specialPlaneId = planeId;
    }

    public static void UpdatePlanesState(bool isActive, bool isCollidersEnabled, Action<PlaneId, Vector3> onClicked)
    {
        _isActive = isActive;
        _isCollidersEnabled = isCollidersEnabled;
        _onClicked = onClicked;
    }

    public void UpdateState()
    {
        _meshCollider.enabled = _isCollidersEnabled;

        if (_planeId != _specialPlaneId)
        {
            gameObject.SetActive(_isActive);
            _meshRenderer.material.color = _colorDefault;
        }
        else
        {
            gameObject.SetActive(true);
            _meshRenderer.material.color = _colorFloor;
        }
    }

    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UpdateState();
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerUp(MixedRealityPointerEventData eventData) { }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        _onClicked?.Invoke(_planeId, eventData.Pointer.Result.Details.Point);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
