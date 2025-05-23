using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] protected RectTransform _handleRectTransform;
    [SerializeField] protected Toggle _toggle;
    [SerializeField] protected Color _colorOn;
    [SerializeField] protected Color _colorOff;
    protected Vector2 _handlePosition;
    protected Image _background;

    public UnityEvent<bool> onValueChanged => _toggle.onValueChanged;

    protected virtual void Start()
    {
        _handlePosition = _handleRectTransform.anchoredPosition;
        _toggle.onValueChanged.AddListener(OnValueChanged);
        _background = _handleRectTransform.parent.GetComponent<Image>();
        UpdateView();
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        _toggle.SetIsOnWithoutNotify(value);
    }

    public void ForceUpdateView()
    {
        UpdateView();
    }

    protected virtual void OnValueChanged(bool value)
    {
        UpdateView();
    }

    protected virtual void UpdateView()
    {
        _handleRectTransform.anchoredPosition = _toggle.isOn ? _handlePosition * -1 : _handlePosition;
        _background.color = _toggle.isOn ? _colorOn : _colorOff;
    }

    protected virtual void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnValueChanged);
    }
}
