using UnityEngine;

public class SwitchToggleWithText : SwitchToggle
{
    [SerializeField] protected GameObject _textOff;
    [SerializeField] protected GameObject _textOn;

    protected override void UpdateView()
    {
        base.UpdateView();

        _textOff?.SetActive(!_toggle.isOn);
        _textOn?.SetActive(_toggle.isOn);
    }
}
