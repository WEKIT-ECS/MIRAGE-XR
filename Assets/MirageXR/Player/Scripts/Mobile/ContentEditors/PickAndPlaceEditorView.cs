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
    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private TMP_InputField _triggerStepIndex;
    [SerializeField] private TMP_InputField _triggerStepTime;
    [SerializeField] private GameObject _triggerIndexObject;
    [SerializeField] private GameObject _triggerTimeObject;
    [SerializeField] private Dropdown _resetOnDropDown;

    private bool _isTrigger;
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
            _isTrigger = trigger != null ? true : false;

            if (_isTrigger)
            {
                _toggleTrigger.isOn = _isTrigger;
                _triggerStepTime.text = trigger.duration.ToString();
                _triggerStepIndex.text = trigger.value;
            }
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

        if (_isTrigger)
        {
            _step.AddOrReplaceArlemTrigger(TriggerMode.PickAndPlace, ActionType.PickAndPlace, _content.poi, int.Parse(_triggerStepTime.text), _triggerStepIndex.text);
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }


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
            if (_toggleTrigger.isOn)
            {
                DialogWindow.Instance.Show(
                "Info!",
                "Only one step has been found in this activity!\n Add a new step and try again.",
                new DialogButtonContent("Ok"));

                _toggleTrigger.isOn = false;
            }
            return;
        }
        else
        {
            _isTrigger = _toggleTrigger.isOn;
            _triggerStepIndex.interactable = _toggleTrigger.isOn;
            _triggerStepIndex.text = numberOfSteps.ToString();
            _triggerStepTime.interactable = _toggleTrigger.isOn;
            _triggerStepTime.text = "1";
        }

        _triggerIndexObject.SetActive(_toggleTrigger.isOn);
        _triggerTimeObject.SetActive(_toggleTrigger.isOn);
    }

    public void OnStepTriggerValueChanged()
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps < int.Parse(_triggerStepIndex.text))
        {
            DialogWindow.Instance.Show(
            "Info!",
            "The entered step number doesn't exist yet. This trigger will jump to the last avalible step",
            new DialogButtonContent("Ok"));

            _triggerStepIndex.text = numberOfSteps.ToString();
            return;
        }
    }
}
