using System.Collections;
using System.Collections.Generic;
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
    protected override void UpdateView(bool value)
    {
        base.UpdateView(value);

        _textOff?.SetActive(true);
        _textOn?.SetActive(true);
        
        _tmpTextOn.color = value ? _colorTextOff : _colorTextOn;
        _tmpTextOff.color = value ? _colorTextOn : _colorTextOff;
    }
}
