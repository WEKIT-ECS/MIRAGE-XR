using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer))]
public class EditorPlaneBehaviour : MonoBehaviour, IPlaneBehaviour, IPointerClickHandler
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

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var screenBegin = eventData.pointerPressRaycast.screenPosition;
        var screenEnd = eventData.pointerCurrentRaycast.screenPosition;
        var worldBegin = eventData.pointerPressRaycast.worldPosition;
        var delta = Vector2.Distance(screenBegin, screenEnd);
        if (delta < EventSystem.current.pixelDragThreshold)
        {
            _onClicked?.Invoke(_planeId, worldBegin);
        }
    }
}
