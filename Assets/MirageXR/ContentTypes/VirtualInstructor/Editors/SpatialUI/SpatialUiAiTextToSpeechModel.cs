using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Represents a container for scene objects.
    /// </summary>
    public class SpatialUiAiTextToSpeechModel : PopupBase
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
        [SerializeField] private TMP_InputField inputField;

        /// <summary>
        /// Represents a data set of AI models.
        /// </summary>
        private List<AIModel> _objectDataSet;

        private AIModel _aiModel;
        private string _voiceInstruction = string.Empty;
        private Action<AIModel, string> _callback;
        private bool _initialized = false;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _objectDataSet = RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels();
            InstantiateObjectData(_objectDataSet);
            close.onClick.AddListener(Close);
            inputField.onValueChanged.AddListener(value => _voiceInstruction = value);
            _initialized = true;
        }

        private void InstantiateObjectData(List<AIModel> objectDataSet)
        {
            foreach (var objectData in objectDataSet)
            {
                InstantiateObjectDataVoiceEndpoint(objectData);
            }
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

                toggle.SetIsOnWithoutNotify(_aiModel != null && _aiModel.Name == objectData.Name);
                toggle.onValueChanged.AddListener(value => ToggleOnValueChanged(value, objectData));
            }
            else
            {
                Debug.LogError("Prefab does not contain a Toggle component.");
            }

            var buttons= instantiatedObject.GetComponentsInChildren<Button>();
            var audioSource = instantiatedObject.GetComponentInChildren<AudioSource>();
            if (buttons is { Length: 2} && audioSource != null)
            {
                var btnStop = buttons[0];
                var btnPlay = buttons[1];
                btnPlay.onClick.AddListener(() => Play(objectData, _voiceInstruction, audioSource, btnPlay, btnStop));
                btnStop.onClick.AddListener(() => Stop(audioSource, btnPlay, btnStop));
            }
            else
            {
                Debug.LogError($"Buttons or AudioSources are missing or insufficient. Buttons are {buttons.Length} Audio Sources is null: {audioSource == null}");
            }
        }

        private void ToggleOnValueChanged(bool value, AIModel objectData)
        {
            if (value && _initialized)
            {
                OnPrefabClicked(objectData);
            }
        }

        private void OnPrefabClicked(AIModel objectData)
        {
            _callback.Invoke(objectData, _voiceInstruction);
            Close();
        }
 
        private static void Play(AIModel objectData, string voiceInstruction, AudioSource audioSource, Button btnPlay, Button btnStop)
        {
            PlayAsync(objectData, voiceInstruction, audioSource, btnPlay, btnStop).Forget();
        }

        private static async UniTask PlayAsync(AIModel objectData, string voiceInstruction, AudioSource audioSource, Button btnPlay, Button btnStop)
        {
            var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync($"Hi I am {objectData.Name}", objectData.ApiName, voiceInstruction);

            audioSource.clip = clip;
            audioSource.Play();

            btnPlay.gameObject.SetActive(false);
            btnStop.gameObject.SetActive(true);
            
            await UniTask.WaitUntil(() => !audioSource.isPlaying);
            
            btnPlay.gameObject.SetActive(true);
            btnStop.gameObject.SetActive(false);
        }

        private void Stop(AudioSource audioSource, Button btnPlay, Button btnStop)
        {
            audioSource.Stop();
            btnStop.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args is not { Length: 2 or 3 })
            {
                return false;
            }

            if (args[0] is not AIModel aiModel || args[1] is not Action<AIModel, string> callback)
            {
                return false;
            }

            _voiceInstruction = args[2] as string;
            _aiModel = aiModel;
            _callback = callback;
            return true;
        }
    }
}