using System;
using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Represents a container for scene objects.
    /// </summary>
    public class SpatialAILanguageModel : PopupBase
    {
        /// <summary>
        /// The prefab template to instantiate for each object data.
        /// </summary>
        [SerializeField] private GameObject prefabTemplate;

        /// <summary>
        /// Represents a container for scene objects.
        /// </summary>
        [SerializeField] private RectTransform sceneContainer;

        [SerializeField] private Button close;

        /// <summary>
        /// Represents a data set of AI models.
        /// </summary>
        private List<AIModel> _objectDataSet;

        private Action<AIModel> _callback;
        private AIModel _aiModel;
        private bool _initialized = false;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _objectDataSet = RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels();
            InstantiateObjectData(_objectDataSet);
            close.onClick.AddListener(Close);
            _initialized = true;
        }

        /// <summary>
        /// Instantiates object data.
        /// </summary>
        private void InstantiateObjectData(List<AIModel> objectDataSet)
        {
            foreach (var objectData in objectDataSet)
            {
                InstantiateObjectDataModelEndpoint(objectData);
            }
        }

        private void InstantiateObjectDataModelEndpoint(AIModel objectData)
        { 
            var instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
            var textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();
            if (textComponents.Length == 2) 
            {
                textComponents[0].text = objectData.Name;
                textComponents[1].text = objectData.Description;
            }
            else
            {
                Debug.LogError("Wrong Prefab in ContentContainer");
            }

            var toggle = instantiatedObject.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                var toggleGroup = sceneContainer.GetComponentInChildren<ToggleGroup>();
                toggleGroup.allowSwitchOff = true;
                if (toggleGroup != null)
                {
                    toggle.group = toggleGroup;
                }
                else
                {
                    Debug.LogError("ToggleGroup component is missing in sceneContainer.");
                }

                toggle.SetIsOnWithoutNotify(_aiModel.Name == objectData.Name);
                toggle.onValueChanged.AddListener(value => ToggleOnValueChanged(value, objectData));
            }
            else
            {
                Debug.LogError("Prefab does not contain a Toggle component.");
            }
        }

        private void ToggleOnValueChanged(bool value, AIModel objectData)
        {
            if (value && _initialized)
            {
                OnPrefabClicked(objectData);
            }
        }

        /// <summary>
        /// Handles the click event of a prefab object.
        /// Updates the speech settings based on the endpoint name of the clicked object.
        /// </summary>
        /// <param name="objectData">The AIModel object associated with the clicked prefab.</param>
        private void OnPrefabClicked(AIModel objectData)
        {
            _callback.Invoke(objectData);
            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args is not { Length: 2 })
            {
                return false;
            }

            if (args[0] is not AIModel aiModel || args[1] is not Action<AIModel> callback)
            {
                return false;
            }

            _aiModel = aiModel;
            _callback = callback;
            return true;
        }
    }
}