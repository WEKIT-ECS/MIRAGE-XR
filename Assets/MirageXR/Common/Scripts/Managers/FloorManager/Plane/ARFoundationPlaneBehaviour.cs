using Microsoft.MixedReality.Toolkit.Input;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane), typeof(MeshCollider), typeof(MeshRenderer))]
public class ARFoundationPlaneBehaviour : MonoBehaviour, IPlaneBehaviour, IMixedRealityPointerHandler
{
    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static readonly int _textureTintColor = Shader.PropertyToID("_TexTintColor");

    [SerializeField] private Color _colorDefault = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color _colorFloor = new Color(0.03f, 1f, 0.09f, 0.7f);

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;
    private ARPlane _arPlane;

    public TrackableId trackableId =>  _arPlane.trackableId;

    public void UpdateState()
    {
        _meshCollider.enabled = floorManager.enableColliders;

        var manager = floorManager.manager as FloorManagerARFoundation;
        if (trackableId != manager.floorId)
        {
            gameObject.SetActive(floorManager.showPlanes);
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
        floorManager.SetFloor(this);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
