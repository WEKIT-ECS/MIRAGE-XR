using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Abstract base class for instructor setup menus.
    ///
    /// This class provides shared logic for handling instructor configuration such as
    /// prompt text and AI model selections (TTS, STT, LLM).
    /// 
    /// It uses a singleton (InstructorMenuModel) to store the state centrally, ensuring
    /// that all views (mobile, desktop, etc.) operate on the same data instance.
    /// 
    /// Platform-specific subclasses should inherit from this and implement 
    /// UpdateUiFromModel() to update their respective user interface elements 
    /// based on the shared model data.
    /// 
    /// Key Features:
    /// - Initializes default prompt and AI models.
    /// - Provides unified getters and setters for menu data.
    /// - Enforces consistent state management via singleton.
    /// - Separates platform logic for clean scalability.
    /// </summary>

    public abstract class AbstractVirtualInstructorMenu : PopupEditorBase
    {
        [Header("Defaults (optional)")]
        [SerializeField] protected string defaultPrompt = "Enter your prompt here";
        [SerializeField] protected string defaultTtsModel;
        [SerializeField] protected string defaultSttModel;
        [SerializeField] protected string defaultLlmModel;
        
        private readonly InstructorMenuModel _model = InstructorMenuModel.Instance;
        
        private bool _isSetup;

        /// <summary>
        /// Initializes prompt and model values using config strings or fallback to first available.
        /// Logs warnings if configured model names are not found.
        /// </summary>
        protected virtual void InitializeDefaults()
        {
            if (_isSetup)
            {
                return;
            }
            UnityEngine.Debug.LogWarning("InitializeDefaults");
            var ai = RootObject.Instance.LEE.ArtificialIntelligenceManager;
            var tts = SelectModel(ai.GetTtsModels(), defaultTtsModel, "TTS");
            var stt = SelectModel(ai.GetSttModels(), defaultSttModel, "STT");
            var llm = SelectModel(ai.GetLlmModels(), defaultLlmModel, "LLM");
            var prompt = defaultPrompt;

            _model.Reset(tts, stt, llm, prompt);
            UpdateUiFromModel();
            _isSetup = true;
        }

        /// <summary>
        /// Selects the model matching the given name or falls back to the first available.
        /// Logs detailed errors if the model list is null or empty.
        /// </summary>
        protected AIModel SelectModel(List<AIModel> models, string configName, string type)
        {
            if (models == null || models.Count == 0)
            {
                Debug.LogError($"[InstructorMenu] No {type} models available. Cannot select configured model: '{configName}'");
                return null;
            }

            if (string.IsNullOrEmpty(configName))
            {
                return models[0];
            }

            var matchByApiName = models.FirstOrDefault(m => string.Equals(m.ApiName, configName, StringComparison.OrdinalIgnoreCase));

            if (matchByApiName != null)
            {
                Debug.Log($"[InstructorMenu] Selected {type} model by ApiName: '{matchByApiName.ApiName}'");
                return matchByApiName;
            }

            Debug.LogWarning($"[InstructorMenu] Configured {type} model '{configName}' not found. Using first available model: '{models[0].ApiName}'");
            return models[0];
        }

        public virtual void SetPrompt(string prompt)
        {
            _model.Prompt = string.IsNullOrEmpty(prompt) ? defaultPrompt : prompt;
            //Debug.Log($"Prompt in the Singleton: {_model.Prompt}");
            UpdateUiFromModel(); 
        }

        public virtual void SetTTS(AIModel model)
        {
            _model.TTS = model;
            Debug.Log($"TTS in Singleton: {_model.TTS?.Name ?? "null"}");
            UpdateUiFromModel(); 
        }

        public virtual void SetSTT(AIModel model)
        {
            _model.STT = model;
            //Debug.Log($"STT in Singleton: {_model.STT?.Name ?? "null"}");
            UpdateUiFromModel(); 
        }

        public virtual void SetLLM(AIModel model)
        {
            _model.LLM = model;
            //Debug.Log($"LLM in Singleton: {_model.LLM?.Name ?? "null"}");
            UpdateUiFromModel(); 
        }

        public AIModel GetTTS() => _model.TTS;
        public AIModel GetSTT() => _model.STT;
        public AIModel GetLLM() => _model.LLM;
        public string GetPrompt() => _model.Prompt;

        /// <summary>
        /// Updates the UI elements to reflect the current state of the model.
        /// 
        /// Concrete subclasses should override this method if they need to update
        /// their specific UI when the model changes.
        /// 
        /// This method is automatically called whenever a model field is updated.
        /// </summary>
        protected abstract void UpdateUiFromModel();
    }
}
