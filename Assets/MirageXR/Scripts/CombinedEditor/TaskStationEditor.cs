using LearningExperienceEngine;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using Newtonsoft.Json;
//using UnityEditor;
//using UnityEditor.Search;
using UnityEngine;
using Action = LearningExperienceEngine.Action;

[RequireComponent(typeof(TaskStationStateController), typeof(ObjectManipulator))]
public class TaskStationEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    private static GridManager gridManager => RootObject.Instance.GridManager;

    private ObjectManipulator _objectManipulator;
    private TaskStationStateController _taskStationStateController;
    private MeshRenderer _meshRenderer;
    private GameObject _mTaskStationNumberTag;
    private LearningExperienceEngine.Detectable _detectable;
    private LearningExperienceEngine.Action _action;

    public void Init(LearningExperienceEngine.Action action)
    {
        _objectManipulator = GetComponent<ObjectManipulator>();
        _taskStationStateController = GetComponent<TaskStationStateController>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _objectManipulator.enabled = activityManager.EditModeActive;
        _objectManipulator.OnManipulationStarted.AddListener(_ => gridManager.onManipulationStarted(_objectManipulator.HostTransform.gameObject));
        _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        _action = action;

        var id = _action == null ? transform.parent.parent.name : _action.id;
        
        var detectableId = id.Replace("TS-", "WA-");
        _detectable = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.GetDetectable(detectableId);
        _objectManipulator.HostTransform = GameObject.Find(_detectable.id).transform;

        try
        {
            _mTaskStationNumberTag = TaskStationDetailMenu.Instance.transform.Find("Pivot/DiamondTopMenu/NumberGB").gameObject;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[tse] Could not find NumberGB game object to activate/deactivate number displayed above task station diamond: {e.Message}");
        }

        UpdateView();
    }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnWorkplaceCalibrated += OnCalibrationFinished;

        MirageXR.EventManager.NotifyOnTaskStationEditorEnabled();
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnWorkplaceCalibrated -= OnCalibrationFinished;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        _objectManipulator.enabled = _taskStationStateController.IsCurrent() && editModeActive;
    }

    private void UpdateView()
    {
        _meshRenderer.enabled = _action?.isDiamondVisible ?? true;
        if (_mTaskStationNumberTag != null)
        {
            _mTaskStationNumberTag.SetActive(_action?.isDiamondVisible ?? true);
        }
    }

    public void OnVisibilityChanged(bool value)
    {
        if (_action == null)
        {
            return;
        }

        _action.isDiamondVisible = value;
        _meshRenderer.enabled = value;
        if (_mTaskStationNumberTag != null)
        {
            _mTaskStationNumberTag.SetActive(value);
        }
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
        MirageXR.EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void RecordTaskStationPosition()
    {
        var source = _objectManipulator.HostTransform.gameObject;
        var anchor = RootObject.Instance.CalibrationManager.Anchor;

        var position = anchor.InverseTransformPoint(source.transform.position);
        var rotation = Quaternion.Inverse(anchor.rotation) * source.transform.rotation;

        _detectable.origin_position = MirageXR.Utilities.Vector3ToString(position);
        _detectable.origin_rotation = MirageXR.Utilities.Vector3ToString(rotation.eulerAngles);

        activityManager.SaveData();
    }
}
