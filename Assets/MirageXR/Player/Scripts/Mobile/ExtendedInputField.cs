using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtendedInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Image _image;
    [SerializeField] private Color _errorColor;
    [SerializeField] private Color _normalColor;

    private Func<string, bool> _validator;

    public string text
    {
        get => _inputField.text;
        set => _inputField.text = value;
    }

    public TMP_InputField inputField => _inputField;

    public bool isValid => _validator?.Invoke(text) ?? true;

    private void Start()
    {
        _image.color = _normalColor;
        _inputField.onEndEdit.AddListener(OnEditEnd);
        _inputField.onSelect.AddListener(OnSelect);
    }

    public void SetValidator(Func<string, bool> validator)
    {
        _validator = validator;
    }

    public void SetInvalid()
    {
        if (_validator == null) return;

        _image.color = _errorColor;
    }

    public void ResetValidation()
    {
        _image.color = _normalColor;
    }

    public bool Validate()
    {
        if (_validator != null)
        {
            var result = _validator(text);
            _image.color = result ? _normalColor : _errorColor;
            return result;
        }

        return true;
    }

    private void OnEditEnd(string value)
    {
        if (_validator != null) _image.color = _validator(value) ? _normalColor : _errorColor;
    }

    private void OnSelect(string value)
    {
        if (_validator != null) ResetValidation();
    }
}
