using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelEditorSpatialView : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonSettings;
        [Header("Game Objects")]
        [SerializeField] private GameObject _itemAddNewModel;
        [SerializeField] private GameObject _itemHint;
        [Header("Containers")]
        [SerializeField] private RectTransform _localModelsContainer;
        [SerializeField] private RectTransform _scetchfabContainer;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _headerInputField;
        [SerializeField] private TMP_InputField _searchField;
        
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            
            _buttonBack.onClick.AddListener(OnClickBackButton);
            _buttonSettings.onClick.AddListener(OnClickSettingsButton);
            _headerInputField.onValueChanged.AddListener(OnHeaderInputFieldChanged);
            _searchField.onValueChanged.AddListener(OnInputFieldSearchChanged);
        }

        private void OnClickSettingsButton()
        {
            Close();
        }

        private void OnClickBackButton()
        {
            // TODO
        }
        
        private void OnInputFieldSearchChanged(string text)
        {
            // TODO
        }
        
        private void OnHeaderInputFieldChanged(string text)
        {
            // TODO
        }
    }
}
