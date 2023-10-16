using System;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupEditorBase : PopupBase
{
    protected const string HTTP_PREFIX = "http://";
    protected const string RESOURCES_PREFIX = "resources://";

    protected static ActivityManager activityManager => RootObject.Instance.activityManager;

    protected static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;

    [SerializeField] protected Image _icon;
    [SerializeField] protected TMP_Text _txtLabel;
    [SerializeField] protected Button _btnAccept;
    [SerializeField] protected Button _btnClose;

    public abstract ContentType editorForType { get; }

    protected ToggleObject _content;
    protected MirageXR.Action _step;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _canBeClosedByOutTap = false;
        _btnAccept.onClick.AddListener(OnAccept);
        _btnClose.onClick.AddListener(Close);
        UpdateBaseView();
    }

    protected abstract void OnAccept();

    protected virtual void UpdateBaseView()
    {
        _icon.sprite = editorForType.GetIcon();
        _txtLabel.text = editorForType.GetName();
    }

    protected virtual Vector3 GetOffset()
    {
        var workplaceManager = RootObject.Instance.workplaceManager;
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
            _step = (MirageXR.Action)args[0];
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            _content = (ToggleObject)args[1];
        }
        catch (Exception) { /**/ }
        return true;
    }
}