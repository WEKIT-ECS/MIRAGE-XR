using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class PickAndPlaceEditor : MonoBehaviour
{
    private static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;
    private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;

    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private InputField textInputField;


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
            //annotationToEdit.predicate = "pickandplace";
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

    public void setResetOption(int option)
    {
        resetOption = option;
    }

}
