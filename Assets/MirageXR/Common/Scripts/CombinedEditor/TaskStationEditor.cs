using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TaskStationEditor : MonoBehaviour
{
    [SerializeField] private ObjectManipulator taskStationMover;

    private PlaceBehaviour placeBehaviour;
    private Detectable associatedDetectable;

    private void Start()
    {
        placeBehaviour = transform.parent.parent.gameObject.GetComponent<PlaceBehaviour>();
        associatedDetectable = WorkplaceManager.Instance.GetDetectable(placeBehaviour.Place);
        // make the entire task station move if the visual part is moved
        taskStationMover.HostTransform = GameObject.Find(associatedDetectable.id).transform;
        SetEditModeState(ActivityManager.Instance.EditModeActive);
    }

    private void OnEnable()
    {
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += SetEditModeState;
        if (ActivityManager.Instance != null)
        {
            SetEditModeState(ActivityManager.Instance.EditModeActive);
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
        if (GetComponent<TaskStationStateController>().IsCurrent())
            taskStationMover.enabled = editModeActive;
        else
            taskStationMover.enabled = false;
    }

    public void OnManipulationEnded()
    {
        Vector3 position = taskStationMover.HostTransform.position;
        Vector3 rotation = taskStationMover.HostTransform.eulerAngles;

        Transform originT = CalibrationTool.Instance.transform;
        Vector3 myPos;
        Vector3 myRot;

        // Some black magic for getting the offset.
        var anchorDummy = new GameObject("AnchorDummy");
        var targetDummy = new GameObject("TargetDummy");

        anchorDummy.transform.position = position;
        anchorDummy.transform.rotation = Quaternion.Euler(rotation);
        targetDummy.transform.position = originT.transform.position;
        targetDummy.transform.eulerAngles = originT.transform.eulerAngles;

        anchorDummy.transform.SetParent(targetDummy.transform);

        myPos = anchorDummy.transform.localPosition;
        myRot = Utilities.ConvertEulerAngles(anchorDummy.transform.localEulerAngles);

        Destroy(anchorDummy);
        Destroy(targetDummy);

        associatedDetectable.origin_position = $"{myPos.x.ToString(CultureInfo.InvariantCulture)}, {myPos.y.ToString(CultureInfo.InvariantCulture)}, {myPos.z.ToString(CultureInfo.InvariantCulture)}";
        associatedDetectable.origin_rotation = $"{myRot.x.ToString(CultureInfo.InvariantCulture)}, {myRot.y.ToString(CultureInfo.InvariantCulture)}, {myRot.z.ToString(CultureInfo.InvariantCulture)}";

        EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void OnActionChanged(Action action)
    {
        foreach (var taskstation in FindObjectsOfType<TaskStationStateController>())
        {
            var taskStationEditor = taskstation.GetComponent<TaskStationEditor>();

            if (taskStationEditor == null) continue;

            if (taskstation.IsCurrent())
                taskStationEditor.SetTaskStationMover(ActivityManager.Instance.EditModeActive);
            else
                taskStationEditor.SetTaskStationMover(false);
        }

    }
}
