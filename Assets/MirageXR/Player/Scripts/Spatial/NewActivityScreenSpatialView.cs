using System;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class NewActivityScreenSpatialView : ScreenView
    {
        [Serializable]
        public class EditorListItem
        {
            public ContentType ContentType;
            public EditorSpatialView EditorView;
        }

        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonCollaborativeSession;
        [SerializeField] private Button _buttonAddNewStep;
        [Header("Containers")]
        [SerializeField] private Transform _stepsContainer;
        [Header("Prefabs")]
        [SerializeField] private StepItemView _stepsItemPrefab;
        [SerializeField] private EditorListItem[] _editorPrefabs;

        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonSettingsClick(UnityAction action) => _buttonSettings.SafeSetListener(action);
        public void SetActionOnButtonCollaborativeSessionClick(UnityAction action) => _buttonCollaborativeSession.SafeSetListener(action);
        public void SetActionOnButtonAddNewStepClick(UnityAction action) => _buttonAddNewStep.SafeSetListener(action);

        public Transform GetStepsContainer()
        {
            return _stepsContainer;
        }

        public StepItemView GetStepsItemPrefab()
        {
            return _stepsItemPrefab;
        }

        public EditorSpatialView GetEditorPrefab(ContentType contentType)
        {
            foreach (var editorListItem in _editorPrefabs)
            {
                if (editorListItem.ContentType == contentType)
                {
                    return editorListItem.EditorView;
                }
            }

            return null;
        }
    }
}
