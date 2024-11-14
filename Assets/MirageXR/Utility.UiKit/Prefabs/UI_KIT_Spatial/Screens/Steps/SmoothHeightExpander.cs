using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SmoothHeightExpander : MonoBehaviour
{
    [SerializeField] private Button _expandButton;
    [SerializeField] private GameObject _buttonImageArrowUp;
    [SerializeField] private GameObject _buttonImageArrowDown;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private GameObject _body;
    [SerializeField] private float targetHeight = 254f;
    [SerializeField] private float animationDuration = 0f;

    private float _initialHeight;
    private Coroutine _resizeCoroutine;

    private void Start()
    {
        _initialHeight = _panel.sizeDelta.y;
        _expandButton.onClick.AddListener(ToggleHeight);
    }

    private void ToggleHeight()
    {
        if (_resizeCoroutine != null) 
            StopCoroutine(_resizeCoroutine);
        
        var target = _panel.sizeDelta.y == _initialHeight ? targetHeight : _initialHeight;
        _body.SetActive(_panel.sizeDelta.y == _initialHeight);
        _buttonImageArrowUp.SetActive(_panel.sizeDelta.y == _initialHeight);
        _buttonImageArrowDown.SetActive(_panel.sizeDelta.y != _initialHeight);
        _resizeCoroutine = StartCoroutine(AnimateHeight(target));
    }

    private IEnumerator AnimateHeight(float target)
    {
        var startHeight = _panel.sizeDelta.y;
        var timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
            timeElapsed += Time.deltaTime;
            var newHeight = Mathf.Lerp(startHeight, target, timeElapsed / animationDuration);
            _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, newHeight);
            yield return null;
        }
        
        _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, target);
        _resizeCoroutine = null;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
    }
}
