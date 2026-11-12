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
    private bool _initialized;

    public UnityEvent<bool> onValueChanged => _toggle.onValueChanged;

    protected virtual void Start()
    {
        EnsureInitialized();
        UpdateView();
    }

    public void EnsureInitialized()
    {
        if (_initialized) return;
        if (_handleRectTransform != null)
        {
            _handlePosition = new Vector2(-Mathf.Abs(_handleRectTransform.anchoredPosition.x), _handleRectTransform.anchoredPosition.y);
            if (_handleRectTransform.parent != null)
            {
                _background = _handleRectTransform.parent.GetComponent<Image>();
            }
        }
        if (_toggle != null)
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }
        _initialized = true;
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
        EnsureInitialized();
        if (_handleRectTransform != null && _toggle != null)
        {
            _handleRectTransform.anchoredPosition = _toggle.isOn ? _handlePosition * -1 : _handlePosition;
        }
        if (_background != null && _toggle != null)
        {
            _background.color = _toggle.isOn ? _colorOn : _colorOff;
        }
    }

    protected virtual void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnValueChanged);
    }
}
