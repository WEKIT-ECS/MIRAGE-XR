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
    public class ContentContainer : MonoBehaviour
    {
        /// <summary>
        /// Enumeration representing the different functions available for ObjectData in the ContentContainer class.
        /// </summary>
        public enum ObjectDataFunctionEnum
        {
            /// <summary>
            /// Represents the voice endpoint for the AIManager's TTS (Text-to-Speech) models.
            /// </summary>
            VoiceEndpoint,

            /// <summary>
            /// Represents an AI model endpoint for the ObjectDataFunctionEnum.
            /// </summary>
            ModelEndpoint,

            /// <summary>
            /// Represents the language endpoint in the ObjectDataFunctionEnum.
            /// </summary>
            LanguageEndpoint,
        }

        /// <summary>
        /// Represents an enumeration of selected functions for the ContentContainer class.
        /// </summary>
        [SerializeField]
        private ObjectDataFunctionEnum selectedFunctionEnum;

        /// <summary>
        /// The prefab template to instantiate for each object data.
        /// </summary>
        [SerializeField]
        private GameObject prefabTemplate;

        /// <summary>
        /// Represents a container for scene objects.
        /// </summary>
        [SerializeField]
        private RectTransform sceneContainer;

        /// <summary>
        /// Represents an audio player.
        /// </summary>
        [SerializeField]
        private GameObject audioPlayer;

        /// <summary>
        /// Speech settings class for managing AI prompt, voice, model, and language settings.
        /// </summary>
        [SerializeField]
        private SpeechSettings speechSettings;

        /// <summary>
        /// Represents a view for the Virtual Instructor in a popup editor.
        /// </summary>
        [SerializeField]
        private VirtualInstructorView VirtualInstructorView;

        /// <summary>
        /// Represents a data set of AI models.
        /// </summary>
        private List<AIModel> _objectDataSet;

        /// <summary>
        /// This variable stores a list of all child GameObjects in the ContentContainer.
        /// </summary>
        private readonly List<GameObject> _allChildGameObjects = new();

        /// <summary>
        /// The Start method is called when the ContentContainer MonoBehaviour is started.
        /// </summary>
        void Start()
        {
            var actions = new Dictionary<ObjectDataFunctionEnum, Func<List<AIModel>>>
            {
                { ObjectDataFunctionEnum.ModelEndpoint, () => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels() },
                { ObjectDataFunctionEnum.LanguageEndpoint, () => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels() },
                { ObjectDataFunctionEnum.VoiceEndpoint, () => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels() },
            };
            _objectDataSet = actions[selectedFunctionEnum]();
            InstantiateObjectData();
        }

        /// <summary>
        /// Instantiates object data.
        /// </summary>
        private void InstantiateObjectData()
        {
            if (_objectDataSet.Count != 0)
            {
                foreach (AIModel objectData in _objectDataSet)
                {
                    InstantiateObjectData(objectData);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Fail to load the models. Is the server online?");
            }
        }

        /// <summary>
        /// Instantiates an object and sets the text components and toggle listeners based on the provided AIModel data.
        /// </summary>
        private void InstantiateObjectData(AIModel objectData)
        {
            GameObject instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
            TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();

            if (textComponents.Length == 2)
            {
                textComponents[0].text = objectData.Name;
                textComponents[1].text = objectData.Description;
            }
            else if (textComponents.Length == 1)
            {
                textComponents[0].text = objectData.Name;
            }
            else
            {
                Debug.LogError("Wrong Prefab in ContentContainer");
            }

            Toggle toggle = instantiatedObject.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                ToggleGroup toggleGroup = sceneContainer.GetComponentInChildren<ToggleGroup>();
                if (toggleGroup != null)
                {
                    toggle.group = toggleGroup;
                }
                else
                {
                    Debug.LogError("ToggleGroup component is missing in sceneContainer.");
                }

                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnPrefabClicked(objectData);
                    }
                });
            }
            else
            {
                Debug.LogError("Prefab does not contain a Toggle component.");
            }

            Button button = instantiatedObject.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    audioPlayer.SetActive(true);
                    
                    AudioStreamPlayer audioStreamPlayerComponent = audioPlayer.GetComponent<AudioStreamPlayer>();
                    if (audioStreamPlayerComponent == null)
                    {
                        audioStreamPlayerComponent = audioPlayer.AddComponent<AudioStreamPlayer>();
                    }
                    audioStreamPlayerComponent.Setup(objectData);
                });
            }
            _allChildGameObjects.Add(instantiatedObject);
        }

        /// <summary>
        /// Handles the click event of a prefab object.
        /// Updates the speech settings based on the endpoint name of the clicked object.
        /// </summary>
        /// <param name="objectData">The AIModel object associated with the clicked prefab.</param>
        private void OnPrefabClicked(AIModel objectData)
        {
            switch (objectData.EndpointName)
            {
                case "stt/":
                    speechSettings.UpdateLanguage(objectData);
                    VirtualInstructorView.SetSTT(objectData);
                    break;
                case "tts/":
                    speechSettings.UpdateVoice(objectData);
                    VirtualInstructorView.SetTTS(objectData);
                    break;
                case "llm/":
                    speechSettings.UpdateModel(objectData);
                    VirtualInstructorView.SetLLM(objectData);
                    break;
                default:
                    Debug.LogError("Did not found the endpoint. Bad configuration from Server!");
                    break;
            }
        }
    }
}