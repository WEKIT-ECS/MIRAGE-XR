using System;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
    
    /// <summary>
    /// Singleton model that stores the configuration state for the instructor setup.
    /// It holds the currently selected prompt, TTS, STT, and LLM models.
    /// Provides unified access to the instructor data across all menu vi
    /// Fields are automatically validated to prevent null assignments.
    /// The model must be reset to initialize or clear the current state.
    /// </summary>
    public class InstructorMenuModel
    {
        private AIModel _tts;
        private AIModel _stt;
        private AIModel _llm;
        private string _prompt;

        
        public AIModel TTS
        {
            get => _tts;
            set => _tts = value ?? throw new ArgumentNullException(nameof(TTS));
        }
        
        public AIModel STT
        {
            get => _stt;
            set => _stt = value ?? throw new ArgumentNullException(nameof(STT));
        }
        
        public AIModel LLM
        {
            get => _llm;
            set => _llm = value ?? throw new ArgumentNullException(nameof(LLM));
        }
        
        public string Prompt
        {
            get => _prompt;
            set => _prompt = value ?? throw new ArgumentNullException(nameof(Prompt));
        }
        
        private static InstructorMenuModel _instance;
        public static InstructorMenuModel Instance => _instance ??= new InstructorMenuModel();
        
        private InstructorMenuModel(){ }

        public void Reset(AIModel tts, AIModel stt, AIModel llm,  string prompt = "")
        {
            TTS = tts ?? throw new ArgumentNullException(nameof(tts));
            STT = stt ?? throw new ArgumentNullException(nameof(stt));
            LLM = llm ?? throw new ArgumentNullException(nameof(llm));
            Prompt = prompt;
        }

    }
}