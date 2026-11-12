using System;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class PostItEditorSpatialView : EditorSpatialView
    {
        private const int DefaultFontSize = 18;
        private const bool DefaultBillboardedValue = true;
        private static readonly Color DefaultPostItColor = new Color(1f, 0.96f, 0.62f, 1f);

        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Toggle _toggleBillboard;
        [SerializeField] private TMP_Text _previewText;
        [SerializeField] private Image _previewBackground;
        [SerializeField] private TMP_Text _exampleLabel;
        [SerializeField] private Image _exampleLabelBackground;
        [SerializeField] private Button _btnIncreaseFontSize;
        [SerializeField] private Button _btnDecreaseFontSize;
        [SerializeField] private Button _fontSizeButton;
        [SerializeField] private Button _fontColorButton;
        [SerializeField] private Button _backgroundColorButton;
        [SerializeField] private TMP_Text _txtFontSize;
        [SerializeField] private TMP_Text _fontSizeText;
        [SerializeField] private Button[] _colorButtons;
        [SerializeField] private Color[] _presetColors = new Color[]
        {
            new Color(1.0f, 0.96f, 0.62f), // Yellow
            new Color(1.0f, 0.80f, 0.82f), // Pink
            new Color(0.73f, 0.87f, 0.98f), // Blue
            new Color(0.78f, 0.90f, 0.79f), // Green
            new Color(1.0f, 0.88f, 0.70f)  // Orange
        };

        private string _text = string.Empty;
        private Color _colorBackground = DefaultPostItColor;
        private Color _colorFont = Color.black;
        private int _fontSize = DefaultFontSize;
        private bool _isBillboarded = DefaultBillboardedValue;
        private Content<LabelContentData> _contentLabel;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _contentLabel = Content as Content<LabelContentData>;
            if (_contentLabel != null)
            {
                _text = _contentLabel.ContentData.Text ?? string.Empty;
                _colorBackground = _contentLabel.ContentData.BackgroundColor;
                _colorFont = _contentLabel.ContentData.FontColor;
                _fontSize = _contentLabel.ContentData.FontSize > 0 ? _contentLabel.ContentData.FontSize : DefaultFontSize;
                _isBillboarded = _contentLabel.ContentData.IsBillboarded;
            }
            else
            {
                _colorBackground = DefaultPostItColor;
                _colorFont = Color.black;
                _fontSize = DefaultFontSize;
                _isBillboarded = DefaultBillboardedValue;
            }

            if (_inputField != null)
            {
                _inputField.text = _text;
                _inputField.onValueChanged.AddListener(OnTextChanged);
            }

            if (_toggleBillboard != null)
            {
                _toggleBillboard.isOn = _isBillboarded;
                _toggleBillboard.onValueChanged.AddListener(val => _isBillboarded = val);
            }

            if (_btnIncreaseFontSize != null)
            {
                _btnIncreaseFontSize.onClick.AddListener(() => ChangeFontSize(2));
            }

            if (_btnDecreaseFontSize != null)
            {
                _btnDecreaseFontSize.onClick.AddListener(() => ChangeFontSize(-2));
            }

            if (_colorButtons != null && _presetColors != null)
            {
                for (int i = 0; i < _colorButtons.Length && i < _presetColors.Length; i++)
                {
                    int index = i;
                    _colorButtons[i].onClick.AddListener(() => SetBackgroundColor(_presetColors[index]));
                }
            }

            UpdatePreview();
        }

        private void OnTextChanged(string newText)
        {
            _text = newText;
            UpdatePreview();
        }

        private void ChangeFontSize(int delta)
        {
            _fontSize = Mathf.Clamp(_fontSize + delta, 8, 48);
            UpdatePreview();
        }

        private void SetBackgroundColor(Color color)
        {
            _colorBackground = color;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var displayText = string.IsNullOrEmpty(_text) ? "Write a note..." : _text;
            if (_previewText != null)
            {
                _previewText.text = displayText;
                _previewText.fontSize = _fontSize;
                _previewText.color = _colorFont;
            }
            if (_exampleLabel != null)
            {
                _exampleLabel.text = displayText;
                _exampleLabel.fontSize = _fontSize;
                _exampleLabel.color = _colorFont;
            }

            if (_previewBackground != null)
            {
                _previewBackground.color = _colorBackground;
            }
            if (_exampleLabelBackground != null)
            {
                _exampleLabelBackground.color = _colorBackground;
            }

            if (_txtFontSize != null)
            {
                _txtFontSize.text = _fontSize.ToString();
            }
            if (_fontSizeText != null)
            {
                _fontSizeText.text = _fontSize.ToString();
            }
        }

        protected override void OnAccept()
        {
            var content = CreateContent<LabelContentData>(ContentType.Label);
            if (content == null)
            {
                return;
            }

            content.ContentData.Text = _text;
            content.ContentData.BackgroundColor = _colorBackground;
            content.ContentData.FontColor = _colorFont;
            content.ContentData.FontSize = _fontSize;
            content.ContentData.IsBillboarded = _isBillboarded;

            if (IsContentUpdate)
            {
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            }
            else
            {
                RootObject.Instance.LEE.ContentManager.AddContent(content);
            }

            Close();
        }
    }
}
