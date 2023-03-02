using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepTrigger : MonoBehaviour
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        [SerializeField] private Toggle stepTriggerToggle;
        [SerializeField] private InputField durationInputField;
        [SerializeField] private InputField stepNumberInputField;

        public ToggleObject MyPoi { get; set; }

        public void Initiate(ToggleObject annotation, float duration, string stepNumber)
        {
            MyPoi = annotation;
            durationInputField.text = duration.ToString(CultureInfo.InvariantCulture);
            stepNumberInputField.text = stepNumber;
            stepTriggerToggle.isOn = activityManager.ActionsOfTypeAction.Count != 1 && IsTrigger();
            durationInputField.interactable = stepTriggerToggle.isOn;
            stepNumberInputField.interactable = stepTriggerToggle.isOn;
        }

        private bool IsTrigger()
        {

            if (MyPoi == null) return false;

            return activityManager.ActiveAction.triggers.Find(t => t.id == MyPoi.poi) != null;
        }

        private void Start()
        {
            stepTriggerToggle.onValueChanged.RemoveAllListeners();
            stepTriggerToggle.onValueChanged.AddListener(delegate { OnStepTriggerValueChanged(); });
            durationInputField.onEndEdit.RemoveAllListeners();
            durationInputField.onEndEdit.AddListener(delegate { OnDurationValueChanged(); });
            stepNumberInputField.onEndEdit.RemoveAllListeners();
            stepNumberInputField.onEndEdit.AddListener(delegate { OnStepNumberValueChanged(); });
        }

        private void OnStepTriggerValueChanged()
        {
            durationInputField.interactable = stepTriggerToggle.isOn;
            stepNumberInputField.interactable = stepTriggerToggle.isOn;

            if (stepTriggerToggle.isOn)
            {
                if (activityManager.ActionsOfTypeAction.Count == 1)
                {
                    // give the info and close
                    DialogWindow.Instance.Show("Info!",
                    "Only one step has been found in this activity!\n Add a new step and try again.",
                    new DialogButtonContent("Ok"));

                    stepTriggerToggle.isOn = false;
                    return;
                }

                if (MyPoi != null)
                {
                    SetupTrigger();
                }
            }
            else
            {
                activityManager.ActiveAction.RemoveArlemTrigger(MyPoi);
            }
        }

        private void OnStepNumberValueChanged()
        {
            var stepNumber = int.Parse(stepNumberInputField.text);
            if (stepNumber > activityManager.ActionsOfTypeAction.Count)
            {
                stepNumberInputField.text = activityManager.ActionsOfTypeAction.Count.ToString();
            }
            else if (stepNumber < 1)
            {
                stepNumberInputField.text = "1";
            }

            if (MyPoi == null) return;

            if (IsTrigger())
            {
                SetupTrigger();
            }
        }

        private void OnDurationValueChanged()
        {
            if (MyPoi == null) return;

            if (IsTrigger())
            {
                SetupTrigger();
            }
        }

        public void SetupTrigger()
        {
            var triggerType = MyPoi.predicate.Contains(":") ? MyPoi.predicate.Split(':')[0] : MyPoi.predicate;
            var activeAction = activityManager.ActiveAction;

            if (stepTriggerToggle.isOn)
            {
                if (activeAction.enter.activates.Contains(MyPoi))
                {
                    if (!IsTrigger())
                    {
                        if (!Enum.TryParse(triggerType, true, out ActionType type))
                        {
                            AppLog.LogWarning($"can't parse {triggerType} to ActionType");
                            type = ActionType.Action;
                        }
                        activeAction.AddArlemTrigger(TriggerMode.Detect, type, MyPoi.poi, float.Parse(durationInputField.text), stepNumberInputField.text);
                    }
                    else
                    {
                        var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == MyPoi.poi);
                        trigger.duration = float.Parse(durationInputField.text);
                        trigger.value = stepNumberInputField.text;
                    }
                }
            }
            else
            {
                if (IsTrigger())
                {
                    activeAction.RemoveArlemTrigger(MyPoi);
                }
            }
        }
    }
}
