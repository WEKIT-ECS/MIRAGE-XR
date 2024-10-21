using System;
using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SelectAugmentationScreenSpatialView : PopupBase
    {
        [SerializeField] private Button _buttonBack;
        [SerializeField] private ContentSelectorSpatialListItem _contentSelectorListItemPrefab;
        [SerializeField] private Transform container;

        private List<ContentType> _types;

        protected override bool TryToGetArguments(params object[] args)
        {
            try
            {
                _types = (List<ContentType>)args[0];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _buttonBack.onClick.AddListener(Close);
            UpdateView();
        }

        private void UpdateView()
        {
            foreach (Transform item in container)
            {
                Destroy(item.gameObject);
            }

            foreach (var type in _types)
            {
                var item = Instantiate(_contentSelectorListItemPrefab, container);
                item.Init(type, OnItemSelected, OnItemHintClick);
            }
        }

        private void OnItemHintClick(ContentType contentType)
        {
        }

        private void OnItemSelected(ContentType contentType)
        {
            Close();
            var prefab = MenuManager.Instance.GetEditorPrefab(contentType);
            if (prefab != null)
            {
                PopupsViewer.Instance.Show(prefab, prefab);   
            }
        }
    }
}
