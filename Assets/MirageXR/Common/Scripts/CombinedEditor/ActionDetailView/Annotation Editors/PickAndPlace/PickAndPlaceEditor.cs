using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditor : MonoBehaviour
{
    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private InputField textInputField;
    
    private Action action;
    private ToggleObject annotationToEdit;

    public void SetAnnotationStartingPoint(Transform startingPoint)
    {
        annotationStartingPoint = startingPoint;
    }

    public void Create()
    {
        if (annotationToEdit != null)
        {
            //annotationToEdit.predicate = "pickandplace";
            EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            annotationToEdit = RootObject.Instance.augmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "pickandplace";
        }
        annotationToEdit.text = textInputField.text;

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
        this.action = action;
        annotationToEdit = annotation;
        textInputField.text = annotation != null ? annotation.text : string.Empty;
    }
}
