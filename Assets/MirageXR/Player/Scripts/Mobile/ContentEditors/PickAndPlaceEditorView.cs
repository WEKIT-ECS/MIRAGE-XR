using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditorView : PopupEditorBase
{
    private static AugmentationManager _augmentationManager => RootObject.Instance.augmentationManager;
    private static WorkplaceManager _workplaceManager => RootObject.Instance.workplaceManager;

    public override ContentType editorForType => ContentType.PICKANDPLACE;

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
            EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = _augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }
        _content.text = _inputField.text;
        _content.key = _resetOption.ToString();

        CreateOrRemoveTrigger(_isCorrectTrigger, TriggerMode.PickAndPlace, "correct", _correctStepIndex.text);
        CreateOrRemoveTrigger(_isIncorrectTrigger, TriggerMode.IncorrectPickAndPlace, "incorrect", _incorrectStepIndex.text);

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);
        Close();
    }

    public void SetResetOption(Dropdown option)
    {
        _resetOption = option.value;
    }

    public void TriggerToggle(bool trigger)
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps == 1)
        {
            if (_correctToggle.isOn || _incorrectToggle.isOn)
            {
                DialogWindow.Instance.Show(
                "Info!",
                "Only one step has been found in this activity!\n Add a new step and try again.",
                new DialogButtonContent("Ok"));

                _correctToggle.isOn = false;
                _incorrectToggle.isOn = false;
            }
            return;
        }
        else
        {
            _isCorrectTrigger = _correctToggle.isOn;
            _correctStepIndex.interactable = _correctToggle.isOn;

            _isIncorrectTrigger = _incorrectToggle.isOn;
            _incorrectStepIndex.interactable = _incorrectToggle.isOn;
        }

        _correctTriggerIndexObject.SetActive(_correctToggle.isOn);
        _incorrectTriggerIndexObject.SetActive(_incorrectToggle.isOn);
    }

    public void OnStepTriggerValueChanged()
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps < int.Parse(_incorrectStepIndex.text))
        {
            DialogWindow.Instance.Show(
            "Info!",
            "The entered step number doesn't exist yet. This trigger will jump to the last avalible step",
            new DialogButtonContent("Ok"));

            _incorrectStepIndex.text = numberOfSteps.ToString();
            return;
        }
    }

    private void CreateOrRemoveTrigger(bool isTrigger, TriggerMode triggerMode, string suffix, string index)
    {
        if (isTrigger)
        {
            _step.AddOrReplaceArlemTrigger(triggerMode, ActionType.PickAndPlace, _content.poi + suffix, 1, index);
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
