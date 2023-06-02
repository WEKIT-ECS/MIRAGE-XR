using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    private const float SHOW_TIME = 3f;
    private const float SHOW_TIME_SHORT = 1.5f;
    private const float FADE_TIME = 0.2f;
    private const int MAX_MESSAGE_QUEUE = 3;

    public static Toast Instance { get; private set; }

    [SerializeField] private TMP_Text _message;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private LayoutElement _layoutElement;
    [SerializeField] private GameObject _icon;

    private readonly Queue<string> _queue = new Queue<string>();
    private bool _isActive;
    private Coroutine _coroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            AppLog.LogError($"{Instance.GetType().FullName} must only be a single copy!");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _canvasGroup.gameObject.SetActive(false);
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (!_isActive && _queue.Count > 0) ViewMessage(_queue.Dequeue());
    }

    public void Show(string message, bool showIcon = false)
    {
        _icon.SetActive(showIcon);
        _queue.Enqueue(message);
    }

    private void ViewMessage(string message)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _coroutine = StartCoroutine(ViewMessageIEnumerator(message));
    }

    private IEnumerator ViewMessageIEnumerator(string message)
    {
        _message.text = message;
        _isActive = true;
        _canvasGroup.gameObject.SetActive(true);

        SetupLayout();

        yield return FadeTo(_canvasGroup, 1.0f, FADE_TIME);
        yield return new WaitForSeconds(_queue.Count >= MAX_MESSAGE_QUEUE ? SHOW_TIME_SHORT : SHOW_TIME);
        yield return FadeTo(_canvasGroup, 0.0f, FADE_TIME);
        _canvasGroup.gameObject.SetActive(false);
        _message.text = string.Empty;
        _isActive = false;
    }

    private void SetupLayout()
    {
        var maxWidth = _layoutElement.preferredWidth;
        var width = _message.margin.x + _message.margin.z + _message.preferredWidth;
        _layoutElement.enabled = width > maxWidth;
    }

    private void UpdateWidth()
    {
        var rect = (RectTransform)_canvasGroup.transform;
        var width = _message.margin.x + _message.margin.z + _message.preferredWidth;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private static IEnumerator FadeTo(CanvasGroup canvasGroup, float alphaEnd, float time, AnimationCurve curve = null, Action callback = null)
    {
        if (curve == null || curve.length == 0) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        var alphaStart = canvasGroup.alpha;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + Time.deltaTime / time);
            canvasGroup.alpha = Mathf.Lerp(alphaStart, alphaEnd, curve.Evaluate(timer));
            yield return null;
        }

        callback?.Invoke();
    }
}
