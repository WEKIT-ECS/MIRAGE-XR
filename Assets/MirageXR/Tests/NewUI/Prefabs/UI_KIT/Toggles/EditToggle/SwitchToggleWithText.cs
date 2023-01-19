using UnityEngine;

public class SwitchToggleWithText : SwitchToggle
{
    [SerializeField] private GameObject _textOff;
    [SerializeField] private GameObject _textOn;

    protected override void UpdateView(bool value)
    {
        base.UpdateView(value);

        _textOff?.SetActive(!value);
        _textOn?.SetActive(value);
    }
}
