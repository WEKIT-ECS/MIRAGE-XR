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
    public class SpatialUiAiModelContainer : MonoBehaviour
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
        
        [SerializeField] private Button close;




        /// <summary>
        /// Represents a data set of AI models.
        /// </summary>
        private List<AIModel> _objectDataSet;

        /// <summary>
        /// This variable stores a list of all child GameObjects in the ContentContainer.
        /// </summary>
        private readonly List<GameObject> _allChildGameObjects = new();

        [SerializeField] private AddEditVirtualInstructor addEditVirtualInstructor;

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
            close.onClick.AddListener(() => { gameObject.SetActive(false); });
            UnityEngine.Debug.Log("Length of data is: " + _objectDataSet.Count);
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
            switch (selectedFunctionEnum)
            {
                case ObjectDataFunctionEnum.VoiceEndpoint:
                    InstantiateObjectDataVoiceEndpoint(objectData);
                    break;
                case ObjectDataFunctionEnum.ModelEndpoint:
                    InstantiateObjectDataModelEndpoint(objectData);
                    break;
                case ObjectDataFunctionEnum.LanguageEndpoint:
                    InstantiateObjectDataLanguageEndpoint(objectData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InstantiateObjectDataLanguageEndpoint(AIModel objectData)
        { 
            var instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
            TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();
            if (textComponents.Length == 1) 
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
            _allChildGameObjects.Add(instantiatedObject);
        }

        private void InstantiateObjectDataModelEndpoint(AIModel objectData)
        { 
            var instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
            TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();
            if (textComponents.Length == 2) 
            {
                textComponents[0].text = objectData.Name;
                textComponents[1].text = objectData.Description;
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
            _allChildGameObjects.Add(instantiatedObject);
        
        }

        private void InstantiateObjectDataVoiceEndpoint(AIModel objectData)
        {
            UnityEngine.Debug.Log("InstantiateObjectDataVoiceEndpoint");
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

            var buttons= instantiatedObject.GetComponentsInChildren<Button>();
            Debug.Log("buttons =" + buttons.Length);
            var audioSources = instantiatedObject.GetComponentsInChildren<AudioSource>();
            Debug.Log("audioSources =" + audioSources.Length);
            if (buttons is { Length: 2} && audioSources is { Length: 1})
            {
                async void Play()
                {
                  
                    var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync("Hi I am " + objectData.Name, objectData.ApiName);

                    audioSources[0].clip = clip;
                    audioSources[0].Play();

                    buttons[1].gameObject.SetActive(false);
                    buttons[0].gameObject.SetActive(true);
                }

                void Stop()
                {
                    
                    audioSources[0].Stop();
                    buttons[0].gameObject.SetActive(false);
                    buttons[1].gameObject.SetActive(true);
                }

               

                buttons[1].onClick.AddListener(Play);
                buttons[0].onClick.AddListener(Stop);
            }
            else
            {
                Debug.LogError("Buttons or AudioSources are missing or insufficient. Buttons are " + buttons.Length +"Audio Sources "+ audioSources.Length);
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
                    
                    addEditVirtualInstructor.SetAIModel(objectData, "SpeechToTextModel");
                    break;
                case "tts/":
                    addEditVirtualInstructor.SetAIModel(objectData, "TextToSpeechModel");
                    break;
                case "llm/":
                    addEditVirtualInstructor.SetAIModel(objectData, "LanguageModel");
                    break;
                default:
                    Debug.LogError("Did not found the endpoint. Bad configuration from Server!");
                    break;
            }
            this.gameObject.SetActive(false);
        }
    }
}