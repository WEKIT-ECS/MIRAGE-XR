using System;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
    public class LabelEditorSpatialView : EditorSpatialView
    {
        public class IntHolder : ObjectHolder<int> { }
        public class GuidHolder : ObjectHolder<Guid> { }

        private const bool DefaultBillboardedValue = true;
        private const int DefaultFontSize = 17;
        private const int MinFontSize = 5;
        private const int MaxFontSize = 50;
        private const int DefaultGazeTime = 3;
        private const int MaxGazeTime = 10;
        private const int MinGazeTime = 1;

        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Toggle _toggleBillboard;
        [SerializeField] private Toggle _toggleTapTrigger;
        [SerializeField] private Toggle _toggleGazeTrigger;
        [SerializeField] private Button _fontSizeButton;
        [SerializeField] private Button _fontColorButton;
        [SerializeField] private Button _backgroundColorButton;
        [SerializeField] private Button _buttonGazePlus;
        [SerializeField] private Button _buttonGazeMinus;
        [SerializeField] private Image _exampleLabelBackground;
        [SerializeField] private Image _fontColourButtonImage;
        [SerializeField] private Image _backgroundColourButtonImage;
        [SerializeField] private TMP_Text _exampleLabel;
        [SerializeField] private TMP_Text _examplePlaceholderLabel;
        [SerializeField] private TMP_Text _fontSizeText;
        [SerializeField] private TMP_Text _textGazeValue;
        [SerializeField] private ClampedScrollRect _clampedScrollRect;
        [SerializeField] private Transform _clampedScrollRectTemplate;
        [FormerlySerializedAs("_settingsPanel")]
        [Space]
        [Header("Font Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button buttonCloseSettings;
        [SerializeField] private Toggle toggleSettingsFontSize;
        [SerializeField] private Toggle toggleSettingsFontColor;
        [SerializeField] private Toggle toggleSettingsBackgroundColor;
        [SerializeField] private GameObject panelSettingsFontSize;
        [SerializeField] private GameObject panelSettingsFontColor;
        [SerializeField] private GameObject panelSettingsBackgroundColor;
        [SerializeField] private Transform containerSettingsFontColor;
        [SerializeField] private Transform containerSettingsBackgroundColor;
        [SerializeField] private GameObject itemSettingsFontColorPrefab;
        [SerializeField] private GameObject itemSettingsBackgroundColorPrefab;
        [SerializeField] private ClampedScrollRect clampedScrollRectFontSize;
        [SerializeField] private Transform clampedScrollRectTemplateFontSize;
        [SerializeField] private Color[] _colors;

        private Guid _triggerStepId;
        private string _labelText;
        private Color _colorBackground;
        private Color _colorFont;
        private int _sizeFont;
        private bool _isBillboarded;
        private int _gazeTime;
        private Content<LabelContentData> _contentLabel;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            _showBackground = false;
            base.Initialization(onClose, args);

            _labelText = string.Empty;
            _colorBackground = _colors[10];
            _colorFont = _colors[0];
            _sizeFont = DefaultFontSize;
            _isBillboarded = DefaultBillboardedValue;
            _gazeTime = DefaultGazeTime;
            _contentLabel = Content as Content<LabelContentData>;

            if (_contentLabel != null)
            {
                _labelText = _contentLabel.ContentData.Text;
                _colorBackground = _contentLabel.ContentData.BackgroundColor;
                _colorFont = _contentLabel.ContentData.FontColor;
                _sizeFont = _contentLabel.ContentData.FontSize;
                _isBillboarded = _contentLabel.ContentData.IsBillboarded;
            }

            _inputField.onValueChanged.AddListener(OnLabelTextValueChanged);
            _toggleBillboard.onValueChanged.AddListener(OnBillboardValueChanged);

            settingsPanel.SetActive(false);
            buttonCloseSettings.onClick.AddListener(() => settingsPanel.SetActive(false));
            _fontSizeButton.onClick.AddListener(() => 
            {
                settingsPanel.SetActive(true);
                toggleSettingsFontSize.isOn = true;
            });
            _fontColorButton.onClick.AddListener(() => 
            {
                settingsPanel.SetActive(true);
                toggleSettingsFontColor.isOn = true;
            });
            _backgroundColorButton.onClick.AddListener(() => 
            {
                settingsPanel.SetActive(true);
                toggleSettingsBackgroundColor.isOn = true;
            });
            _buttonGazePlus.onClick.AddListener(OnClickGazePlusClicked);
            _buttonGazeMinus.onClick.AddListener(OnClickGazeMinusClicked);

            panelSettingsFontSize.SetActive(true);
            panelSettingsFontColor.SetActive(false);
            panelSettingsBackgroundColor.SetActive(false);
            
            toggleSettingsFontSize.onValueChanged.AddListener(panelSettingsFontSize.SetActive);
            toggleSettingsFontColor.onValueChanged.AddListener(OnToggleSettingsFontColorValueChanged);
            toggleSettingsBackgroundColor.onValueChanged.AddListener(OnToggleSettingsBackgroundColorValueChanged);

            InitColorsPicker(_colors, itemSettingsFontColorPrefab, containerSettingsFontColor, OnFontColorValueChanged);
            InitColorsPicker(_colors, itemSettingsBackgroundColorPrefab, containerSettingsBackgroundColor, OnBackgroundColorValueChanged);
            InitClampedScrollRect();
            InitSettingsClampedScrollRect();
            UpdateView();
        }

        private void OnToggleSettingsFontColorValueChanged(bool value)
        {
            panelSettingsFontColor.SetActive(value);
            if (value)
            {
                foreach (Transform item in containerSettingsFontColor)
                {
                    var toggle = item.GetComponent<Toggle>();
                    if (toggle.targetGraphic.color == _colorFont)
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
            }
        }

        private void OnToggleSettingsBackgroundColorValueChanged(bool value)
        {
            panelSettingsBackgroundColor.SetActive(value);
            if (value)
            {
                foreach (Transform item in containerSettingsBackgroundColor)
                {
                    var toggle = item.GetComponent<Toggle>();
                    if (toggle.targetGraphic.color == _colorBackground)
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
            }
        }

        private void OnFontColorValueChanged(bool value, Toggle toggle)
        {
            if (value)
            {
                _colorFont = toggle.targetGraphic.color;
            }

            ((Image)toggle.targetGraphic).maskable = !value;
            ((Image)toggle.graphic).maskable = !value;

            _fontColourButtonImage.color = _colorFont;
            _exampleLabel.color = _colorFont;
            _examplePlaceholderLabel.color = _colorFont;
        }

        private void OnBackgroundColorValueChanged(bool value, Toggle toggle)
        {
            if (value)
            {
                _colorBackground = toggle.targetGraphic.color;
            }

            ((Image)toggle.targetGraphic).maskable = !value;
            ((Image)toggle.graphic).maskable = !value;

            _backgroundColourButtonImage.color = _colorBackground;
            _exampleLabelBackground.color = _colorBackground;
        }

        private void OnClickGazeMinusClicked()
        {
            if (_gazeTime == MinGazeTime)
            {
                return;
            }
            _gazeTime--;
            _textGazeValue.text = _gazeTime.ToString();
        }

        private void OnClickGazePlusClicked()
        {
            if (_gazeTime == MaxGazeTime)
            {
                return;
            }
            _gazeTime++;
            _textGazeValue.text = _gazeTime.ToString();
        }

        private void OnLabelTextValueChanged(string text)
        {
            _labelText = text;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_inputField.transform);
        }

        private void UpdateView()
        {
            _toggleBillboard.SetIsOnWithoutNotify(_isBillboarded);
            _fontColourButtonImage.color = _colorFont;
            _backgroundColourButtonImage.color = _colorBackground;
            _exampleLabelBackground.color = _colorBackground;
            _exampleLabel.color = _colorFont;
            _exampleLabel.fontSize = _sizeFont;
            _examplePlaceholderLabel.color = _colorFont;
            _examplePlaceholderLabel.fontSize = _sizeFont;
            _fontSizeText.text = _sizeFont.ToString();
            _inputField.text = _labelText;
            _textGazeValue.text = _gazeTime.ToString();

            clampedScrollRectFontSize.SetCurrentItem(_sizeFont - MinFontSize);
        }

        private void OnBillboardValueChanged(bool value)
        {
            _isBillboarded = value;
        }

        private static void InitColorsPicker(Color[] colors, GameObject prefab, Transform contentHolder, UnityAction<bool, Toggle> onValueChanged)
        {
            foreach (var color in colors)
            {
                var itemFont = Instantiate(prefab, contentHolder);
                itemFont.SetActive(true);
                var toggle = itemFont.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(value => onValueChanged(value, toggle));
                toggle.targetGraphic.color = color;
                ((Image)toggle.targetGraphic).maskable = true;
                ((Image)toggle.graphic).maskable = true;
            }
        }

        private void InitClampedScrollRect()
        {
            var steps = RootObject.Instance.LEE.StepManager.Steps;
            for (int i = 0; i < steps.Count; i++)
            {
                var obj = Instantiate(_clampedScrollRectTemplate, _clampedScrollRect.content, false);
                obj.gameObject.name = steps[i].Name;
                obj.gameObject.SetActive(true);
                obj.gameObject.AddComponent<GuidHolder>().item = steps[i].Id;
                var texts = obj.GetComponentsInChildren<TMP_Text>();
                if (texts.Length == 2)
                {
                    var number = RootObject.Instance.LEE.StepManager.GetStepNumber(steps[i].Id);
                    texts[0].text = number.ToString();
                    texts[1].text = steps[i].Name;
                }

                if (steps[i].Id == RootObject.Instance.LEE.StepManager.CurrentStep.Id)
                {
                    _clampedScrollRect.currentItemIndex = i;
                }
            }
            
            _clampedScrollRect.onItemChanged.AddListener(rectTransform =>
            {
                _triggerStepId = rectTransform.GetComponent<GuidHolder>().item;
            });
        }

        private void InitSettingsClampedScrollRect()
        {
            for (int i = 0; i <= MaxFontSize - MinFontSize; i++)
            {
                var obj = Instantiate(clampedScrollRectTemplateFontSize, clampedScrollRectFontSize.content, false);
                obj.gameObject.name = (i + MinFontSize).ToString();
                obj.gameObject.SetActive(true);
                obj.gameObject.AddComponent<IntHolder>().item = i + MinFontSize;
                var text = obj.GetComponentInChildren<TMP_Text>();
                text.text = (i + MinFontSize).ToString();

                if (i + MinFontSize == _sizeFont)
                {
                    clampedScrollRectFontSize.currentItemIndex = i;
                }
            }

            clampedScrollRectFontSize.onItemChanged.AddListener(rectTransform =>
            {
                _sizeFont = rectTransform.GetComponent<IntHolder>().item;
                _exampleLabel.fontSize = _sizeFont;
                _examplePlaceholderLabel.fontSize = _sizeFont;
                _fontSizeText.text = _sizeFont.ToString();
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_inputField.transform);
            });
        }

        protected override void OnAccept()
        {
            if (string.IsNullOrEmpty(_labelText))
            {
                AppLog.LogWarning("Text field is empty");
                return;
            }

            _contentLabel = CreateContent<LabelContentData>(ContentType.Label);
            _contentLabel.ContentData.Text = _labelText;
            _contentLabel.ContentData.IsBillboarded = _isBillboarded;
            _contentLabel.ContentData.BackgroundColor = _colorBackground;
            _contentLabel.ContentData.FontColor = _colorFont;
            _contentLabel.ContentData.FontSize = _sizeFont;
            _contentLabel.Location = Location.GetDefaultStartLocation();

            if (IsContentUpdate)
            {
                RootObject.Instance.LEE.ContentManager.UpdateContent(_contentLabel);
            }
            else
            {
                RootObject.Instance.LEE.ContentManager.AddContent(_contentLabel);
            }
            Close();
        }
    }
}
