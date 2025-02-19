using LearningExperienceEngine;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditor : MonoBehaviour
{
    private static LearningExperienceEngine.AugmentationManager _augmentationManager => LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager;
    private static LearningExperienceEngine.ActivityManager _activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    private static LearningExperienceEngine.WorkplaceManager _workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;

    [SerializeField] private Transform _annotationStartingPoint;
    [SerializeField] private InputField _textInputField;

    [SerializeField] private Toggle _correctToggle;
    [SerializeField] private InputField _correctStepIndex;

    [SerializeField] private Toggle _incorrectToggle;
    [SerializeField] private InputField _incorrectStepIndex;
    [SerializeField] private Dropdown _resetOnDropDown;

    [SerializeField] private GameObject _editor;
    [SerializeField] private GameObject _triggerSettings;

    private bool _isCorrectTrigger;
    private bool _isIncorrectTrigger;

    private LearningExperienceEngine.Action _action;
    private LearningExperienceEngine.ToggleObject _annotationToEdit;
    private int _resetOption = 0;

    public void SetAnnotationStartingPoint(Transform startingPoint)
    {
        _annotationStartingPoint = startingPoint;
    }

    public void Create()
    {
        if (_annotationToEdit != null)
        {
            // annotationToEdit.predicate = "pickandplace";
            LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);
        }
        else
        {
            Detectable detectable = _workplaceManager.GetDetectable(_workplaceManager.GetPlaceFromTaskStationId(_action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = MirageXR.Utilities.CalculateOffset(_annotationStartingPoint.transform.position,
                _annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            _annotationToEdit = _augmentationManager.AddAugmentation(_action, offset);
            _annotationToEdit.predicate = "pickandplace";
        }
        _annotationToEdit.text = _textInputField.text;
        _annotationToEdit.key = "1";

        CreateOrRemoveTrigger(_isCorrectTrigger, TriggerMode.PickAndPlace, "correct", _correctStepIndex.text);
        CreateOrRemoveTrigger(_isIncorrectTrigger, TriggerMode.IncorrectPickAndPlace, "incorrect", _incorrectStepIndex.text);

        LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
        LearningExperienceEngine.EventManager.NotifyActionModified(_action);

        Close();
    }

    public void Close()
    {
        _action = null;
        _annotationToEdit = null;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
    {
        gameObject.SetActive(true);
        CloseTriggerSettings();
        this._action = action;
        _annotationToEdit = annotation;
        _textInputField.text = annotation != null ? annotation.text : string.Empty;
        _isCorrectTrigger = false;
        _isIncorrectTrigger = false;

        if (_annotationToEdit != null)
        {
            _textInputField.text = _annotationToEdit.text;

            _resetOption = int.Parse(_annotationToEdit.key);
            _resetOnDropDown.value = _resetOption;

            _isCorrectTrigger = OpenTriggerInfo("correct", _correctToggle, _correctStepIndex);
            _isIncorrectTrigger = OpenTriggerInfo("incorrect", _incorrectToggle, _incorrectStepIndex);
        }
    }

    public void SetResetOption(Dropdown option)
    {
        _resetOption = option.value;
    }

    public void OnCorrectToggleChanged()
    {
        _isCorrectTrigger = ToggleChanged(_correctToggle, _correctStepIndex);
    }

    public void OnIncorrectToggleChanged()
    {
        _isIncorrectTrigger = ToggleChanged(_incorrectToggle, _incorrectStepIndex);
    }

    public bool ToggleChanged(Toggle toggle, InputField stepIndex)
    {
        if (toggle.isOn)
        {
            var isMorethanOneStep = IsMorethanOneStep();

            stepIndex.interactable = isMorethanOneStep;
            toggle.isOn = isMorethanOneStep;
            return isMorethanOneStep;
        }
        else
        {
            stepIndex.interactable = false;
            return false;
        }
    }

    private bool IsMorethanOneStep()
    {
        var numberOfSteps = _activityManager.ActionsOfTypeAction.Count;

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

    public void OpenTriggerSettings()
    {
        _editor.SetActive(false);
        _triggerSettings.SetActive(true);
    }

    public void CloseTriggerSettings()
    {
        _editor.SetActive(true);
        _triggerSettings.SetActive(false);
    }

    public void OnCorrectStepIndexValueChanged()
    {
        CheckInputedIndex(_correctStepIndex);
    }

    public void OnIncorrectStepIndexValueChanged()
    {
        CheckInputedIndex(_incorrectStepIndex);
    }

    private void CheckInputedIndex(InputField inputedIndex)
    {
        var numberOfSteps = _activityManager.ActionsOfTypeAction.Count;

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

    private void CreateOrRemoveTrigger(bool isTrigger, TriggerMode triggerMode, string suffix, string index)
    {
        if (isTrigger)
        {
            _action.AddOrReplaceArlemTrigger(triggerMode, ActionType.PickAndPlace, _annotationToEdit.poi + suffix, 1, index);
        }
        else
        {
            _action.RemoveArlemTrigger(_annotationToEdit.poi + suffix);
        }
    }

    private bool OpenTriggerInfo(string suffix, Toggle toggle, InputField inputField)
    {
        var trigger = _activityManager.ActiveAction.triggers.Find(t => t.id == _annotationToEdit.poi + suffix);

        var isTrigger = trigger != null ? true : false;

        if (isTrigger)
        {
            toggle.isOn = isTrigger;
            inputField.text = trigger.value;
        }

        return isTrigger;
    }
}
