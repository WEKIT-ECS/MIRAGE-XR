using LearningExperienceEngine;
using System;
using MirageXR;
using Newtonsoft.Json;
using UnityEngine;
using Action = LearningExperienceEngine.Action;

[RequireComponent(typeof(TaskStationStateController))]
public class TaskStationEditor : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    private static GridManager gridManager => RootObject.Instance ? RootObject.Instance.GridManager : null;

    private SimpleBoundingBox _boundingBox;
    private Transform _hostTransform;
    private TaskStationStateController _taskStationStateController;
    private MeshRenderer _meshRenderer;
    private GameObject _mTaskStationNumberTag;
    private LearningExperienceEngine.Detectable _detectable;
    private LearningExperienceEngine.Action _action;

    public void Init(LearningExperienceEngine.Action action)
    {
        _taskStationStateController = GetComponent<TaskStationStateController>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _action = action;

        var id = _action == null ? transform.parent.parent.name : _action.id;
        
        var detectableId = id.Replace("TS-", "WA-");
        _detectable = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.GetDetectable(detectableId);
        var detectableObj = _detectable != null ? GameObject.Find(_detectable.id) : null;
        _hostTransform = detectableObj != null ? detectableObj.transform : transform;

        _boundingBox = GetComponent<SimpleBoundingBox>();
        if (_boundingBox == null)
        {
            _boundingBox = gameObject.AddComponent<SimpleBoundingBox>();
        }
        _boundingBox.Target = _hostTransform;
        _boundingBox.SetEnabled(activityManager.EditModeActive);

        if (gridManager != null)
        {
            _boundingBox.TranslateStarted.AddListener(() => gridManager.onManipulationStarted(_hostTransform.gameObject));
            _boundingBox.TranslateStopped.AddListener(OnManipulationEnded);
            _boundingBox.RotateStarted.AddListener(() => gridManager.onManipulationStarted(_hostTransform.gameObject));
            _boundingBox.RotateStopped.AddListener(OnManipulationEnded);
        }

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
        if (_boundingBox != null)
        {
            _boundingBox.SetEnabled(_taskStationStateController != null && _taskStationStateController.IsCurrent() && editModeActive);
        }
    }

    private void UpdateView()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = _action?.isDiamondVisible ?? true;
        }
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
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = value;
        }
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

    private void OnManipulationEnded()
    {
        if (gridManager != null && _hostTransform != null)
        {
            gridManager.onManipulationEnded(_hostTransform.gameObject);
        }

        RecordTaskStationPosition();
        MirageXR.EventManager.NotifyOnTaskStationEditorDragEnd();
    }

    private void RecordTaskStationPosition()
    {
        if (_hostTransform == null || _detectable == null) return;
        var anchor = RootObject.Instance?.CalibrationManager?.Anchor;
        if (anchor == null) return;

        var position = anchor.InverseTransformPoint(_hostTransform.position);
        var rotation = Quaternion.Inverse(anchor.rotation) * _hostTransform.rotation;

        _detectable.origin_position = MirageXR.Utilities.Vector3ToString(position);
        _detectable.origin_rotation = MirageXR.Utilities.Vector3ToString(rotation.eulerAngles);

        activityManager.SaveData();
    }
}
