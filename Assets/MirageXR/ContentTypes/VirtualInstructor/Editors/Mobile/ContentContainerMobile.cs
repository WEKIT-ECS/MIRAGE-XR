using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
    /// <summary>
    /// UI container for listing and selecting AI models (LLM, TTS, STT) in the mobile setup workflow.
    /// Dynamically populates a list of available models and handles user interactions 
    /// for model selection and audio playback.
    /// Selected models are passed back to the SpeechSettingsMobile component.
    /// </summary>
    public class ContentContainerMobile : MonoBehaviour
    {
        [SerializeField] private ContentTypeEndpoint selectedType;
        [SerializeField] private GameObject prefabTemplate;
        [SerializeField] private RectTransform container;
        [SerializeField] private GameObject audioPlayer;
        [SerializeField] private SpeechSettingsMobile speechSettings;

        private List<AIModel> _availableModels;
        private readonly List<GameObject> _instantiatedPrefabs = new();

        private void Start()
        {
            _availableModels = selectedType switch
            {
                ContentTypeEndpoint.LLM => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels(),
                ContentTypeEndpoint.TTS => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels(),
                ContentTypeEndpoint.STT => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels(),
                _ => new List<AIModel>()
            };

            if (_availableModels.Count == 0)
            {
                Debug.LogError("[ContentContainerMobile] No models found for selected type.");
                return;
            }

            foreach (var model in _availableModels)
                CreateModelEntry(model);
        }

        private void CreateModelEntry(AIModel model)
        {
            var go = Instantiate(prefabTemplate, container);
            _instantiatedPrefabs.Add(go);

            var texts = go.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = model.Name;
            if (texts.Length > 1) texts[1].text = model.Description;

            var toggle = go.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                var group = container.GetComponentInChildren<ToggleGroup>();
                if (group != null) toggle.group = group;
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnModelSelected(model);
                });
            }

            var button = go.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    audioPlayer.SetActive(true);
                    var player = audioPlayer.GetComponent<AudioStreamPlayer>() ?? audioPlayer.AddComponent<AudioStreamPlayer>();
                    player.Setup(model);
                });
            }
        }

        private void OnModelSelected(AIModel model)
        {
            switch (selectedType)
            {
                case ContentTypeEndpoint.LLM:
                    speechSettings.SetLLM(model);
                    break;
                case ContentTypeEndpoint.TTS:
                    speechSettings.SetTTS(model);
                    break;
                case ContentTypeEndpoint.STT:
                    speechSettings.SetSTT(model);
                    break;
            }
        }

        public enum ContentTypeEndpoint
        {
            TTS,
            STT,
            LLM
        }
    }
}
