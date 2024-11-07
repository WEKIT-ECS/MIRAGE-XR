using UnityEngine;

public class SwitchToggleWithText : SwitchToggle
{
    [SerializeField] protected GameObject _textOff;
    [SerializeField] protected GameObject _textOn;

    protected override void UpdateView(bool value)
    {
        base.UpdateView(value);

        _textOff?.SetActive(!value);
        _textOn?.SetActive(value);
    }
}
