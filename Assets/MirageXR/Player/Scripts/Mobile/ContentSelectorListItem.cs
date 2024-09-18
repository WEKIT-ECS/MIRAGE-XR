using LearningExperienceEngine;
using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentSelectorListItem : MonoBehaviour
{
    [SerializeField] private Button _btnMain;
    [SerializeField] private Button _btnHint;
    [SerializeField] private Button _btnTutorial;
    [SerializeField] private TMP_Text _txtContent;
    [SerializeField] private Image _image;

    private LearningExperienceEngine.ContentType _type;

    public void Init(LearningExperienceEngine.ContentType type, Action<LearningExperienceEngine.ContentType> onSelected, Action<LearningExperienceEngine.ContentType> onHintClick)
    {
        _type = type;
        _btnMain.onClick.AddListener(() => onSelected(_type));
        _btnHint.onClick.AddListener(() => onHintClick(_type));
        _btnTutorial.onClick.AddListener(() => onSelected(_type)); // Augmentation tutorials start as if clicked
        _btnTutorial.onClick.AddListener(() => TutorialManager.Instance.StartAugmentationTutorial(type));
        UpdateView();
    }

    private void UpdateView()
    {
        _image.sprite = _type.GetIcon();
        _txtContent.text = _type.GetName();
    }
}