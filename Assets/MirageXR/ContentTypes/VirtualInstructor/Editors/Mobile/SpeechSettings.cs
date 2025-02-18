using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using LearningExperienceEngine.DataModel;
using UnityEngine.Serialization;

namespace MirageXR
{
    /// <summary>
    /// Speech settings class for managing AI prompt, voice, model and language settings.
    /// </summary>
    public class SpeechSettings : MonoBehaviour
    {
        /// <summary>
        /// Represents the AI prompt in the Speech Settings.
        /// </summary>
        [SerializeField]
        private GameObject aiPrompt;
        [SerializeField]
        private GameObject voice;

        /// <summary>
        /// The GameObject representing the model.
        /// </summary>
        [SerializeField]
        private GameObject model;

        /// <summary>
        /// Reference to the GameObject representing the language.
        /// </summary>
        [SerializeField]
        private GameObject language;

        /// <summary>
        /// The AI prompt data variable represents the text entered by the user.
        /// </summary>
        private string _aiPromptData = "Enter text";

        /// <summary>
        /// Represents voice data used for AI processing.
        /// </summary>
        private AIModel _voiceData;

        /// <summary>
        /// Represents the data of an AI model.
        /// </summary>
        private AIModel _modelData;

        /// <summary>
        /// Represents the language data for an AI model.
        /// </summary>
        private AIModel _languageData;

        /// <summary>
        /// Starts the SpeechSettings by setting default values.
        /// </summary>

        [FormerlySerializedAs("PromtView")] [SerializeField] 
        private GameObject PromptView; 

        [SerializeField] 
        private GameObject LLMView;

        [SerializeField]
        private GameObject STTView;

        [SerializeField]
        private GameObject TTSView; 
        
        [FormerlySerializedAs("PromtViewBackBtn")] [SerializeField]
        private Button  PromptViewBackBtn; 
        
        [SerializeField] 
        private Button LLMViewBackBtn;
        
        [SerializeField] 
        private Button STTViewBackBtn;
        
        [SerializeField] 
        private Button TTSViewBackBtn; 
        
        [SerializeField]
        private Button AiPromptBtn;
        
        [SerializeField] 
        private Button ModelBtn;
        
        [SerializeField] 
        private Button LanguageBtn;
        
        [SerializeField]
        private Button VoiceBtn;

        
        public void Start()
        {
            try
            {
                if (_modelData == null)
                {
                    UpdateModel(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels()[0]);
                }

                if (_languageData == null)
                {
                    UpdateLanguage(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels()[0]);
                }

                if (_voiceData == null)
                {
                    UpdateVoice(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels()[0]);
                }
            
                if (_modelData != null)
                {
                    UpdateModel(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels()[0]);
                }

                if (_languageData != null)
                {
                    UpdateLanguage(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels()[0]);
                }

                if (_voiceData != null)
                {
                    UpdateVoice(RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels().Last());
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Fail to load the models. Is the server online?");
            }
         

            AiPromptBtn.onClick.AddListener(() =>OpenView(PromptView, true));
            ModelBtn.onClick.AddListener(() =>OpenView(LLMView, true));
            LanguageBtn.onClick.AddListener(() => OpenView(STTView, true));
            VoiceBtn.onClick.AddListener(() => OpenView(TTSView, true));
            
            PromptViewBackBtn.onClick.AddListener(() => OpenView(PromptView, false));
            LLMViewBackBtn.onClick.AddListener(() => OpenView(LLMView, false));
            STTViewBackBtn.onClick.AddListener(() => OpenView(STTView, false));
            TTSViewBackBtn.onClick.AddListener(() => OpenView(TTSView, false));
            
        }

        private void OpenView(GameObject view, Boolean state)
        {
            
            view.SetActive(state);
        }


        /// <summary>
        /// Updates the AI prompt with the provided new string.
        /// </summary>
        /// <param name="newString">The new string to update the AI prompt with.</param>
        public void UpdateAIPrompt(string newString)
        {
            _aiPromptData = newString;
            UpdateText(aiPrompt, null, _aiPromptData);
        }

        /// <summary>
        /// Updates the voice for the AI.
        /// </summary>
        /// <param name="obj">The AI model containing the voice data.</param>
        public void UpdateVoice(AIModel obj)
        {
            _voiceData = obj;
            UpdateText(voice, _voiceData);
        }

        /// <summary>
        /// Updates the model for the AI service.
        /// </summary>
        /// <param name="obj">The new AI model to update to.</param>
        public void UpdateModel(AIModel obj)
        {
            _modelData = obj;
            UpdateText(model, _modelData);
        }

        /// <summary>
        /// Updates the language in the speech settings.
        /// </summary>
        /// <param name="obj">The AIModel object representing the new language.</param>
        public void UpdateLanguage(AIModel obj)
        {
            _languageData = obj;
            UpdateText(language, _languageData);
        }

        /// <summary>
        /// Update the text of a given GameObject using either an AIModel object or a string prompt.
        /// </summary>
        /// <param name="prefab">The GameObject to update the text on.</param>
        /// <param name="newObj">An AIModel object containing the new text to be displayed. If null, the prompt parameter will be used.</param>
        /// <param name="prompt">A string prompt to be used as the new text. Only used if newObj is null.</param>
        private void UpdateText(GameObject prefab, AIModel newObj = null, string prompt = null)
        {
            string text = string.IsNullOrEmpty(prompt) ? newObj?.Name ?? string.Empty : prompt;
            SetTextMeshProComponents(prefab, text.Substring(0, Math.Min(10, text.Length)));
        }

        /// <summary>
        /// Sets the text of the TMP_Text components in the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab containing the TMP_Text components.</param>
        /// <param name="text">The new text to set.</param>
        private void SetTextMeshProComponents(GameObject prefab, string text)
        {
            var texts = prefab.GetComponentsInChildren<TMP_Text>();
            texts[1].text = text;
        }


        /// <summary>
        /// Retrieves the AI prompt.
        /// </summary>
        /// <returns>The AI prompt data as a string.</returns>
        public String GetAIPrompt()
        {
            return _aiPromptData;
        }

        /// <summary>
        /// Retrieves the AIModel object representing the voice data.
        /// </summary>
        /// <returns>The AIModel object representing the voice data.</returns>
        public AIModel GetVoice()
        {
            return _modelData;
        }

        /// <summary>
        /// Retrieves the AIModel object. </summary> <returns>
        /// The AIModel object. </returns>
        /// /
        public AIModel GetModel()
        {
            return _voiceData;
        }

        /// <summary>
        /// Retrieves the language model.
        /// </summary>
        /// <returns>The language model.</returns>
        public AIModel GetLanguage()
        {
            return _languageData;
        }
    }
}