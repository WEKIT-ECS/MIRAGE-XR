using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabelSettings : PopupBase
{
    public class IntHolder : ObjectHolder<int> { }

    [SerializeField] ColourSelector _colorSelector;
    [SerializeField] GameObject _colorSelectorObject;
    [SerializeField] ClampedScrollRect _clampedScrollRect;
    [SerializeField] GameObject _fontScrollRectObject;
    [SerializeField] private GameObject _templatePrefab;
    [Space]
    [SerializeField] private Toggle _fontSizeToggle;
    [SerializeField] private Toggle _textColorToggle;
    [SerializeField] private Toggle _backgroundToggle;
    [SerializeField] private Button _closeButton;
    [Space]
    [SerializeField] private TMP_InputField _labelPreviewInputField;
    [SerializeField] private TMP_Text _labelPreviewText;
    [SerializeField] private Image _labelPreviewImage;

    private LabelEditorView _labelEditorView;
    private Color _fontColor;
    private Color _backgroundColor;
    private float _fontSize;
    private string _text;

    private enum ColourPickerOption { NA, Font, Background };
    private ColourPickerOption _colourPickerOption = ColourPickerOption.NA;

    protected override bool TryToGetArguments(params object[] args)
    {
        if (args is { Length: 1 } && args[0] is LabelEditorView obj)
        {
            _labelEditorView = obj;
            return true;
        }

        return false;
    }

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _colorSelector.onColourSelected.AddListener(OnColourPickerChange);
        _fontSizeToggle.onValueChanged.AddListener(ShowFontSizePanel);
        _textColorToggle.onValueChanged.AddListener(ShowFontColorPanel);
        _backgroundToggle.onValueChanged.AddListener(ShowBackgroundColorPanel);
        _closeButton.onClick.AddListener(Close);
        _clampedScrollRect.onItemChanged.AddListener(OnFontSizeChanged);

        _fontSize = _labelEditorView._exampleLabel.fontSize;
        _fontColor = _labelEditorView._exampleLabel.color;
        _backgroundColor = _labelEditorView._exampleLabelBackground.color;
        _text = _labelEditorView._exampleLabel.text;

        UpdateLabelPreview();
        InitClampedScrollRect(_clampedScrollRect, _templatePrefab, 100);

        switch (_labelEditorView._settingsPanelStart)
        {
            case LabelEditorView.SettingsPanel.Size:
                _fontSizeToggle.isOn = true;
                break;
            case LabelEditorView.SettingsPanel.Font:
                _textColorToggle.isOn = true;
                break;
            case LabelEditorView.SettingsPanel.Background:
                _backgroundToggle.isOn = true;
                break;
            default:
                break;
        }
    }

    public override void Close()
    {
        _labelEditorView.LabelSettingsChanged(_fontColor, _backgroundColor, _fontSize);
        base.Close();
    }

    private void ShowFontSizePanel(bool isOn)
    {
        if (isOn)
        {
            _colorSelectorObject.SetActive(false);
            _fontScrollRectObject.SetActive(true);

            _colourPickerOption = ColourPickerOption.NA;
        }
    }

    private void ShowFontColorPanel(bool isOn)
    {
        if (isOn)
        {
            _fontScrollRectObject.SetActive(false);
            _colorSelectorObject.SetActive(true);

            _colourPickerOption = ColourPickerOption.Font;
        }
    }

    private void ShowBackgroundColorPanel(bool isOn)
    {
        if (isOn)
        {
            _fontScrollRectObject.SetActive(false);
            _colorSelectorObject.SetActive(true);

            _colourPickerOption = ColourPickerOption.Background;
        }
    }

    private void OnColourPickerChange()
    {
        switch (_colourPickerOption)
        {
            case ColourPickerOption.Font:
                _fontColor = _colorSelector._selectedColour;
                break;
            case ColourPickerOption.Background:
                _backgroundColor = _colorSelector._selectedColour;
                break;
            default:
                break;
        }
        UpdateLabelPreview();
    }

    private void OnFontSizeChanged(Component item)
    {
        _fontSize = item.GetComponent<ObjectHolder<int>>().item;
        UpdateLabelPreview();
    }

    private void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount)
    {
        for (int i = 1; i <= maxCount; i++)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = i;
            obj.GetComponentInChildren<TMP_Text>().text = $"{i}";

            if (Mathf.Approximately(i, _fontSize))
            {
                _clampedScrollRect.currentItemIndex = i - 1;
            }
        }
    }

    private void UpdateLabelPreview()
    {
        _labelPreviewInputField.text = _text;
        _labelPreviewText.color = _fontColor;
        _labelPreviewText.fontSize = _fontSize;
        _labelPreviewImage.color = _backgroundColor;
    }
}
