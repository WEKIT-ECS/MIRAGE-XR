using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using MirageXR;

public class HintViewWithButtonAndToggle : PopupBase
{

    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Button _buttonGotIt;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Toggle _toggle;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _buttonGotIt.onClick.AddListener(Close);
        _toggle.onValueChanged.AddListener(DontShowAgain);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void DontShowAgain(bool value)
    {
        LearningExperienceEngine.UserSettings.dontShowNewAugmentationHint = value;
    }
}