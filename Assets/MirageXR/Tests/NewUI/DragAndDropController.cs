using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropController : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [Serializable] public class UnityEventIntInt : UnityEvent<int, int> { }

    private const string SHADOW_OBJECT_NAME = "Shadow";

    [SerializeField] private RectTransform _moveTransform;
    [SerializeField] private LayoutElement _layout;
    [SerializeField] private UnityEventIntInt _onSiblingIndexChanged = new UnityEventIntInt();

    public UnityEventIntInt onSiblingIndexChanged => _onSiblingIndexChanged;

    private ScrollRect _scrollRect;
    private Transform _shadowTransform;
    private LayoutElement _shadowLayout;
    private int _totalChild;
    private bool _childControlHeight;
    private bool _isInited;
    private bool _isActive;
    private bool _isNeedToSkipUpdate;
    private PointerEventData _lastPointerData;

    private void Awake()
    {
        _scrollRect = GetComponentInParent<ScrollRect>();
    }

    private void CreateShadowObject()
    {
        var obj = new GameObject(SHADOW_OBJECT_NAME);
        _shadowLayout = obj.AddComponent<LayoutElement>();
        _shadowLayout.preferredHeight = _layout.preferredHeight;
        _shadowTransform = obj.transform;
        _shadowTransform.SetParent(_scrollRect.content, false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CreateShadowObject();

        _isActive = true;
        _shadowTransform.gameObject.SetActive(true);
        _shadowTransform.SetSiblingIndex(_moveTransform.GetSiblingIndex());
        _moveTransform.SetAsLastSibling();
        _layout.ignoreLayout = true;
        _totalChild = _scrollRect.content.childCount - 1;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Move(eventData);
        ChangeSiblingIndex();
        Scroll(eventData);
        _lastPointerData = eventData;
        _isNeedToSkipUpdate = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isActive = false;
        _moveTransform.SetSiblingIndex(_shadowTransform.GetSiblingIndex());
        _shadowTransform.SetAsLastSibling();
        _shadowTransform.gameObject.SetActive(false);
        _layout.ignoreLayout = false;

        Destroy(_shadowTransform.gameObject);
    }

    private void Update()
    {
        if (_isActive && _lastPointerData != null)
        {
            if (_isNeedToSkipUpdate)
            {
                _isNeedToSkipUpdate = false;
                return;
            }

            OnDrag(_lastPointerData);
        }
    }

    private void Move(PointerEventData eventData)
    {
        var position = _moveTransform.position;
        _moveTransform.position = new Vector3(position.x, eventData.position.y, position.z);
    }

    private void ChangeSiblingIndex()
    {
        var index = _shadowTransform.GetSiblingIndex();
        Transform topItem = null;
        Transform botItem = null;

        if (index > 0)
        {
            topItem = _scrollRect.content.GetChild(index - 1);
        }

        if (index < _totalChild - 1)
        {
            botItem = _scrollRect.content.GetChild(index + 1);
        }

        if (topItem != null && topItem.localPosition.y < _moveTransform.localPosition.y)
        {
            var newIndex = index - 1;
            _shadowTransform.SetSiblingIndex(newIndex);
            _onSiblingIndexChanged.Invoke(index, newIndex);
        }

        if (botItem != null && botItem.localPosition.y > _moveTransform.localPosition.y)
        {
            var newIndex = index + 1;
            _shadowTransform.SetSiblingIndex(newIndex);
            _onSiblingIndexChanged.Invoke(index, newIndex);
        }
    }

    private void Scroll(PointerEventData eventData)
    {
        var viewport = _scrollRect.viewport;
        var viewportRect = viewport.rect;
        var contentRect = _scrollRect.content.rect;

        if (viewport.position.y - (viewportRect.height * viewport.pivot.y) < eventData.position.y)
        {
            _scrollRect.verticalNormalizedPosition = Mathf.Min(1, _scrollRect.verticalNormalizedPosition + (viewportRect.height / contentRect.height * 0.01f));
        }

        if (viewport.position.y + (viewportRect.height * (1 - viewport.pivot.y)) > eventData.position.y)
        {
            _scrollRect.verticalNormalizedPosition = Mathf.Max(0, _scrollRect.verticalNormalizedPosition - (viewportRect.height / contentRect.height * 0.01f));
        }
    }
}
