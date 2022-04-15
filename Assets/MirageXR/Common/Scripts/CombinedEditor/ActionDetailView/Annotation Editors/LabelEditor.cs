
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class LabelEditor : MonoBehaviour
    {
        [SerializeField] private InputField textInputField;
        [SerializeField] private Transform annotationStartingPoint;
        [SerializeField] private StepTrigger stepTrigger;
        [SerializeField] private GameObject acceptButton;

        public GameObject GetAcceptButton()
        {
            return acceptButton;
        }

        private Action action;
        private ToggleObject annotationToEdit;

        public void Close()
        {
            action = null;
            annotationToEdit = null;
            gameObject.SetActive(false);
            this.textInputField.onValueChanged.RemoveListener(delegate { EventManager.NotifyOnLabelEditorTextChanged(); });

            Destroy(gameObject);
        }

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            annotationStartingPoint = startingPoint;
        }

        public void Open(Action action, ToggleObject annotation)
        {
            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;

            if (annotationToEdit != null)
            {
                textInputField.text = annotationToEdit.text;
                var trigger = ActivityManager.Instance.ActiveAction.triggers.Find(t => t.id == annotationToEdit.poi);
                var duration = trigger != null ? trigger.duration : 1;
                var stepNumber = trigger != null ? trigger.value : "1";
                stepTrigger.Initiate(annotationToEdit, duration, stepNumber);
            }

            this.textInputField.onValueChanged.AddListener(delegate { EventManager.NotifyOnLabelEditorTextChanged(); });
            this.acceptButton = this.gameObject.transform.Find("AcceptButton").gameObject;
        }

        public void OnAccept()
        {

            if (annotationToEdit != null)
            {
                EventManager.DeactivateObject(annotationToEdit);
            }
            else
            {
                Detectable detectable = WorkplaceManager.Instance.GetDetectable(WorkplaceManager.Instance.GetPlaceFromTaskStationId(action.id));
                GameObject originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                    annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                annotationToEdit = ActivityManager.Instance.AddAnnotation(action, offset);
                annotationToEdit.predicate = "label";
            }
            annotationToEdit.text = textInputField.text;

            stepTrigger.MyPoi = annotationToEdit;
            stepTrigger.SetupTrigger();

            EventManager.ActivateObject(annotationToEdit);
            EventManager.NotifyActionModified(action);
            Close();
        }

    }
}
