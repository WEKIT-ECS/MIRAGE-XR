using LearningExperienceEngine;
using MirageXR;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditorView : PopupEditorBase
{
    private static LearningExperienceEngine.AugmentationManager _augmentationManager => LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager;
    private static LearningExperienceEngine.WorkplaceManager _workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;

    public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.PICKANDPLACE;

    [SerializeField] private TMP_InputField _inputField;


    [SerializeField] private Toggle _correctToggle;
    [SerializeField] private TMP_InputField _correctStepIndex;
    [SerializeField] private Toggle _incorrectToggle;
    [SerializeField] private TMP_InputField _incorrectStepIndex;

    [SerializeField] private GameObject _correctTriggerIndexObject;
    [SerializeField] private GameObject _incorrectTriggerIndexObject;
    [SerializeField] private Dropdown _resetOnDropDown;

    private bool _isCorrectTrigger;
    private bool _isIncorrectTrigger;

    private int _resetOption = 0;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        _inputField.text = _content != null ? _content.text : string.Empty;

        if (_content != null)
        {
            _inputField.text = _content.text;

            _resetOption = int.Parse(_content.key);
            _resetOnDropDown.value = _resetOption;

            var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == _content.poi);

            _isCorrectTrigger = OpenTriggerInfo("correct", _correctToggle, _correctStepIndex);
            _isIncorrectTrigger = OpenTriggerInfo("incorrect", _incorrectToggle, _incorrectStepIndex);
        }
    }

    protected override void OnAccept()
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
            _content = _augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }
        _content.text = _inputField.text;
        _content.key = "1";

        CreateOrRemoveTrigger(_isCorrectTrigger, LearningExperienceEngine.TriggerMode.PickAndPlace, "correct", _correctStepIndex.text);
        CreateOrRemoveTrigger(_isIncorrectTrigger, LearningExperienceEngine.TriggerMode.IncorrectPickAndPlace, "incorrect", _incorrectStepIndex.text);

        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();
        Close();
    }

    public void SetResetOption(Dropdown option)
    {
        _resetOption = option.value;
    }

    public void OnCorrectToggleChanged()
    {
        _isCorrectTrigger = ToggleChanged(_correctToggle, _correctTriggerIndexObject);
    }

    public void OnIncorrectToggleChanged()
    {
        _isIncorrectTrigger = ToggleChanged(_incorrectToggle, _incorrectTriggerIndexObject);
    }

    public bool ToggleChanged(Toggle toggle, GameObject stepIndexObject)
    {
        if (toggle.isOn)
        {
            var isMorethanOneStep = IsMorethanOneStep();

            stepIndexObject.SetActive(isMorethanOneStep);
            toggle.isOn = isMorethanOneStep;
            return isMorethanOneStep;
        }
        else
        {
            stepIndexObject.SetActive(false);
            return false;
        }
    }

    private bool IsMorethanOneStep()
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps == 1)
        {
            DialogWindow.Instance.Show(
            "Info!",
            "Only one step has been found in this activity!\n Add a new step and try again.",
            new DialogButtonContent("Ok"));

            return false;
        }
        return true;
    }

    public void OnCorrectStepIndexValueChanged()
    {
        CheckInputedIndex(_correctStepIndex);
    }

    public void OnIncorrectStepIndexValueChanged()
    {
        CheckInputedIndex(_incorrectStepIndex);
    }

    private void CheckInputedIndex(TMP_InputField inputedIndex)
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps < int.Parse(inputedIndex.text))
        {
            DialogWindow.Instance.Show(
            "Info!",
            "The entered step number doesn't exist yet. This trigger will jump to the last avalible step",
            new DialogButtonContent("Ok"));

            inputedIndex.text = numberOfSteps.ToString();
            return;
        }
    }

    private void CreateOrRemoveTrigger(bool isTrigger, LearningExperienceEngine.TriggerMode triggerMode, string suffix, string index)
    {
        if (isTrigger)
        {
            _step.AddOrReplaceArlemTrigger(triggerMode, LearningExperienceEngine.ActionType.PickAndPlace, _content.poi + suffix, 1, index);
        }
        else
        {
            _step.RemoveArlemTrigger(_content.poi + suffix);
        }
    }

    private bool OpenTriggerInfo(string suffix, Toggle toggle, TMP_InputField inputField)
    {
        var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == _content.poi + suffix);

        var isTrigger = trigger != null ? true : false;

        if (isTrigger)
        {
            toggle.isOn = isTrigger;
            inputField.text = trigger.value;
        }

        return isTrigger;
    }
}
