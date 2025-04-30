using LearningExperienceEngine;
using DG.Tweening;
using System;
using System.Globalization;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

public class LabelEditorView : PopupEditorBase
{
    public class IntHolder : ObjectHolder<int> { }

    private const float MIN_SLIDER_VALUE = 1;
    private const float MAX_SLIDER_VALUE = 10;
    private const float DEFAULT_SLIDER_VALUE = 3;
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    private const bool DefaultBillboardedValue = true;
    private const int DefaultFontSize = 17;
    private const int DefaultGazeTime = 3;
    private const int MinFontSize = 5;
    private const int MaxFontSize = 50;
    private const int MaxGazeTime = 10;
    private const int MinGazeTime = 1;

    public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.LABEL;

    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Toggle _toggleBillboard;
    [SerializeField] private Button _btnIncreaseGazeDuration;
    [SerializeField] private Button _btnDecreaseGazeDuration;
    [SerializeField] private TMP_Text _txtGazeDurationValue;
    [SerializeField] private GameObject _gazeDurationPanel;
    [SerializeField] private ClampedScrollRect _clampedScrollJumpToStep;
    [SerializeField] private GameObject _clampedScrollObject;
    [SerializeField] private GameObject _templatePrefab;
    [Space]
    [SerializeField] public TMP_Text _exampleLabel;
    [SerializeField] public TMP_Text _examplePlaceholderLabel;
    [SerializeField] public Image _exampleLabelBackground;
    [SerializeField] private Button _fontSizeButton;
    [SerializeField] private Button _fontColorButton;
    [SerializeField] private Button _backgroundColorButton;
    [SerializeField] private Image _fontColourButtonImage;
    [SerializeField] private Image _backgroundColourButtonImage;
    [SerializeField] private TMP_Text _fontSizeText;
    [Space]
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [SerializeField] private LabelSettings _labelSettings;

    //private Trigger _trigger;
    private float _gazeDuration;
    private int _triggerStepIndex;
    private bool _isBillboarded;
    private Content<LabelContentData> _contentLabel;
    private string _labelText;
    private Color _colorBackground;
    private Color _colorFont;
    private int _sizeFont;
    private int _gazeTime;

    public enum SettingsPanel { Size, Font, Background };
    public SettingsPanel _settingsPanelStart = SettingsPanel.Background;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
        
        _labelText = string.Empty;
        _colorBackground = Color.white;
        _colorFont = Color.black;
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
        
        _toggleTrigger.onValueChanged.AddListener(OnTriggerValueChanged);
        _toggleBillboard.onValueChanged.AddListener(OnBillboardValueChanged);
        _btnIncreaseGazeDuration.onClick.AddListener(OnIncreaseGazeDuration);
        _btnDecreaseGazeDuration.onClick.AddListener(OnDecreaseGazeDuration);
        _clampedScrollJumpToStep.onItemChanged.AddListener(OnItemJumpToStepChanged);

        _fontSizeButton.onClick.AddListener(ShowFontSizePanel);
        _fontColorButton.onClick.AddListener(ShowFontColorPanel);
        _backgroundColorButton.onClick.AddListener(ShowBackgroundColorPanel);
        _inputField.onValueChanged.AddListener(OnInputValueChanged);

        //var steps = activityManager.ActionsOfTypeAction;
        //var stepsCount = steps.Count;
        //InitClampedScrollRect(_clampedScrollJumpToStep, _templatePrefab, stepsCount, stepsCount.ToString());

        _gazeDurationPanel.SetActive(false);

