using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditor : MonoBehaviour
{
    private static AugmentationManager _augmentationManager => RootObject.Instance.augmentationManager;
    private static ActivityManager _activityManager => RootObject.Instance.activityManager;
    private static WorkplaceManager _workplaceManager => RootObject.Instance.workplaceManager;

    [SerializeField] private Transform _annotationStartingPoint;
    [SerializeField] private InputField _textInputField;

    [SerializeField] private Toggle _toggleTrigger;
    [SerializeField] private InputField _triggerStepIndex;
    [SerializeField] private InputField _triggerStepTime;
    [SerializeField] private GameObject _triggerHelp;
    [SerializeField] private GameObject _triggerIndexHelp;
    [SerializeField] private GameObject _triggerTimeHelp;
    [SerializeField] private Dropdown _resetOnDropDown;

    [SerializeField] private GameObject _editor;
    [SerializeField] private GameObject _triggerSettings;

    [SerializeField] private Text _hoverGuide;

    private const string _triggerHelpText = "This toggle gives this pick and place augmentation a trigger, allowing you to jump to another step once placed correctly";
    private const string _triggerIndexHelpText = "Use this input box to enter the step the trigger should take you to";
    private const string _triggerTimeHelpText = "use this input box to determine the time between correct placement and moving onto the set step";

    private bool _isTrigger;
    private Action _action;
    private ToggleObject _annotationToEdit;
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
            EventManager.DeactivateObject(_annotationToEdit);
        }
        else
        {
            Detectable detectable = _workplaceManager.GetDetectable(_workplaceManager.GetPlaceFromTaskStationId(_action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = Utilities.CalculateOffset(_annotationStartingPoint.transform.position,
                _annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            _annotationToEdit = _augmentationManager.AddAugmentation(_action, offset);
            _annotationToEdit.predicate = "pickandplace";
        }
        _annotationToEdit.text = _textInputField.text;
        _annotationToEdit.key = _resetOption.ToString();

        if (_isTrigger)
        {
            _action.AddOrReplaceArlemTrigger(TriggerMode.PickAndPlace, ActionType.PickAndPlace, _annotationToEdit.poi, int.Parse(_triggerStepTime.text), _triggerStepIndex.text);
        }
        else
        {
            _action.RemoveArlemTrigger(_annotationToEdit);
        }

        EventManager.ActivateObject(_annotationToEdit);
        EventManager.NotifyActionModified(_action);

        Close();
    }

    public void Close()
    {
        _action = null;
        _annotationToEdit = null;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(Action action, ToggleObject annotation)
    {
        gameObject.SetActive(true);
        CloseTriggerSettings();
        this._action = action;
        _annotationToEdit = annotation;
        _textInputField.text = annotation != null ? annotation.text : string.Empty;
        _isTrigger = false;

        AddHoverGuide(_triggerHelp, _triggerHelpText);
        AddHoverGuide(_triggerIndexHelp, _triggerIndexHelpText);
        AddHoverGuide(_triggerTimeHelp, _triggerTimeHelpText);

        if (_annotationToEdit != null)
        {
            _textInputField.text = _annotationToEdit.text;

            _resetOption = int.Parse(_annotationToEdit.key);
            _resetOnDropDown.value = _resetOption;

            var trigger = _activityManager.ActiveAction.triggers.Find(t => t.id == _annotationToEdit.poi);
            _isTrigger = trigger != null ? true : false;
            if (_isTrigger)
            {
                _toggleTrigger.isOn = _isTrigger;
                _triggerStepTime.text = trigger.duration.ToString();
                _triggerStepIndex.text = trigger.value;
            }
        }
    }

    public void SetResetOption(int option)
    {
        _resetOption = option;
    }

    public void TriggerToggle(bool trigger)
    {
        var numberOfSteps = _activityManager.ActionsOfTypeAction.Count;

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

    public void OnStepTriggerValueChanged()
    {
        var numberOfSteps = _activityManager.ActionsOfTypeAction.Count;

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

    private void AddHoverGuide(GameObject obj, string hoverMessage)
    {
        var HoverGuilde = obj.AddComponent<HoverGuilde>();
        HoverGuilde.SetGuildText(_hoverGuide);
        HoverGuilde.SetMessage(hoverMessage);
    }
}
