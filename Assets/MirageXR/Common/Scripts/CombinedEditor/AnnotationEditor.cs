using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationEditor : MonoBehaviour
{
    [SerializeField] private Button deleteAnnotation;
    [SerializeField] private GameObject lifeIcon;

    private AnnotationListItem annotationListItem;

    private void Awake()
    {
        annotationListItem = GetComponent<AnnotationListItem>();
    }

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += SetEditModeState;
        if (ActivityManager.Instance != null)
        {
            SetEditModeState(ActivityManager.Instance.EditModeActive);
        }
    }

    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= SetEditModeState;
    }

    private void Start()
    {
        SetEditModeState(ActivityManager.Instance.EditModeActive);
    }

    private void SetEditModeState(bool editModeActive)
    {
        deleteAnnotation.gameObject.SetActive(editModeActive);
        lifeIcon.SetActive(editModeActive);
    }

    public void DeleteAnnotation()
    {
        //hide the binder if is linked to deleted button
        if (TaskStationDetailMenu.Instance.SelectedButton == GetComponent<Button>())
            TaskStationDetailMenu.Instance.SelectedButton = null;

        ActivityManager.Instance.DeleteAnnotation( annotationListItem.DisplayedAnnotation);
        
    }
}
