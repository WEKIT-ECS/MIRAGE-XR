using UnityEngine;

public class SwitchToggleWithText : SwitchToggle
{
    [SerializeField] private GameObject _textOff;
    [SerializeField] private GameObject _textOn;

    protected override void Start()
    {
        base.Start();

        _textOff?.SetActive(true);
        _textOn?.SetActive(false);

        if (_toggle.isOn)
        {
            OnEditValueChanged(true);
            _textOff?.SetActive(false);
            _textOn?.SetActive(true);
        }
    }

    protected override void OnEditValueChanged(bool value)
    {
        base.OnEditValueChanged(value);

        _textOff?.SetActive(!value);
        _textOn?.SetActive(value);
    }
}
