using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepTrigger : MonoBehaviour
    {
        [SerializeField] private Toggle stepTriggerToggle;
        [SerializeField] private InputField durationInputField;
        [SerializeField] private InputField stepNumberInputField;

        public ToggleObject MyPoi { get; set; }

        public void Initiate(ToggleObject annotation, float duration, string stepNumber)
        {
            MyPoi = annotation;
            durationInputField.text = duration.ToString();
            stepNumberInputField.text = stepNumber;
            stepTriggerToggle.isOn =  ActivityManager.Instance.ActionsOfTypeAction.Count != 1 && IsTrigger();
            durationInputField.interactable = stepTriggerToggle.isOn;
            stepNumberInputField.interactable = stepTriggerToggle.isOn;
        }

        private bool IsTrigger() {

            if (MyPoi == null) return false;

            return ActivityManager.Instance.ActiveAction.triggers.Find(t => t.id == MyPoi.poi) != null;
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
                if( ActivityManager.Instance.ActionsOfTypeAction.Count == 1)
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
                ActivityManager.Instance.ActiveAction.RemoveArlemTrigger(MyPoi);
            }
        }

        private void OnStepNumberValueChanged()
        {
            var stepNumber = int.Parse(stepNumberInputField.text);
            if (stepNumber > ActivityManager.Instance.ActionsOfTypeAction.Count)
            {
                stepNumberInputField.text = ActivityManager.Instance.ActionsOfTypeAction.Count.ToString();
            }
            else if(stepNumber < 1)
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
            var triggerTpye = MyPoi.predicate.Contains(":") ? MyPoi.predicate.Split(':')[0] : MyPoi.predicate;
            var activeAction = ActivityManager.Instance.ActiveAction;

            if (stepTriggerToggle.isOn)
            {
                if (activeAction.enter.activates.Contains(MyPoi))
                {
                    if (!IsTrigger())
                    {
                        activeAction.AddArlemTrigger("detect", triggerTpye, MyPoi.poi, float.Parse(durationInputField.text), stepNumberInputField.text);
                    }
                    else
                    {
                        var trigger = ActivityManager.Instance.ActiveAction.triggers.Find(t => t.id == MyPoi.poi);
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
