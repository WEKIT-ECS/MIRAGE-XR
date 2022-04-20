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
    
    public override ContentType editorForType => ContentType.ACT;
    
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private GlyphListItem _glyphListItemPrefab;
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _txtSliderValue;
    [SerializeField] private TMP_Text _txtStep;
    [SerializeField] private Button _btnNextStep;
    [SerializeField] private Button _btnPreviousStep;
    [SerializeField] private ActionObject[] _actionObjects;

    private Trigger _trigger;
    private float _gazeDuration;
    private int _triggerStepIndex;
    private string _prefabName;
    
    private int _maxStepIndex => ActivityManager.Instance.ActionsOfTypeAction.Count - 1;
    
    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        _toggleTrigger.onValueChanged.AddListener(OnTriggerValueChanged);
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _btnNextStep.onClick.AddListener(OnNextToClick);
        _btnPreviousStep.onClick.AddListener(OnPreviousToClick);

        UpdateView();
    }
    
    private void UpdateView()
    {
        _slider.minValue = MIN_SLIDER_VALUE;
        _slider.maxValue = MAX_SLIDER_VALUE;

        _toggleTrigger.isOn = false;
        _slider.value = DEFAULT_SLIDER_VALUE;
        
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
        
        _triggerStepIndex = ActivityManager.Instance.ActionsOfTypeAction.IndexOf(_step);
        var isLastStep = ActivityManager.Instance.IsLastAction(_step);

        if (ActivityManager.Instance.ActionsOfTypeAction.Count > 1)
        {
            _triggerStepIndex = isLastStep ? _triggerStepIndex - 1 : _triggerStepIndex + 1;
        }
        
        if (_content != null)
        {
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
    
    private void OnAccept(string prefabName)
    {
        _prefabName = prefabName;
        OnAccept();
    }
    
    protected override void OnAccept()
    {
        if (_content != null)
        {
            EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = ActivityManager.Instance.AddAugmentation(_step, GetOffset());
        }

        _content.predicate = $"act:{_prefabName}";
        
        if (_toggleTrigger.isOn)
        {
            var triggerType = _content.predicate.Contains(":") ? _content.predicate.Split(':')[0] : _step.predicate;
            _step.AddOrReplaceArlemTrigger("detect", triggerType, _content.poi, _gazeDuration, (_triggerStepIndex + 1).ToString());
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }
        
        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);

        Close();
    }
}