        UpdateView();
        _clampedScrollObject.SetActive(_toggleTrigger.isOn);
        RootView_v2.Instance.HideBaseView();
    }
    private void OnInputValueChanged(string value)
    {
        _labelText = value;
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
        //_textGazeValue.text = _gazeTime.ToString();

        //clampedScrollRectFontSize.SetCurrentItem(_sizeFont - MinFontSize);
    }

    /*private void UpdateView()
    {
        _inputField.text = string.Empty;
        _toggleTrigger.isOn = false;
        _isBillboarded = true;
        _gazeDuration = DEFAULT_SLIDER_VALUE;
        _txtGazeDurationValue.text = DEFAULT_SLIDER_VALUE.ToString("0");

        _triggerStepIndex = activityManager.ActionsOfTypeAction.IndexOf(_step);
        var isLastStep = activityManager.IsLastAction(_step);

        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            _triggerStepIndex = isLastStep ? _triggerStepIndex - 1 : _triggerStepIndex + 1;
        }

        if (_content != null)
        {
            _inputField.text = _content.text;
            //_trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            _isBillboarded = _content.billboarded;
            _toggleBillboard.isOn = _content.billboarded;

            /*if (_trigger != null)
            {
                _toggleTrigger.isOn = true;
                _triggerStepIndex = int.Parse(_trigger.value) - 1;
                _gazeDuration = _trigger.duration;
                _clampedScrollJumpToStep.currentItemIndex = _triggerStepIndex;
            }*/

            /*if (_content.option != "")
            {
                string[] splitArray = _content.option.Split(char.Parse("-"));

                _exampleLabel.text = _content.text;

                _exampleLabel.fontSize = int.Parse(splitArray[0]);

                _exampleLabel.color = GetColorFromString(splitArray[1]);
                _exampleLabelBackground.color = GetColorFromString(splitArray[2]);
            }
        }
        UpdateButtonColours();
    }*/

    private void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount, string text)
    {
        var currentActionId = activityManager.ActiveAction.id;
        var steps = activityManager.ActionsOfTypeAction;

        for (int i = 1; i <= maxCount; i++)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = i;
            obj.GetComponentInChildren<TMP_Text>().text = $"   {i}/{text}     {steps[i - 1].instruction.title}";

            if (steps[i - 1].id == currentActionId)
            {
                _clampedScrollJumpToStep.currentItemIndex = i - 1;
            }
        }
    }

    private void OnItemJumpToStepChanged(Component item)
    {
        _triggerStepIndex = item.GetComponent<ObjectHolder<int>>().item - 1;
    }

    private void OnIncreaseGazeDuration()
    {
        if (_gazeDuration < MAX_SLIDER_VALUE)
        {
            _gazeDuration++;
        }

        _txtGazeDurationValue.text = _gazeDuration.ToString("0");
    }

    private void OnDecreaseGazeDuration()
    {
        if (_gazeDuration > MIN_SLIDER_VALUE)
        {
            _gazeDuration--;
        }

        _txtGazeDurationValue.text = _gazeDuration.ToString("0");
    }

    private void OnTriggerValueChanged(bool value)
    {
        _gazeDurationPanel.SetActive(value);
        _clampedScrollObject.SetActive(value);
    }

    private void OnBillboardValueChanged(bool value)
    {
        _isBillboarded = value;
    }

    /*protected override void OnAccept()
    {
        if (string.IsNullOrEmpty(_inputField.text))
        {
            Toast.Instance.Show("Input field is empty.");
            return;
        }

        if (_content != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }
        _content.text = _inputField.text;
        _content.option = _exampleLabel.fontSize.ToString() + "-" + _exampleLabel.color.ToString() + "-" + _exampleLabelBackground.color.ToString();
        _content.billboarded = _isBillboarded;

        if (_toggleTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(LearningExperienceEngine.TriggerMode.Detect, LearningExperienceEngine.ActionType.Label, _content.poi, _gazeDuration, (_triggerStepIndex + 1).ToString());
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        LearningExperienceEngine.EventManager.ActivateObject(_content);
        LearningExperienceEngine.EventManager.NotifyActionModified(_step);
        Close();
    }*/

    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
    }

    private Color GetColorFromString(string rgb)
    {
        string[] rgba = rgb.Substring(5, rgb.Length - 6).Split(", ");
        Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));

        return color;
    }

    public void onInputChanged()
    {
        _exampleLabel.text = _inputField.text;
    }

    private void UpdateButtonColours()
    {
        _fontColourButtonImage.color = _exampleLabel.color;
        _backgroundColourButtonImage.color = _exampleLabelBackground.color;
        _fontSizeText.text = _exampleLabel.fontSize.ToString(CultureInfo.InvariantCulture);
    }

    private void ShowFontSizePanel()
    {
        _settingsPanelStart = SettingsPanel.Size;
        ShowSettings();
    }

    private void ShowFontColorPanel()
    {
        _settingsPanelStart = SettingsPanel.Font;
        ShowSettings();
    }

    private void ShowBackgroundColorPanel()
    {
        _settingsPanelStart = SettingsPanel.Background;
        ShowSettings();
    }

    private void ShowSettings()
    {
        PopupsViewer.Instance.Show(_labelSettings, this);
    }

    public void LabelSettingsChanged(Color font, Color background, float size)
    {
        _exampleLabelBackground.color = background;
        _exampleLabel.color = font;
        _exampleLabel.fontSize = size;

        _colorBackground = background;
        _colorFont = font;
        _sizeFont = (int)size;
        
        UpdateButtonColours();
    }
}
