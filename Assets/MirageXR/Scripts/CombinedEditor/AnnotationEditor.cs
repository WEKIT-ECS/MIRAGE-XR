using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    [SerializeField] private Button deleteAnnotation;
    [SerializeField] private GameObject lifeIcon;

    private AnnotationListItem annotationListItem;

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += SetEditModeState;
        if (activityManager != null)
        {
            SetEditModeState(activityManager.EditModeActive);
        }
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= SetEditModeState;
    }

    private void Start()
    {
        annotationListItem = GetComponent<AnnotationListItem>();
        SetEditModeState(activityManager.EditModeActive);
    }

    private void SetEditModeState(bool editModeActive)
    {
        deleteAnnotation.gameObject.SetActive(editModeActive);
        lifeIcon.SetActive(editModeActive);
    }

    public void DeleteAnnotation()
    {
        // hide the binder if is linked to deleted button
        if (TaskStationDetailMenu.Instance.SelectedButton == GetComponent<Button>())
            TaskStationDetailMenu.Instance.SelectedButton = null;

        LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.DeleteAugmentation(annotationListItem.DisplayedAnnotation);

    }

    public void LockAugmentation()
    {
        LearningExperienceEngine.EventManager.NotifyAugmentationLocked(annotationListItem.DisplayedAnnotation.poi, !annotationListItem.DisplayedAnnotation.positionLock);
    }
}
