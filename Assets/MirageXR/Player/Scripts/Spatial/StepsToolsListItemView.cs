using System;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepsToolsListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private Button buttonDelete;

        private RequiredToolsPartsMaterials _requiredToolsPartsMaterials;
        private Guid _stepId;

        public bool Interactable
        {
            get => buttonDelete.gameObject.activeSelf;
            set => buttonDelete.gameObject.SetActive(value);
        }

        public void Initialize(RequiredToolsPartsMaterials requiredToolsPartsMaterials, Guid stepId, UnityAction<Guid, RequiredToolsPartsMaterials> onDeleteClick)
        {
            _stepId = stepId;
            _requiredToolsPartsMaterials = requiredToolsPartsMaterials;
            tmpText.text = _requiredToolsPartsMaterials.ToolPartMaterial;
            buttonDelete.onClick.AddListener(() => onDeleteClick(_stepId, _requiredToolsPartsMaterials));
        }
    }
}