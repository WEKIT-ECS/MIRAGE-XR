using TMPro;
using UnityEngine;

public class SwitchToggleWithText_Spatial : SwitchToggleWithText
{
    [SerializeField] private Color _colorTextOn;
    [SerializeField] private Color _colorTextOff;
    
    private TMP_Text _tmpTextOn;
    private TMP_Text _tmpTextOff;
    private void Awake()
    {
        _tmpTextOn = _textOn.GetComponent<TMP_Text>();
        _tmpTextOff = _textOff.GetComponent<TMP_Text>();
    }
    protected override void UpdateView()
    {
        base.UpdateView();

        _textOff?.SetActive(true);
        _textOn?.SetActive(true);
        
        _tmpTextOn.color = _toggle.isOn ? _colorTextOff : _colorTextOn;
        _tmpTextOff.color = _toggle.isOn ? _colorTextOn : _colorTextOff;
    }
}
