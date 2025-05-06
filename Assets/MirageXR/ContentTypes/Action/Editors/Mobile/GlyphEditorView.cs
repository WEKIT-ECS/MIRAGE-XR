using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlyphEditorView : PopupEditorBase
{
    private const float MIN_SLIDER_VALUE = 1;
    private const float MAX_SLIDER_VALUE = 10;
    private const float DEFAULT_SLIDER_VALUE = 3;

    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Action;

    [SerializeField] private Transform _contentContainer;
    [SerializeField] private GlyphListItem _glyphListItemPrefab;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _txtSliderValue;
    [SerializeField] private TMP_Text _txtStep;
    [SerializeField] private Button _btnNextStep;
    [SerializeField] private Button _btnPreviousStep;
    [SerializeField] private ActionObject[] _actionObjects;
    [Space]
    [SerializeField] private Button _accept;
    [SerializeField] private GameObject _acceptButtonObject;
    [SerializeField] private GameObject _closeButtonObject;

    private LearningExperienceEngine.Trigger _trigger;
    private float _gazeDuration;
    private int _triggerStepIndex;
    private string _prefabName;
    private bool _editing;

    private int _maxStepIndex => activityManager.ActionsOfTypeAction.Count - 1;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _toggleTrigger.onValueChanged.AddListener(OnTriggerValueChanged);
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _btnNextStep.onClick.AddListener(OnNextToClick);
        _btnPreviousStep.onClick.AddListener(OnPreviousToClick);
        _accept.onClick.AddListener(Edit);

        _editing = false;

        UpdateView();
    }

    private void UpdateView()
    {
        _slider.minValue = MIN_SLIDER_VALUE;
        _slider.maxValue = MAX_SLIDER_VALUE;

        _toggleTrigger.isOn = false;
        _slider.value = DEFAULT_SLIDER_VALUE;
        OnSliderValueChanged(_slider.value);

        for (int i = _contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = _contentContainer.GetChild(i);
            Destroy(child);
        }

        foreach (var actionObject in _actionObjects)
        {
            var item = Instantiate(_glyphListItemPrefab, _contentContainer);
            item.Init(actionObject, OnAccept);
        }

        _triggerStepIndex = activityManager.ActionsOfTypeAction.IndexOf(_step);
        var isLastStep = activityManager.IsLastAction(_step);

        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            _triggerStepIndex = isLastStep ? _triggerStepIndex - 1 : _triggerStepIndex + 1;
        }

        if (_content != null)
        {
            _closeButtonObject.SetActive(false);
            _acceptButtonObject.SetActive(true);

            _trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            if (_trigger != null)
            {
                _toggleTrigger.isOn = true;
                _triggerStepIndex = int.Parse(_trigger.value) - 1;
                _slider.value = _trigger.duration;
                OnSliderValueChanged(_trigger.duration);
            }
        }
        _txtStep.text = (_triggerStepIndex + 1).ToString();
    }

    private void OnTriggerValueChanged(bool value)
    {
        _slider.interactable = value;
    }

    private void OnSliderValueChanged(float value)
    {
        _gazeDuration = value;
        _txtSliderValue.text = $"{_gazeDuration} sec";
    }

    private void OnNextToClick()
    {
        if (_triggerStepIndex >= _maxStepIndex) return;
        _triggerStepIndex++;
        _txtStep.text = (_triggerStepIndex + 1).ToString();
    }

    private void OnPreviousToClick()
    {
        if (_triggerStepIndex <= 0) return;
        _triggerStepIndex--;
        _txtStep.text = (_triggerStepIndex + 1).ToString();
    }

    private void OnAccept(string prefabName, Sprite sprite)
    {
        _prefabName = prefabName;
        OnAccept();
    }

    protected override void OnAccept()
    {
        if (_content != null)
        {
            if (!_editing)
            {
                LearningExperienceEngine.EventManager.DeactivateObject(_content);
            }
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        if (!_editing)
        {
            _content.predicate = $"act:{_prefabName}";
        }

        if (_toggleTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(LearningExperienceEngine.TriggerMode.Detect, LearningExperienceEngine.ActionType.Act, _content.poi, _gazeDuration, (_triggerStepIndex + 1).ToString());
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        if (!_editing)
        {
            LearningExperienceEngine.EventManager.ActivateObject(_content);
        }

        base.OnAccept();
        Close();
    }

    public void Edit()
    {
        _editing = true;
        OnAccept();
    }
}
