using System;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ContetItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _buttonDelete;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _textType;
        [SerializeField] private TMP_Text _textTitle;

        public bool Interactable
        {
            get => _button.interactable;
            set
            {
                _button.interactable = value;
                _buttonDelete.interactable = value;
            }
        }

        public Guid ContentID => _content.Id;

        private Content _content;
        private UnityAction<Content> _onClickAction;
        private UnityAction<Content> _onDeleteClick;

        public void Initialize(Content content, UnityAction<Content> onClick, UnityAction<Content> onDeleteClick)
        {
            _content = content;
            _onClickAction = onClick;
            _onDeleteClick = onDeleteClick;
            _button.onClick.AddListener(OnClick);
            _buttonDelete.onClick.AddListener(OnDeleteClick);
            UpdateView();
        }

        private void UpdateView()
        {
            _textTitle.text = _content.Type.ToString();
            _textType.text = _content.Type.ToString();
        }
        
        private void OnClick()
        {
            _onClickAction?.Invoke(_content);
        }
        
        private void OnDeleteClick()
        {
            _onDeleteClick?.Invoke(_content);
        }
    }
}
