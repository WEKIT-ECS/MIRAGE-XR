using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using UnityEngine;
using Action = MirageXR.Action;

[RequireComponent(typeof(TaskStationStateController), typeof(ObjectManipulator))]
public class TaskStationEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    private ObjectManipulator _objectManipulator;
    private TaskStationStateController _taskStationStateController;
    private PlaceBehaviour _placeBehaviour;
    private Detectable _detectable;

    private void Awake()
    {
        _objectManipulator = GetComponent<ObjectManipulator>();
        _taskStationStateController = GetComponent<TaskStationStateController>();
    }

    private void Start()
    {
        _placeBehaviour = transform.parent.parent.gameObject.GetComponent<PlaceBehaviour>();
        _detectable = RootObject.Instance.workplaceManager.GetDetectable(_placeBehaviour.Place);
        _objectManipulator.HostTransform = GameObject.Find(_detectable.id).transform;
        _objectManipulator.enabled = activityManager.EditModeActive;
    }

    private void OnEnable()
    {
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += OnEditModeChanged;

        _objectManipulator.enabled = activityManager.EditModeActive;

        EventManager.NotifyOnTaskStationEditorEnabled();
    }

    private void OnDisable()
    {
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && editModeActive;
    }

    public void OnManipulationEnded()
    {
        var position = _objectManipulator.HostTransform.localPosition;
        var rotation = _objectManipulator.HostTransform.localRotation;

        _detectable.origin_position = Utilities.Vector3ToString(position);
        _detectable.origin_rotation = Utilities.Vector3ToString(rotation.eulerAngles);

        EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void OnActionChanged(Action action)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && activityManager.EditModeActive;
    }
}
