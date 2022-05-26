using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using System.Globalization;
using UnityEngine;

public class TaskStationEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    
    [SerializeField] private ObjectManipulator taskStationMover;

    private PlaceBehaviour placeBehaviour;
    private Detectable associatedDetectable;

    private void Start()
    {
        placeBehaviour = transform.parent.parent.gameObject.GetComponent<PlaceBehaviour>();
        associatedDetectable = RootObject.Instance.workplaceManager.GetDetectable(placeBehaviour.Place);
        // make the entire task station move if the visual part is moved
        taskStationMover.HostTransform = GameObject.Find(associatedDetectable.id).transform;
        SetEditModeState(activityManager.EditModeActive);
    }

    private void OnEnable()
    {
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += SetEditModeState;
        if (activityManager != null)
        {
            SetEditModeState(activityManager.EditModeActive);
        }

        EventManager.NotifyOnTaskStationEditorEnabled();
    }

    private void OnDisable()
    {
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= SetEditModeState;
    }

    public void SetTaskStationMover(bool status)
    {
        taskStationMover.enabled = status;
    }

    private void SetEditModeState(bool editModeActive)
    {
        taskStationMover.enabled = GetComponent<TaskStationStateController>().IsCurrent() && editModeActive;
    }

    public void OnManipulationEnded()
    {
        var position = taskStationMover.HostTransform.position;
        var rotation = taskStationMover.HostTransform.eulerAngles;

        var originT = CalibrationTool.Instance.transform;

        // Some black magic for getting the offset.
        var anchorDummy = new GameObject("AnchorDummy");
        var targetDummy = new GameObject("TargetDummy");

        anchorDummy.transform.position = position;
        anchorDummy.transform.rotation = Quaternion.Euler(rotation);
        targetDummy.transform.position = originT.transform.position;
        targetDummy.transform.eulerAngles = originT.transform.eulerAngles;

        anchorDummy.transform.SetParent(targetDummy.transform);

        var myPos = anchorDummy.transform.localPosition;
        var myRot = Utilities.ConvertEulerAngles(anchorDummy.transform.localEulerAngles);

        Destroy(anchorDummy);
        Destroy(targetDummy);

        associatedDetectable.origin_position = $"{myPos.x.ToString(CultureInfo.InvariantCulture)}, {myPos.y.ToString(CultureInfo.InvariantCulture)}, {myPos.z.ToString(CultureInfo.InvariantCulture)}";
        associatedDetectable.origin_rotation = $"{myRot.x.ToString(CultureInfo.InvariantCulture)}, {myRot.y.ToString(CultureInfo.InvariantCulture)}, {myRot.z.ToString(CultureInfo.InvariantCulture)}";

        EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void OnActionChanged(Action action)
    {
        foreach (var taskStation in FindObjectsOfType<TaskStationStateController>())
        {
            var taskStationEditor = taskStation.GetComponent<TaskStationEditor>();

            if (taskStationEditor == null) continue;

            taskStationEditor.SetTaskStationMover(taskStation.IsCurrent() && activityManager.EditModeActive);
        }
    }
}
