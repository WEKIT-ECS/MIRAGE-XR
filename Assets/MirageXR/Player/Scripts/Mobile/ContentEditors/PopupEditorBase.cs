using LearningExperienceEngine;
using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupEditorBase : PopupBase
{
    protected const string HTTP_PREFIX = "http://";
    protected const string RESOURCES_PREFIX = "resources://";

    protected static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    protected static LearningExperienceEngine.AugmentationManager augmentationManager => LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager;

    [SerializeField] protected Image _icon;
    [SerializeField] protected TMP_Text _txtLabel;
    [SerializeField] protected Button _btnAccept;
    [SerializeField] protected Button _btnClose;

    public abstract LearningExperienceEngine.ContentType editorForType { get; }

    protected LearningExperienceEngine.ToggleObject _content;
    protected LearningExperienceEngine.Action _step;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _canBeClosedByOutTap = false;
        _btnAccept.onClick.AddListener(OnAccept);
        _btnClose.onClick.AddListener(Close);
        UpdateBaseView();
    }

    protected virtual void OnAccept()
    {
        LearningExperienceEngine.EventManager.NotifyActionModified(_step);
        LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
    }

    protected virtual void UpdateBaseView()
    {
        _icon.sprite = editorForType.GetIcon();
        _txtLabel.text = editorForType.GetName();
    }

    protected virtual Vector3 GetOffset()
    {
        var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
        var detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_step.id));
        var annotationStartingPoint = ActionEditor.Instance.GetDefaultAugmentationStartingPoint();
        var originT = GameObject.Find(detectable.id);   // TODO: replace by direct reference to the object
        if (!originT)
        {
            Debug.LogError($"Can't find detectable {detectable.id}");
            return annotationStartingPoint.transform.position;
        }

        var detectableBehaviour = originT.GetComponent<DetectableBehaviour>();

        if (!detectableBehaviour)
        {
            Debug.LogError($"Can't find DetectableBehaviour");
            return annotationStartingPoint.transform.position;
        }

        var attachedObject = detectableBehaviour.AttachedObject;
        return attachedObject.transform.InverseTransformPoint(annotationStartingPoint.transform.position);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _step = (LearningExperienceEngine.Action)args[0];
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            _content = (LearningExperienceEngine.ToggleObject)args[1];
        }
        catch (Exception) { /**/ }
        return true;
    }
}