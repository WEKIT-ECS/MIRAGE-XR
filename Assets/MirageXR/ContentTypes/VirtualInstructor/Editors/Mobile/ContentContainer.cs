using System;
using System.Collections.Generic;
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
            /// Represents a demo data object with a name.
            /// </summary>
            DemoDataWithName,

            /// <summary>
            /// Represents a demo data member in the ObjectDataFunctionEnum enum with a name and description.
            /// </summary>
            DemoDataWithNameAndDescription,

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
        public ObjectDataFunctionEnum selectedFunctionEnum;

        /// <summary>
        /// The prefab template to instantiate for each object data.
        /// </summary>
        public GameObject prefabTemplate;

        /// <summary>
        /// Represents a container for scene objects.
        /// </summary>
        public Transform sceneContainer;

        /// <summary>
        /// Represents an audio player.
        /// </summary>
        public GameObject audioPlayer;

        /// <summary>
        /// Speech settings class for managing AI prompt, voice, model, and language settings.
        /// </summary>
        public SpeechSettings speechSettings;

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
                { ObjectDataFunctionEnum.DemoDataWithName, DemoWithOne },
                { ObjectDataFunctionEnum.DemoDataWithNameAndDescription, DemoWithTow },
                { ObjectDataFunctionEnum.ModelEndpoint, () => RootObject.Instance.aiManager.GetLlmModels() },
                { ObjectDataFunctionEnum.LanguageEndpoint, () => RootObject.Instance.aiManager.GetSttModels() },
                { ObjectDataFunctionEnum.VoiceEndpoint, () => RootObject.Instance.aiManager.GetTtsModels() },
            };

            _objectDataSet = actions[selectedFunctionEnum]();

            if (selectedFunctionEnum == ObjectDataFunctionEnum.VoiceEndpoint)
            {
                InstantiateObjectDataWithTwoButtons();
            }
            else
            {
                InstantiateObjectDataWithOneButton();
            }
        }

        /// <summary>
        /// Instantiates data objects with two buttons.
        /// </summary>
        private void InstantiateObjectDataWithTwoButtons()
        {
            foreach (AIModel objectData in _objectDataSet)
            {
                InstantiateObjectDataWithTowButton(objectData);
            }
        }

        /// <summary>
        /// Instantiates object data with a single button.
        /// </summary>
        private void InstantiateObjectDataWithOneButton()
        {
            foreach (AIModel objectData in _objectDataSet)
            {
                InstantiateObjectDataWithOneButton(objectData);
            }
        }

        /// <summary>
        /// Instantiates an object with two buttons and sets the text components and button listeners based on the provided AIModel data.
        /// </summary>
        private void InstantiateObjectDataWithTowButton(AIModel objectData)
        {
            GameObject instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
            TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();

            if (textComponents.Length == 2)
            {
                textComponents[0].text = objectData.Name;
                textComponents[1].text = objectData.Description;
            }
            else
            {
                UnityEngine.Debug.LogError("Wrong Prefab in ContentContainer");
            }

            Button[] buttons = instantiatedObject.GetComponentsInChildren<Button>();
            UnityEngine.Debug.LogWarning(buttons.Length);
            if (buttons[0] != null && buttons[1] != null)
            {
                buttons[0].onClick.AddListener(() => OpenAudioPrefab(objectData));
                buttons[1].onClick.AddListener(() => OnPrefabClicked(objectData));

                GameObject child = buttons[1].transform.GetChild(0).gameObject;
                _allChildGameObjects.Add(child);
                buttons[1].onClick.AddListener(() => ToggleChildObjects(child));
            }
            else
            {
                UnityEngine.Debug.LogError("Button-Component one is missing in Prefab");
            }
        }

        /// <summary>
        /// Instantiate an object data with a button.
        /// </summary>
        private void InstantiateObjectDataWithOneButton(AIModel objectData)
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
                UnityEngine.Debug.LogError("Wrong Prefab in ContentContainer");
            }

            Button button = instantiatedObject.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnPrefabClicked(objectData));
                GameObject child = button.transform.GetChild(0).gameObject;
                _allChildGameObjects.Add(child);
                button.onClick.AddListener(() => ToggleChildObjects(child));
            }
            else
            {
                UnityEngine.Debug.LogError("Button-Component is missing in Prefab");
            }
        }

        /// <summary>
        /// Toggles the visibility of child objects in the ContentContainer.
        /// </summary>
        private void ToggleChildObjects(GameObject activeChild)
        {
            foreach (var child in _allChildGameObjects)
            {
                child.SetActive(false); 
            }
            activeChild.SetActive(true); 
        }


        /// <summary>
        /// Opens the audio player.
        /// </summary>
        private void OpenAudioPrefab(AIModel objectData)
        {
            audioPlayer.SetActive(true);
            var tmpTextComponent = audioPlayer.GetComponentInChildren<TMP_Text>();
            if (tmpTextComponent != null)
            {
                tmpTextComponent.text = objectData.Name;
            }
            else
            {
                UnityEngine.Debug.LogError("TMP_Text component is missing in audioPlayer");
            }
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
                case "listen/":
                    speechSettings.UpdateLanguage(objectData);
                    break;
                case "speak/":
                    speechSettings.UpdateVoice(objectData);
                    break;
                case "think/":
                    speechSettings.UpdateModel(objectData);
                    break;
                default:
                    UnityEngine.Debug.LogError("Endpunkt nicht gefuden!");
                    break;
            }
            
        }


        /// <summary>
        /// Generates a demo list of AIModel objects based on a specified condition.
        /// </summary>
        private List<AIModel> DemoWithOne()
        {
            return CreateDemo(false, 5);
        }


        /// <summary>
        /// Creates a demo with two AIModel objects based on the given parameters.
        /// </summary>
        private List<AIModel> DemoWithTow()
        {
            return CreateDemo(true, 5);
        }


        /// <summary>
        /// Create a demo list of AIModel objects.
        /// </summary>
        private List<AIModel> CreateDemo(bool b, int i)
        {
            var temp = new List<AIModel>();
            if (b)
            {
                for (var j = 0; j < i; j++)
                {
                    var t = new AIModel("think/", "Lorem ipus" + j, "Description" + j);
                    temp.Add(t);
                }
            }
            else
            {
                for (var j = 0; j < i; j++)
                {
                    var t = new AIModel("think/", "Lorem ipus" + j);
                    temp.Add(t);
                }
            }

            return temp;
        }
    }
}