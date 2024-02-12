using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using UnityEngine;
using Action = MirageXR.Action;

[RequireComponent(typeof(TaskStationStateController), typeof(ObjectManipulator))]
public class TaskStationEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    private static GridManager gridManager => RootObject.Instance.gridManager;

    private ObjectManipulator _objectManipulator;
    private TaskStationStateController _taskStationStateController;
    private PlaceBehaviour _placeBehaviour;
    private Detectable _detectable;
    private bool _isInitialized;

    private void Awake()
    {
        _objectManipulator = GetComponent<ObjectManipulator>();
        _taskStationStateController = GetComponent<TaskStationStateController>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (_isInitialized)
        {
            return;
        }
        
        _placeBehaviour = transform.parent.parent.gameObject.GetComponent<PlaceBehaviour>();
        _detectable = RootObject.Instance.workplaceManager.GetDetectable(_placeBehaviour.Place);
        _objectManipulator.HostTransform = GameObject.Find(_detectable.id).transform;
        _objectManipulator.enabled = activityManager.EditModeActive;
        _objectManipulator.OnManipulationStarted.AddListener(_ => gridManager.onManipulationStarted(_objectManipulator.HostTransform.gameObject));
        _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        _isInitialized = true;
    }

    private void OnEnable()
    {
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += OnEditModeChanged;

        EventManager.OnWorkplaceCalibrated += OnCalibrationFinished;

        _objectManipulator.enabled = activityManager.EditModeActive;

        EventManager.NotifyOnTaskStationEditorEnabled();
    }

    private void OnDisable()
    {
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
        EventManager.OnWorkplaceCalibrated -= OnCalibrationFinished;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && editModeActive;
    }

    private void OnCalibrationFinished()
    {
        RecordTaskStationPosition();
    }

    private void OnManipulationEnded(ManipulationEventData eventData)
    {
        var source = _objectManipulator.HostTransform.gameObject;
        gridManager.onManipulationEnded(source);

        RecordTaskStationPosition();
        EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void RecordTaskStationPosition()
    {
        if (_detectable == null)
        {
            Debug.LogError("can't find '_detectable'");
            return;
        }
        
        var source = _objectManipulator.HostTransform.gameObject;
        var anchor = RootObject.Instance.calibrationManager.anchor;

        var position = anchor.InverseTransformPoint(source.transform.position);
        var rotation = Quaternion.Inverse(anchor.rotation) * source.transform.rotation;

        _detectable.origin_position = Utilities.Vector3ToString(position);
        _detectable.origin_rotation = Utilities.Vector3ToString(rotation.eulerAngles);

        activityManager.SaveData();
    }
    
    private void OnActionChanged(Action action)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && activityManager.EditModeActive;
    }
}
