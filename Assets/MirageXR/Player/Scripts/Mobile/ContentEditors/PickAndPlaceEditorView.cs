using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditorView : PopupEditorBase
{
    private static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;
    private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;

    public override ContentType editorForType => ContentType.PICKANDPLACE;

    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Toggle toggleTrigger;
    [SerializeField] private TMP_InputField triggerStepIndex;
    [SerializeField] private TMP_InputField triggerStepTime;
    [SerializeField] private GameObject triggerIndexObject;
    [SerializeField] private GameObject triggerTimeObject;
    [SerializeField] private Dropdown resetOnDropDown;

    private bool isTrigger;
    private int resetOption = 0;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        _inputField.text = _content != null ? _content.text : string.Empty;

        if (_content != null)
        {
            _inputField.text = _content.text;

            resetOption = int.Parse(_content.key);
            resetOnDropDown.value = resetOption;

            var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == _content.poi);
            isTrigger = trigger != null ? true : false;

            if (isTrigger)
            {
                toggleTrigger.isOn = isTrigger;
                triggerStepTime.text = trigger.duration.ToString();
                triggerStepIndex.text = trigger.value;
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
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }
        _content.text = _inputField.text;
        _content.key = resetOption.ToString();

        if (isTrigger)
        {
            _step.AddOrReplaceArlemTrigger(TriggerMode.PickAndPlace, ActionType.PickAndPlace, _content.poi, int.Parse(triggerStepTime.text), triggerStepIndex.text);
        }
        else
        {
            _step.RemoveArlemTrigger(_content);
        }


        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);
        Close();
    }

    public void setResetOption(Dropdown option)
    {
        resetOption = option.value;
    }

    public void triggerToggle(bool trigger)
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps == 1)
        {
            if (toggleTrigger.isOn)
            {
                DialogWindow.Instance.Show(
                "Info!",
                "Only one step has been found in this activity!\n Add a new step and try again.",
                new DialogButtonContent("Ok"));

                toggleTrigger.isOn = false;
            }
            return;
        }
        else
        {
            isTrigger = toggleTrigger.isOn;
            triggerStepIndex.interactable = toggleTrigger.isOn;
            triggerStepIndex.text = numberOfSteps.ToString();
            triggerStepTime.interactable = toggleTrigger.isOn;
            triggerStepTime.text = "1";
        }

        triggerIndexObject.SetActive(toggleTrigger.isOn);
        triggerTimeObject.SetActive(toggleTrigger.isOn);
    }

    public void OnStepTriggerValueChanged()
    {
        var numberOfSteps = activityManager.ActionsOfTypeAction.Count;

        if (numberOfSteps < int.Parse(triggerStepIndex.text))
        {
            DialogWindow.Instance.Show(
            "Info!",
            "The entered step number doesn't exist yet. This trigger will jump to the last avalible step",
            new DialogButtonContent("Ok"));

            triggerStepIndex.text = numberOfSteps.ToString();
            return;
        }
    }
}
