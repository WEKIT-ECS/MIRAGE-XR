using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
    /// <summary>
    /// Speech settings menu implementation for mobile setup.
    /// Manages prompt, TTS, STT, and LLM model views.
    /// </summary>
    public class SpeechSettingsMobile : AbstractVirtualInstructorMenu
    {
        [Header("Model Buttons")]
        [SerializeField] private Button promptButton;
        [SerializeField] private Button ttsButton;
        [SerializeField] private Button llmButton;
        [SerializeField] private Button sttButton;

        [Header("View Panels")]
        [SerializeField] private GameObject promptView;
        [SerializeField] private GameObject ttsView;
        [SerializeField] private GameObject llmView;
        [SerializeField] private GameObject sttView;

        [Header("Back Buttons")]
        [SerializeField] private Button promptBack;
        [SerializeField] private Button ttsBack;
        [SerializeField] private Button llmBack;
        [SerializeField] private Button sttBack;

        [Header("Text Fields")]
        [SerializeField] private GameObject promptLabel;
        [SerializeField] private GameObject ttsLabel;
        [SerializeField] private GameObject llmLabel;
        [SerializeField] private GameObject sttLabel;

        public override ContentType editorForType => ContentType.Instructor;

        private void Start()
        {
            InitializeDefaults();

            // Hook navigation
            promptButton.onClick.AddListener(() => ToggleView(promptView, true));
            ttsButton.onClick.AddListener(() => ToggleView(ttsView, true));
            llmButton.onClick.AddListener(() => ToggleView(llmView, true));
            sttButton.onClick.AddListener(() => ToggleView(sttView, true));

            promptBack.onClick.AddListener(() => ToggleView(promptView, false));
            ttsBack.onClick.AddListener(() => ToggleView(ttsView, false));
            llmBack.onClick.AddListener(() => ToggleView(llmView, false));
            sttBack.onClick.AddListener(() => ToggleView(sttView, false));
        }

        private void ToggleView(GameObject view, bool active)
        {
            if (view != null)
                view.SetActive(active);
        }

        public void UpdatePromptText(string prompt)
        {
            SetLabelText(promptLabel, prompt);
        }

        public void UpdateModelText(AIModel model, GameObject label)
        {
            if (model != null)
                SetLabelText(label, model.Name);
        }

        private void SetLabelText(GameObject labelObject, string text)
        {
            var tmp = labelObject.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.text = text.Length > 10 ? text.Substring(0, 10) : text;
        }

        protected override void OnPromptUpdated(string prompt)
        {
            UpdatePromptText(prompt);
        }

        protected override void OnTTSUpdated(AIModel model)
        {
            UpdateModelText(model, ttsLabel);
        }

        protected override void OnSTTUpdated(AIModel model)
        {
            UpdateModelText(model, sttLabel);
        }

        protected override void OnLLMUpdated(AIModel model)
        {
            UpdateModelText(model, llmLabel);
        }
    }
}
