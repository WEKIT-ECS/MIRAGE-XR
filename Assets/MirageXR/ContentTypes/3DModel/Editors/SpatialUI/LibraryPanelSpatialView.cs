using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class LibraryPanelSpatialView : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _searchField;
        [Header("Containers")]
        [SerializeField] private RectTransform _libraryModelsContainer;
        
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            
            _buttonBack.onClick.AddListener(OnClickBackButton);
            _searchField.onValueChanged.AddListener(OnInputFieldSearchChanged);
        }

        private void OnClickBackButton()
        {
            Close();
        }
        
        private void OnInputFieldSearchChanged(string text)
        {
            // TODO
        }
    }
}
