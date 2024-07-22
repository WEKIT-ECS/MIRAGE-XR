using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane), typeof(MeshCollider), typeof(MeshRenderer))]
public class ARFoundationPlaneBehaviour : MonoBehaviour, IPlaneBehaviour, IMixedRealityPointerHandler
{
    private static bool _isActive = false;
    private static bool _isCollidersEnabled = false;
    private static Action<PlaneId, Vector3> _onClicked;
    private static TrackableId _specialPlaneId = TrackableId.invalidId;

    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _shadowMaterial;

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;
    private ARPlane _arPlane;
    private readonly Material[] _defaultMaterials = new Material[2];
    private readonly Material[] _selectedMaterials = new Material[2];
    private readonly Material[] _shadowOnlyMaterials = new Material[1];

    public TrackableId TrackableId => _arPlane != null ? _arPlane.trackableId : TrackableId.invalidId;

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

        if (TrackableId != _specialPlaneId || TrackableId == TrackableId.invalidId)
        {
            _meshRenderer.sharedMaterials = _isActive ? _defaultMaterials : _shadowOnlyMaterials;
        }
        else
        {
            _meshRenderer.sharedMaterials = _selectedMaterials;
        }
    }

    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _arPlane = GetComponent<ARPlane>();
        _defaultMaterials[0] = _defaultMaterial;
        _defaultMaterials[1] = _shadowMaterial;
        _selectedMaterials[0] = _selectedMaterial;
        _selectedMaterials[1] = _shadowMaterial;
        _shadowOnlyMaterials[0] = _shadowMaterial;
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
        if (TrackableId == TrackableId.invalidId || eventData?.Pointer?.Result?.Details.Point == null)
        {
            return;
        }

        _onClicked?.Invoke(new PlaneId(TrackableId.subId1, TrackableId.subId2), eventData.Pointer.Result.Details.Point);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
