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
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _textType;
        [SerializeField] private TMP_Text _textTitle;

        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private Content _content;
        private UnityAction<Content> _onClickAction;

        public void Initialize(Content content, UnityAction<Content> onClick)
        {
            _content = content;
            _onClickAction = onClick;
            _button.onClick.AddListener(OnClick);
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
    }
}
