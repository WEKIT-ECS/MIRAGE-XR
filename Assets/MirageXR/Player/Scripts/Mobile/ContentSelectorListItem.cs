using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

public class ContentSelectorListItem : MonoBehaviour
{
    [SerializeField] private Button _btnMain;
    [SerializeField] private Button _btnHint;
    [SerializeField] private Button _btnTutorial;
    [SerializeField] private TMP_Text _txtContent;
    [SerializeField] private Image _image;

    private ContentType _type;

    public void Init(ContentType type, Action<ContentType> onSelected, Action<ContentType> onHintClick)
    {
        _type = type;
        _btnMain.onClick.AddListener(() => onSelected(_type));
        _btnHint.onClick.AddListener(() => onHintClick(_type));
        _btnTutorial.onClick.AddListener(() => onSelected(_type)); // Augmentation tutorials start as if clicked
        //_btnTutorial.onClick.AddListener(() => TutorialManager.Instance.StartAugmentationTutorial(type));
        UpdateView();
    }

    private void UpdateView()
    {
        _image.sprite = RootView_v2.Instance.GetContentTypeSprite(_type);
        _txtContent.text = RootView_v2.Instance.GetContentTypeLabel(_type);
    }
}