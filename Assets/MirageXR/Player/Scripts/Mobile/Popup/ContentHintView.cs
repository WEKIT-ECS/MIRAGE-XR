using LearningExperienceEngine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentHintView : PopupBase
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _txtLabel;
    [SerializeField] private TMP_Text _txtHint;

    private LearningExperienceEngine.ContentType _type;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _canBeClosedByOutTap = true;
        base.Initialization(onClose, args);
        UpdateView();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _type = (LearningExperienceEngine.ContentType)args[0];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void UpdateView()
    {
        _image.sprite = _type.GetIcon();
        _txtLabel.text = _type.GetName();
        _txtHint.text = _type.GetHint();

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
