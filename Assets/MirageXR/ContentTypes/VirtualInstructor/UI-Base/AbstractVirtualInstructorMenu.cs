using System;
using System.Collections.Generic;
using System.Linq;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Abstract base class for instructor setup menus.
    /// Handles shared logic such as model assignment, prompt defaults, and update hooks.
    /// Concrete platform-specific classes (e.g. Mobile, Vision) should inherit from this.
    /// </summary>
    public abstract class AbstractVirtualInstructorMenu : PopupEditorBase
    {
        [Header("Defaults (optional)")]
        [SerializeField] protected string defaultPrompt = "Enter your prompt here";
        [SerializeField] protected string defaultTtsModel;
        [SerializeField] protected string defaultSttModel;
        [SerializeField] protected string defaultLlmModel;

        protected AIModel _tts;
        protected AIModel _stt;
        protected AIModel _llm;
        protected string _aiPrompt;

        /// <summary>
        /// Initializes prompt and model values using config strings or fallback to first available.
        /// Logs warnings if configured model names are not found.
        /// </summary>
        protected virtual void InitializeDefaults()
        {
            var ai = RootObject.Instance.LEE.ArtificialIntelligenceManager;

            _tts = SelectModel(ai.GetTtsModels(), defaultTtsModel, "TTS");
            _stt = SelectModel(ai.GetSttModels(), defaultSttModel, "STT");
            _llm = SelectModel(ai.GetLlmModels(), defaultLlmModel, "LLM");
            _aiPrompt = defaultPrompt;

            OnPromptUpdated(_aiPrompt);
            OnTTSUpdated(_tts);
            OnSTTUpdated(_stt);
            OnLLMUpdated(_llm);
            
            Debug.LogWarning($"[Instructor Init] defaultTtsModel = '{defaultTtsModel}'");

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

            if (string.IsNullOrEmpty(configName)) return models[0];

            var matchByApiName = models.FirstOrDefault(m =>
                string.Equals(m.ApiName, configName, StringComparison.OrdinalIgnoreCase));

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
            _aiPrompt = string.IsNullOrEmpty(prompt) ? defaultPrompt : prompt;
            OnPromptUpdated(_aiPrompt);
        }

        public virtual void SetTTS(AIModel model)
        {
            _tts = model;
            
            OnTTSUpdated(model);
        }

        public virtual void SetSTT(AIModel model)
        {
            _stt = model;
            OnSTTUpdated(model);
        }

        public virtual void SetLLM(AIModel model)
        {
            _llm = model;
            OnLLMUpdated(model);
        }

        protected virtual void OnPromptUpdated(string prompt) { }
        protected virtual void OnTTSUpdated(AIModel model) { }
        protected virtual void OnSTTUpdated(AIModel model) { }
        protected virtual void OnLLMUpdated(AIModel model) { }

        public AIModel GetTTS() => _tts;
        public AIModel GetSTT() => _stt;
        public AIModel GetLLM() => _llm;
        public string GetPrompt() => _aiPrompt;
    }
}
