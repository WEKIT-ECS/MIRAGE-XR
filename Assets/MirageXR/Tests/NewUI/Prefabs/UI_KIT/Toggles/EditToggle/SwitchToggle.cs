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

    protected virtual void Start()
    {
        _handlePosition = _handleRectTransform.anchoredPosition;
        _toggle.onValueChanged.AddListener(UpdateView);
        _background = _handleRectTransform.parent.GetComponent<Image>();
        UpdateView(_toggle.isOn);
    }

    protected virtual void UpdateView(bool value)
    {
        _handleRectTransform.anchoredPosition = value ? _handlePosition * -1 : _handlePosition;
        _background.color = value ? _colorOn : _colorOff;
    }

    protected virtual void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(UpdateView);
    }
}
