using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] protected RectTransform _handleRectTransform;
    [SerializeField] protected Toggle _toggle;
    [SerializeField] protected Color _colorOn;
    [SerializeField] protected Color _colorOff;
    protected Vector2 _handlePosition;
    protected Image _background;
    protected Color _currentBgColor;

    protected virtual void Start()
    {
        _handlePosition = _handleRectTransform.anchoredPosition;
        _toggle.onValueChanged.AddListener(OnEditValueChanged);
        _background = _handleRectTransform.parent.GetComponent<Image>();
        _currentBgColor = _colorOff;
    }

    protected virtual void OnEditValueChanged(bool value)
    {
        _handleRectTransform.anchoredPosition = value ? _handlePosition * -1 : _handlePosition;
        _background.color = value ? _colorOn : _currentBgColor;
    }

    protected virtual void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnEditValueChanged);
    }
}
