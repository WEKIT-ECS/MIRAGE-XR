using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditor : MonoBehaviour
{
    private static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;

    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private InputField textInputField;

    [SerializeField] private Toggle toggleTrigger;
    [SerializeField] private InputField triggerStepIndex;
    [SerializeField] private InputField triggerStepTime;
    [SerializeField] private GameObject triggerHelp;
    [SerializeField] private GameObject triggerIndexHelp;
    [SerializeField] private GameObject triggerTimeHelp;

    [SerializeField] private GameObject editor;
    [SerializeField] private GameObject triggerSettings;

    [SerializeField] private Text hoverGuide;

    private const string TriggerHelpText = "This toggle gives this pick and place augmentation a trigger, allowing you to jump to another step once placed correctly";
    private const string TriggerIndexHelpText = "Use this input box to enter the step the trigger should take you to";
    private const string TriggerTimeHelpText = "use this input box to determine the time between correct placement and moving onto the set step";

    private bool isTrigger;
    private Action action;
    private ToggleObject annotationToEdit;
    private int resetOption = 0;

    public void SetAnnotationStartingPoint(Transform startingPoint)
    {
        annotationStartingPoint = startingPoint;
    }

    public void Create()
    {
        if (annotationToEdit != null)
        {
            // annotationToEdit.predicate = "pickandplace";
            EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            annotationToEdit = augmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "pickandplace";
        }
        annotationToEdit.text = textInputField.text;
        annotationToEdit.key = resetOption.ToString();

        if (isTrigger)
        {
            action.AddOrReplaceArlemTrigger(TriggerMode.PickAndPlace, ActionType.PickAndPlace, annotationToEdit.poi, int.Parse(triggerStepTime.text), triggerStepIndex.text);
        }
        else
        {
            action.RemoveArlemTrigger(annotationToEdit);
        }

        EventManager.ActivateObject(annotationToEdit);
        EventManager.NotifyActionModified(action);

        Close();
    }

    public void Close()
    {
        action = null;
        annotationToEdit = null;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(Action action, ToggleObject annotation)
    {
        gameObject.SetActive(true);
        CloseTriggerSettings();
        this.action = action;
        annotationToEdit = annotation;
        textInputField.text = annotation != null ? annotation.text : string.Empty;
        isTrigger = false;

        AddHoverGuide(triggerHelp, TriggerHelpText);
        AddHoverGuide(triggerIndexHelp, TriggerIndexHelpText);
        AddHoverGuide(triggerTimeHelp, TriggerTimeHelpText);

        if (annotationToEdit != null)
        {
            var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == annotationToEdit.poi);
            isTrigger = trigger != null ? true : false;
            if (isTrigger)
            {
                toggleTrigger.isOn = isTrigger;
                triggerStepTime.text = trigger.duration.ToString();
                triggerStepIndex.text = trigger.value;
            }
        }
    }

    public void setResetOption(int option)
    {
        resetOption = option;
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
    }

    public void OpenTriggerSettings()
    {
        editor.SetActive(false);
        triggerSettings.SetActive(true);
    }

    public void CloseTriggerSettings()
    {
        editor.SetActive(true);
        triggerSettings.SetActive(false);
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

    private void AddHoverGuide(GameObject obj, string hoverMessage)
    {
        var HoverGuilde = obj.AddComponent<HoverGuilde>();
        HoverGuilde.SetGuildText(hoverGuide);
        HoverGuilde.SetMessage(hoverMessage);
    }
}
