using System;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using Newtonsoft.Json;
using UnityEngine;
using Action = MirageXR.Action;

[RequireComponent(typeof(TaskStationStateController), typeof(ObjectManipulator))]
public class TaskStationEditor : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    private static GridManager gridManager => RootObject.Instance.gridManager;

    private ObjectManipulator _objectManipulator;
    private TaskStationStateController _taskStationStateController;
    private MeshRenderer _meshRenderer;
    private Detectable _detectable;
    private Action _action;

    public void Init(Action action)
    {
        _objectManipulator = GetComponent<ObjectManipulator>();
        _taskStationStateController = GetComponent<TaskStationStateController>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _objectManipulator.enabled = activityManager.EditModeActive;
        _objectManipulator.OnManipulationStarted.AddListener(_ => gridManager.onManipulationStarted(_objectManipulator.HostTransform.gameObject));
        _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        _action = action;

        var detectableId = _action.id.Replace("TS-", "WA-");
        _detectable = RootObject.Instance.workplaceManager.GetDetectable(detectableId);
        _objectManipulator.HostTransform = GameObject.Find(_detectable.id).transform;

        UpdateView();
    }

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += OnEditModeChanged;
        EventManager.OnWorkplaceCalibrated += OnCalibrationFinished;

        EventManager.NotifyOnTaskStationEditorEnabled();
    }

    private void OnDisable()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
        EventManager.OnWorkplaceCalibrated -= OnCalibrationFinished;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && editModeActive;
    }

    private void UpdateView()
    {
        _meshRenderer.enabled = _action.isDiamondVisible ?? true;
    }

    public void OnVisibilityChanged(bool value)
    {
        if (_action == null)
        {
            return;
        }

        _action.isDiamondVisible = value;
        _meshRenderer.enabled = value;
        activityManager.SaveData();
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
        var source = _objectManipulator.HostTransform.gameObject;
        var anchor = RootObject.Instance.calibrationManager.anchor;

        var position = anchor.InverseTransformPoint(source.transform.position);
        var rotation = Quaternion.Inverse(anchor.rotation) * source.transform.rotation;

        _detectable.origin_position = MirageXR.Utilities.Vector3ToString(position);
        _detectable.origin_rotation = MirageXR.Utilities.Vector3ToString(rotation.eulerAngles);

        activityManager.SaveData();
    }
}
