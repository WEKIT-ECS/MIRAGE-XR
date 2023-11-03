using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane), typeof(MeshCollider), typeof(MeshRenderer))]
public class ARFoundationPlaneBehaviour : MonoBehaviour, IPlaneBehaviour, IMixedRealityPointerHandler
{
    private static readonly int _textureTintColor = Shader.PropertyToID("_TexTintColor");

    private static bool _isActive = false;
    private static bool _isCollidersEnabled = false;
    private static Action<PlaneId, Vector3> _onClicked;
    private static TrackableId _specialPlaneId = TrackableId.invalidId;

    [SerializeField] private Color _colorDefault = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color _colorFloor = new Color(0.03f, 1f, 0.09f, 0.7f);

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;
    private ARPlane _arPlane;

    public TrackableId trackableId => _arPlane.trackableId;

    public static void SetSelectedPlane(PlaneId planeId)
    {
        _specialPlaneId = new TrackableId(planeId.subId1, planeId.subId2);
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

        if (trackableId != _specialPlaneId)
        {
            gameObject.SetActive(_isActive);
            _meshRenderer.material.color = _colorDefault;
        }
        else
        {
            gameObject.SetActive(true);
            _meshRenderer.material.SetColor(_textureTintColor, _colorFloor);
        }
    }

    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _arPlane = GetComponent<ARPlane>();
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
        _onClicked?.Invoke(new PlaneId(trackableId.subId1, trackableId.subId2), eventData.Pointer.Result.Details.Point);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
