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
        [SerializeField] private Button _buttonLock;
        [SerializeField] private Image _image;
        [SerializeField] private Image _imageLock;
        [SerializeField] private Sprite _spriteLock;
        [SerializeField] private Sprite _spriteUnlock;
        [SerializeField] private TMP_Text _textType;
        [SerializeField] private TMP_Text _textTitle;

        public bool Interactable
        {
            get => _button.interactable;
            set
            {
                _button.interactable = value;
                _buttonDelete.interactable = value;
                if (_buttonLock != null)
                {
                    _buttonLock.interactable = value;
                }
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
            _button.onClick.RemoveAllListeners();
            _buttonDelete.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
            _buttonDelete.onClick.AddListener(OnDeleteClick);

            if (_buttonLock != null)
            {
                _buttonLock.onClick.RemoveAllListeners();
                _buttonLock.onClick.AddListener(OnLockClick);
            }

            UpdateView();
        }

        private void OnDestroy()
        {
            if (_buttonLock != null)
            {
                _buttonLock.onClick.RemoveListener(OnLockClick);
            }
        }

        private void UpdateView()
        {
            _textTitle.text = _content.Type.ToString();
            _textType.text = _content.Type.ToString();
            UpdateLockVisual();
        }

        private void OnLockClick()
        {
            if (_content == null) return;
            if (_content.Location == null)
            {
                _content.Location = new Location();
            }

            _content.Location.IsLocked = !_content.Location.IsLocked;
            UpdateLockVisual();
            RootObject.Instance.LEE.ContentManager.UpdateContent(_content);
        }

        private void UpdateLockVisual()
        {
            if (_imageLock != null)
            {
                bool isLocked = _content?.Location?.IsLocked ?? false;
                _imageLock.sprite = isLocked ? _spriteLock : _spriteUnlock;
            }
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
