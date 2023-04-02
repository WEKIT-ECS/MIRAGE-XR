using System;
using System.Collections;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Action = System.Action;

public class PageView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Serializable] public class IntUnityEvent : UnityEvent<int> { }

    [SerializeField] private RectTransform _content;
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private bool _elastic;
    [SerializeField] private bool _interactable = true;
    [SerializeField] private float _moveTime = 0.3f;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private IntUnityEvent _onMoveBegin = new IntUnityEvent();
    [SerializeField] private IntUnityEvent _onMoveEnd = new IntUnityEvent();
    [SerializeField] private IntUnityEvent _onPageChanged = new IntUnityEvent();

    private int _totalPages;
    private int _currentPageIndex;
    private Coroutine _coroutine;

    public bool elastic => _elastic;

    public bool interactable => _interactable;

    public IntUnityEvent onMoveBegin => _onMoveBegin;

    public IntUnityEvent onMoveEnd => _onMoveEnd;

    public IntUnityEvent onPageChanged => _onPageChanged;

    public int currentPageIndex
    {
        get
        {
            return _currentPageIndex;
        }

        set
        {
            if (value >= 0 && value < _totalPages && _currentPageIndex != value)
            {
                OnMoveBegin();
                _currentPageIndex = value;
                MoveTo(CalculatePositionForPage(_currentPageIndex));
                _onPageChanged.Invoke(_currentPageIndex);
            }
        }
    }

    private void Awake()
    {
        _totalPages = _content.childCount;
    }

    public void MovePrevious()
    {
        currentPageIndex -= 1;
    }

    public void MoveNext()
    {
        //TODO: Replace back
        //currentPageIndex += 1;
        currentPageIndex = 4;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnMoveBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_interactable)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_content, eventData.position, eventData.pressEventCamera, out var position))
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_content, eventData.position - eventData.delta, eventData.pressEventCamera, out var positionLast))
            {
                var deltaX = position.x - positionLast.x;

                if (!_elastic)
                {
                    var viewMax = _viewport.TransformPoint(_viewport.rect.max);
                    var viewMin = _viewport.TransformPoint(_viewport.rect.min);
                    var contentMax = _content.TransformPoint(_content.rect.max);
                    var contentMin = _content.TransformPoint(_content.rect.min);

                    if (viewMax.x > contentMax.x + deltaX)
                    {
                        deltaX = viewMax.x - contentMax.x;
                    }

                    if (viewMin.x < contentMin.x + deltaX)
                    {
                        deltaX = viewMin.x - contentMin.x;
                    }
                }

                _content.Translate(deltaX, 0, 0);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_interactable)
        {
            return;
        }

        var width = _content.rect.width;
        var position = -(_content.anchoredPosition.x - (width * _content.pivot.x));
        var index = Mathf.RoundToInt(position / (width / _totalPages));
        index = Mathf.Clamp(index, 0, _totalPages - 1);
        var newPosition = CalculatePositionForPage(index);
        MoveTo(newPosition);
        if (_currentPageIndex != index)
        {
            _currentPageIndex = index;
            _onPageChanged.Invoke(_currentPageIndex);
        }
    }

    public void SetPageIndexImmediately(int index)
    {
        if (index < 0 || index >= _totalPages) return;

        _content.anchoredPosition = CalculatePositionForPage(index);
        if (_currentPageIndex != index)
        {
            _currentPageIndex = index;
            _onPageChanged.Invoke(_currentPageIndex);
        }
    }

    private void MoveTo(Vector3 newPosition)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        _coroutine = StartCoroutine(MoveToEnumerator(_content, newPosition, _moveTime, _animationCurve, OnMoveEnd));
    }

    private void OnMoveBegin()
    {
        _onMoveBegin.Invoke(_currentPageIndex);
    }

    private void OnMoveEnd()
    {
        _onMoveEnd.Invoke(_currentPageIndex);
    }

    private Vector3 CalculatePositionForPage(int index)
    {
        var anchoredPosition = _content.anchoredPosition3D;
        var width = _content.rect.width;
        var x = -((index * width / _totalPages) - (width * _content.pivot.x));
        return new Vector3(x, anchoredPosition.y, anchoredPosition.z);
    }

    private static IEnumerator MoveToEnumerator(RectTransform rectTransform, Vector3 endPosition, float time, AnimationCurve curve = null, Action callback = null)
    {
        curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);

        var startPosition = rectTransform.anchoredPosition;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + (Time.deltaTime / time));
            var value = curve.Evaluate(timer);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, value);

            yield return null;
        }

        EventManager.NotifyOnMobilePageChanged();
        callback?.Invoke();
    }
}