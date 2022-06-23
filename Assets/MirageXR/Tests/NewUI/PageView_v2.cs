using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PageView_v2 : MonoBehaviour, IDragHandler, IEndDragHandler
{

    [Serializable]
    public class IntUnityEvent : UnityEvent<int> { }

    [SerializeField] private RectTransform _content;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private bool _elastic;
    [SerializeField] private float _moveTime = 0.2f;
    public bool interactable = true;

    public IntUnityEvent OnPageChanged = new IntUnityEvent();

    private int _totalPages;
    private int _currentPageIndex;
    private Coroutine _coroutine;

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
                _currentPageIndex = value;
                MoveTo(CalculatePositionForPage(_currentPageIndex));
                OnPageChanged.Invoke(_currentPageIndex);
            }
        }
    }

    private void Start()
    {
        _totalPages = _content.childCount;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!interactable) return;

        _content.Translate(data.delta.x, 0, 0);
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (!interactable) return;

        var width = _content.rect.width;
        var position = -(_content.anchoredPosition.x - width * _content.pivot.x);
        var index = Mathf.RoundToInt(position / (width / _totalPages));
        index = Mathf.Clamp(index, 0, _totalPages - 1);
        var newPosition = CalculatePositionForPage(index);
        MoveTo(newPosition);
        if (_currentPageIndex != index)
        {
            _currentPageIndex = index;
            OnPageChanged.Invoke(_currentPageIndex);
        }
    }

    public void SetPageIndexImmediately(int index)
    {
        if (index < 0 || index >= _totalPages) return;

        _content.anchoredPosition = CalculatePositionForPage(index);
        if (_currentPageIndex != index)
        {
            _currentPageIndex = index;
            OnPageChanged.Invoke(_currentPageIndex);
        }
    }

    private void MoveTo(Vector3 newPosition)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _coroutine = StartCoroutine(MoveToEnumerator(_content, newPosition, _moveTime, _animationCurve));
    }

    private Vector3 CalculatePositionForPage(int index)
    {
        var anchoredPosition = _content.anchoredPosition3D;
        var width = _content.rect.width;
        var x = -(index * width / _totalPages - width * _content.pivot.x);
        return new Vector3(x, anchoredPosition.y, anchoredPosition.z);
    }

    private static IEnumerator MoveToEnumerator(RectTransform rectTransform, Vector3 endPosition, float time, AnimationCurve curve = null, Action callback = null)
    {
        if (curve == null) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        var startPosition = rectTransform.anchoredPosition;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + Time.deltaTime / time);
            var value = curve.Evaluate(timer);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, value);

            yield return null;
        }

        callback?.Invoke();
    }
}