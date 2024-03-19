using System;
using DG.Tweening;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ActionEditorView : PopupEditorBase
{
    //private const float MIN_SLIDER_VALUE = 1; // del
    //private const float MAX_SLIDER_VALUE = 10; // del
    //private const float DEFAULT_SLIDER_VALUE = 3; // del
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
     
    private const float MIN_GAZE_DURATION_VALUE = 1.0f;
    private const float MAX_GAZE_DURATION_VALUE = 10.0f;
    private const float DEFAULT_GAZE_DURATION_VALUE = 3.0f;
    private float _currentGazeDurationValue;
    public class IntHolder : ObjectHolder<int> { }

    public override ContentType editorForType => ContentType.ACT;

    [SerializeField] private Image _thumbnailImage;
    [SerializeField] private TMP_Text _thumbnailLabel;
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private GlyphListItem _glyphListItemPrefab;
    [SerializeField] private Toggle _toggleGazeTrigger;
    //[SerializeField] private Slider _slider;// del
    [SerializeField] private Button _btnAccept;
    [SerializeField] private Button _btnIncreaseGazeDuration;
    [SerializeField] private Button _btnDecreaseGazeDuration;
    [SerializeField] private GameObject _panelGazeDuration;
    [SerializeField] private GameObject _underline;
    [SerializeField] private GameObject _panelJumpToStep;
    [Space]
    [SerializeField] private ClampedScrollRect _clampedScrollJumpToStep;
    [SerializeField] private GameObject _templatePrefab;
    [Space] 
    [SerializeField] private GameObject _actionScrollPanel;
    [SerializeField] private GameObject _actionSettingsPanel;
    [SerializeField] private GameObject _bottomButtonPanel;
    [Space]
    [SerializeField] private TMP_Text _txtSliderValue;
    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    //[SerializeField] private TMP_Text _txtStep;
    //[SerializeField] private Button _btnNextStep;
    //[SerializeField] private Button _btnPreviousStep;
    [SerializeField] private ActionObject[] _actionObjects;
    
    //[SerializeField] private Button _accept;
    //[SerializeField] private GameObject _acceptButtonObject;
    //[SerializeField] private GameObject _closeButtonObject;

    private Trigger _trigger;
    private float _gazeDuration;
    private string _inputTriggerStepNumber = string.Empty;
    private int _scrollRectStep;
    private string _prefabName;
    private bool _editing;

    private int _maxStepIndex => activityManager.ActionsOfTypeAction.Count - 1;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
        _toggleGazeTrigger.onValueChanged.AddListener(OnTriggerValueChanged);
        //_slider.onValueChanged.AddListener(OnSliderValueChanged); // del
        _btnIncreaseGazeDuration.onClick.AddListener(OnIncreaseGazeDuration);
        _btnDecreaseGazeDuration.onClick.AddListener(OnDecreaseGazeDuration);
        _btnAccept.onClick.AddListener(OnAccept);
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        
        _actionScrollPanel.SetActive(true);
        _actionSettingsPanel.SetActive(false);
        _bottomButtonPanel.SetActive(false);
        //_btnNextStep.onClick.AddListener(OnNextToClick); // del
        //_btnPreviousStep.onClick.AddListener(OnPreviousToClick);  //del
        //_accept.onClick.AddListener(Edit);

        _editing = false;
        _currentGazeDurationValue = DEFAULT_GAZE_DURATION_VALUE;
        
        _clampedScrollJumpToStep.onItemChanged.AddListener(OnItemJumpToStepChanged);
        var steps = activityManager.ActionsOfTypeAction;
        var stepsCount = steps.Count;
        InitClampedScrollRect(_clampedScrollJumpToStep, _templatePrefab, stepsCount, stepsCount.ToString());

        UpdateView();
        RootView_v2.Instance.HideBaseView();
    }
    
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
                _scrollRectStep = i - 1;
            }
        }
    }

    private void UpdateView()
    {
        //_slider.minValue = MIN_SLIDER_VALUE;
        //_slider.maxValue = MAX_SLIDER_VALUE;

        _toggleGazeTrigger.isOn = true;
        //_slider.value = DEFAULT_SLIDER_VALUE; // del
        //OnSliderValueChanged(_slider.value); // del
        _txtSliderValue.text = _currentGazeDurationValue.ToString("0");;

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

        _scrollRectStep = activityManager.ActionsOfTypeAction.IndexOf(_step);
        var isLastStep = activityManager.IsLastAction(_step);

        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            _scrollRectStep = isLastStep ? _scrollRectStep - 1 : _scrollRectStep + 1;
        }

        if (_content != null)
        {
            //_closeButtonObject.SetActive(false);
            //_acceptButtonObject.SetActive(true);

            _trigger = _step.triggers.Find(tr => tr.id == _content.poi);
            if (_trigger != null)
            {
                _toggleGazeTrigger.isOn = true;
                _inputTriggerStepNumber = _trigger.value;
                _scrollRectStep = int.Parse(_inputTriggerStepNumber) - 1;
                //_slider.value = _trigger.duration;
                //OnSliderValueChanged(_trigger.duration);
            }
        }
        //_txtStep.text = (_inputTriggerStepNumber + 1).ToString();
    }

    private void OnTriggerValueChanged(bool value)
    {
        UnityEngine.Debug.LogError("[111] OnTriggerValueChanged value = " + value);
        //_slider.interactable = value;
        // Disable +/- and jump to step
        _panelGazeDuration.SetActive(value);
        _underline.SetActive(value);
        _panelJumpToStep.SetActive(value);
    }
    
    private void OnItemJumpToStepChanged(Component item)
    {
        _inputTriggerStepNumber = item.GetComponent<ObjectHolder<int>>().item.ToString();
    }

    /*private void OnSliderValueChanged(float value)
    {
        _gazeDuration = value;
        _txtSliderValue.text = $"{_gazeDuration} sec";
    }*/

    private void OnIncreaseGazeDuration()
    {
        if (_currentGazeDurationValue < MAX_GAZE_DURATION_VALUE)
        {
            _currentGazeDurationValue++;
        }

        _txtSliderValue.text = _currentGazeDurationValue.ToString("0");
    }
    
    private void OnDecreaseGazeDuration()
    {
        if (_currentGazeDurationValue > MIN_GAZE_DURATION_VALUE)
        {
            _currentGazeDurationValue--;
        }

        _txtSliderValue.text = _currentGazeDurationValue.ToString("0");
    }

    /*private void OnNextToClick()
    {
        if (_scrollRectStep >= _maxStepIndex) return;
        _scrollRectStep++;
        _txtStep.text = (_inputTriggerStepNumber + 1).ToString(); // ?
    }*/

    /*private void OnPreviousToClick()
    {
        if (_scrollRectStep <= 0) return;
        _scrollRectStep--;
        _txtStep.text = (_inputTriggerStepNumber + 1).ToString();
    }*/

    private void OnAccept(string prefabName)
    {
        _prefabName = prefabName;
        Debug.LogError("[111] OnAccept prefabName = " + prefabName);
        //OnAccept();
        //_thumbnailImage = 
        _thumbnailLabel.text = prefabName;
        OpenActionSettingsPanel();
    }

    private void OpenActionSettingsPanel()
    {
        _actionScrollPanel.SetActive(false);
        _actionSettingsPanel.SetActive(true);
        _bottomButtonPanel.SetActive(true);
    }

    protected override void OnAccept()
    {
        if (_content != null)
        {
            if (!_editing)
            {
                EventManager.DeactivateObject(_content);
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

        if (_toggleGazeTrigger.isOn)
        {
            _step.AddOrReplaceArlemTrigger(TriggerMode.Detect, ActionType.Act, _content.poi, _gazeDuration, (_inputTriggerStepNumber + 1).ToString());
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }

        if (!_editing)
        {
            EventManager.ActivateObject(_content);
        }

        base.OnAccept();
        Close();
    }
    
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

    public void Edit()
    {
        _editing = true;
        OnAccept();
    }

    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
    }
}
