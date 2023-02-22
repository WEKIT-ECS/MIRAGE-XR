using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClampedScrollRect : ScrollRect
{
    [Serializable] public class UnityEventRectTransform : UnityEvent<RectTransform> {}

    [SerializeField] private UnityEventRectTransform _onItemChanged = new UnityEventRectTransform();
    [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float _moveTime = 0.25f;

    public UnityEventRectTransform onItemChanged => _onItemChanged;

    public RectTransform currentItem
    {
        get => _currentItem;
        set => SetCurrentItem(value);
    }

    public int currentItemIndex
    {
        get => GetItemIndex(_currentItem);
        set => SetCurrentItem(value);
    }

    private RectTransform _currentItem;
    private int _index;
    private float _child;
    private float _childSize;
    private HorizontalOrVerticalLayoutGroup _layoutGroup;
    private bool _horizontalLast = true;
    private bool _verticalLast = true;
    private bool _valid;
    private bool _updateInLateUpdate;
    private float _min;
    private Coroutine _coroutine;

    private int activeChildCount 
    {
        get
        {
            var ignoreCount = 0;
            foreach (Transform child in content)
            {
                var layoutElement = child.GetComponent<LayoutElement>();
                if (!child.gameObject.activeSelf || layoutElement && layoutElement.ignoreLayout)
                    ignoreCount++;
            }

            return content.childCount - ignoreCount;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (_horizontalLast != horizontal)
        {
            vertical = !horizontal;
            _verticalLast = vertical;
            _horizontalLast = horizontal;
        }

        if (_verticalLast != vertical)
        {
            horizontal = !vertical;
            _horizontalLast = horizontal;
            _verticalLast = vertical;
        }

        currentItem = _currentItem;
        movementType = MovementType.Unrestricted;
        scrollSensitivity = 0;
    }
#endif

    protected override void OnEnable()
    {
        base.OnEnable();
        SetCurrentItem(_index);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (_updateInLateUpdate)
        {
            SetCurrentItem(_index);
            _updateInLateUpdate = false;
        }
    }

    public void SetCurrentItem(int childIndex, bool activeOnly = true)
    {
        _index = childIndex;
        SetCurrentItem(GetContentItem(childIndex, activeOnly));
    }

    public int GetItemIndex(RectTransform rectTransform)
    {
        if (!rectTransform)
        {
            return -1;
        }

        var count = 0;
        var found = false;

        foreach (RectTransform child in content)
        {
            var layoutElement = child.GetComponent<LayoutElement>();
            if (!child.gameObject.activeSelf || (layoutElement && layoutElement.ignoreLayout))
            {
                continue;
            }

            if (rectTransform == child)
            {
                found = true;
                break;
            }

            count++;
        }

        if (!found)
        {
            return -1;
        }

        return count;
    }

    public void SetCurrentItem(RectTransform item)
    {
        var index = GetItemIndex(item);

        if (index == -1)
        {
            if (_currentItem != null)
            {
                _currentItem = null;
                _onItemChanged.Invoke(_currentItem);
            }

            return;
        }

        _index = index;
        CalculateValues();
        if (!_valid)
        {
            return;
        }

        var childCount = activeChildCount;
        index = childCount - 1 - index;
        if (_currentItem != item)
        {
            _currentItem = item;
            _onItemChanged.Invoke(_currentItem);
        }

        var position = _min + (_child * index);
        SetNormalizedPosition(position, horizontal ? 0 : 1);
    }

    public RectTransform GetContentItem(int index, bool activeOnly = true)
    {
        var count = -1;
        foreach (Transform child in content)
        {
            var layoutElement = child.GetComponent<LayoutElement>();
            if ((!activeOnly || child.gameObject.activeSelf) && (!layoutElement || !layoutElement.ignoreLayout))
            {
                count++;
            }

            if (count == index)
            {
                return (RectTransform)child;
            }
        }

        Debug.LogWarningFormat("can't get child with index {0}", index);
        return null;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        SnapRect();
    }

    private void CalculateValues()
    {
        if (!_layoutGroup)
        {
            _layoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        }

        if (activeChildCount == 0)
        {
            _valid = false;
            return;
        }

        var childTransform = GetContentItem(0);
        if (!childTransform)
        {
            Debug.LogWarning("content must have active child");
        }

        var spacing = _layoutGroup ? _layoutGroup.spacing : 0;
        var contentRect = content.rect;
        var viewportRect = viewport.rect;
        var childRect = childTransform.rect;
        var viewportSize = horizontal ? viewportRect.size.x : viewportRect.size.y;
        var contentSize = horizontal ? contentRect.size.x : contentRect.size.y;

        _valid = viewportSize < contentSize;
        if (!_valid && contentSize > 0 && viewportSize > 0)
        {
            Debug.LogWarning("content must be larger then viewport");
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content.transform);
            _updateInLateUpdate = true;
        }

        _childSize = spacing + (horizontal ? childRect.size.x : childRect.size.y);
        _child = _childSize / (contentSize - viewportSize);
        _min = ((-1 * (viewportSize / _childSize)) + 1 - (1 * (spacing / _childSize))) * _child * 0.5f;
    }

    private void SnapRect() {
        CalculateValues();

        if (!_valid)
        {
            return;
        }

        var childCount = activeChildCount;
        var pos = horizontal ? horizontalNormalizedPosition : verticalNormalizedPosition;

        var v = velocity;
        var force = horizontal ? v.x : v.y;

        var last = float.MaxValue;
        var count = 0;
        for (int i = 0; i < childCount; i++)
        {
            var p = _min + _child * i;
            var dist = Mathf.Abs(pos - p);
            if (dist < last)
            {
                last = dist;
                count = i;
            }
        }

        if (inertia)
        {
            count += (force > 0 ? -1 : 1) * Mathf.FloorToInt(Mathf.Abs(force * decelerationRate) / _childSize);
        }

        count = Mathf.Clamp(count, 0, childCount - 1);

        var item = GetContentItem(childCount - 1 - count);
        if (_currentItem != item)
        {
            _currentItem = item;
            _onItemChanged.Invoke(_currentItem);
        }

        var newPos = _min + _child * count;
        _coroutine = StartCoroutine(ScrollTo(newPos, _moveTime));
    }

    private IEnumerator ScrollTo(float position, float time) {
        var start = horizontal ? horizontalNormalizedPosition : verticalNormalizedPosition;

        var timer = 0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + (Time.deltaTime / time));
            var value = Mathf.Lerp(start, position, _curve.Evaluate(timer));
            SetNormalizedPosition(value, horizontal ? 0 : 1);
            yield return new WaitForEndOfFrame();
        }
    }
}