using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] private RectTransform _handleRectTransform;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Color _colorOn;
    [SerializeField] private Color _colorOff;
    [SerializeField] private GameObject _textOff;
    [SerializeField] private GameObject _textOn;
    private Vector2 _handlePosition;
    private Image _background;
    private Color _currentBgColor;

    private void Start()
    {
        _handlePosition = _handleRectTransform.anchoredPosition;
        _toggle.onValueChanged.AddListener(OnEditValueChanged);
        _background = _handleRectTransform.parent.GetComponent<Image>();
        _currentBgColor = _colorOff;
        _textOff?.SetActive(true);
        _textOn?.SetActive(false);

        if (_toggle.isOn)
        {
            OnEditValueChanged(true);
            _textOff?.SetActive(false);
            _textOn?.SetActive(true);
        }
    }

    private void OnEditValueChanged(bool value)
    {
        _handleRectTransform.anchoredPosition = value ? _handlePosition * -1 : _handlePosition;
        _background.color = value ? _colorOn : _currentBgColor;

        _textOff?.SetActive(!value);
        _textOn?.SetActive(value);
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnEditValueChanged);
    }
}
