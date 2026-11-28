using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DataModelContentType = LearningExperienceEngine.DataModel.ContentType;

namespace MirageXR
{
    /// <summary>
    /// Mobile-specific implementation of the instructor setup view.
    /// Inherits shared logic from AbstractVirtualInstructorMenu and handles the 
    /// mobile UI for selecting characters, configuring AI models, and setting prompts.
    /// Provides user interactions for audio mode selection, character listing, and 
    /// updates the instructor content based on user inputs.
    /// </summary>
    public class VirtualInstructorViewMobile : AbstractVirtualInstructorMenu
    {
        [FormerlySerializedAs("_panel")]
        [Header("Panels")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private ReplaceModel avatarModelSettingsPanel;
        [SerializeField] private GameObject promptSettingsPanel;
        [SerializeField] private GameObject voiceSettingsPanel;
        [SerializeField] private GameObject llmModelSettingsPanel;
        [SerializeField] private GameObject languageSettingsPanel;
        [SerializeField] private TMP_InputField voiceInstructionInputField;

        [Header("UI Elements")]
        [SerializeField] private GallerySelectionElement characterModelSelectionElement;
        [SerializeField] private ChangeableSettingsPanel promptSettingsElement;
        [SerializeField] private ChangeableSettingsPanel voiceSettingsElement;
        [SerializeField] private ChangeableSettingsPanel llmModelSettingsElement;
        [SerializeField] private ChangeableSettingsPanel languageSettingsElement;

        [Header("Configuration Checkboxes")]
        [SerializeField] private Toggle _togglePersistentThread;
        [SerializeField] private Toggle _toggleStartConversation;
        [SerializeField] private Toggle _toggleFollowUser;
        [SerializeField] private Toggle _toggleEyesFollowUser;
        [SerializeField] private Toggle _toggleStandOnFloorPlane;

        //[Header("Audio Mode Toggle")]
        //[SerializeField] private Toggle[] audioToggles;
        //[SerializeField] private GameObject audioSetting;
        //[SerializeField] private TextMeshProUGUI audioMenuText;
        //[SerializeField] private GameObject audioRecodingMenu;
        //[SerializeField] private GameObject aiMenu;
        //[SerializeField] private GameObject noSpeech;

        [Header("Magic Numbers")]
        [SerializeField] private string defaultCharacter = "Hanna";

        private string _prefabName;
        private bool _useReadyPlayerMe;
        private string _characterModelUrl;
        private string _voiceInstruction;

        private VirtualInstructorSubMenu _shownSubMenu = VirtualInstructorSubMenu.GeneralSettings;

        private VirtualInstructorSubMenu ShownSubMenu
        {
            get => _shownSubMenu;
            set
            {
                settingsPanel.SetActive(value == VirtualInstructorSubMenu.GeneralSettings);
                avatarModelSettingsPanel.gameObject.SetActive(value == VirtualInstructorSubMenu.CharacterModelSettings);
                promptSettingsPanel.SetActive(value == VirtualInstructorSubMenu.PromptSettings);
                voiceSettingsPanel.SetActive(value == VirtualInstructorSubMenu.VoiceSettings);
                llmModelSettingsPanel.SetActive(value == VirtualInstructorSubMenu.AIModelSettings);
                languageSettingsPanel.SetActive(value == VirtualInstructorSubMenu.LanguageSettings);
                _shownSubMenu = value;
            }
        }

        public override DataModelContentType editorForType => DataModelContentType.Instructor;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            _showBackground = false;
            base.Initialization(onClose, args);

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case LearningExperienceEngine.Action step:
                        _step = step;
                        break;
                    case Content content:
                        Content = content;
                        IsContentUpdate = true;
                        break;
                }
            }

            voiceInstructionInputField.onValueChanged.AddListener(OnVoiceInstructionChanged);
            characterModelSelectionElement.GalleryElementSelectionStarted += OpenCharacterModelSettingPanel;
            avatarModelSettingsPanel.CharacterModelSelected += OnAvatarModelSelected;

            characterModelSelectionElement.Thumbnail.ThumbnailProvider = RootObject.Instance.AvatarLoadManager;

            InitializeDefaults();
            //RegisterEvents();

            RootView_v2.Instance.HideBaseView();

            ShownSubMenu = VirtualInstructorSubMenu.GeneralSettings;

            EnsureTogglesInitialized();

            if (IsContentUpdate && Content is Content<InstructorContentData> instructorContent && instructorContent.ContentData != null)
            {
                var data = instructorContent.ContentData;
                _voiceInstruction = data.VoiceInstruction;
                voiceInstructionInputField.text = _voiceInstruction ?? string.Empty;

                _useReadyPlayerMe = data.UseReadyPlayerMe;
                _characterModelUrl = data.CharacterModelUrl;
                _prefabName = data.CharacterName;

                if (_useReadyPlayerMe && !string.IsNullOrEmpty(_characterModelUrl))
                {
                    characterModelSelectionElement.Thumbnail.ElementId = AvatarLoadUtils.GetId(_characterModelUrl);
                }
                else if (!string.IsNullOrEmpty(_prefabName))
                {
                    characterModelSelectionElement.Thumbnail.ElementId = _prefabName;
                }

                if (!string.IsNullOrEmpty(data.Prompt))
                {
                    SetPrompt(data.Prompt);
                }

                if (promptSettingsPanel != null)
                {
                    var promptInputs = promptSettingsPanel.GetComponentsInChildren<TMP_InputField>(true);
                    foreach (var input in promptInputs)
                    {
                        input.text = data.Prompt ?? string.Empty;
                    }
                    var contextPrompt = promptSettingsPanel.GetComponentInChildren<ContextPromptMobile>(true);
                    if (contextPrompt != null && !string.IsNullOrEmpty(data.Prompt))
                    {
                        contextPrompt.SetPromptText(data.Prompt);
                    }
                }

                if (data.TextToSpeechModel != null)
                {
                    SetTTS(data.TextToSpeechModel);
                }
                if (data.SpeechToTextModel != null)
                {
                    SetSTT(data.SpeechToTextModel);
                }
                if (data.LanguageModel != null)
                {
                    SetLLM(data.LanguageModel);
                }

                if (_togglePersistentThread != null) _togglePersistentThread.isOn = data.PersistentThread == true;
                if (_toggleStartConversation != null) _toggleStartConversation.isOn = data.StartConversation == true;
                if (_toggleFollowUser != null) _toggleFollowUser.isOn = data.FollowUser;
                if (_toggleEyesFollowUser != null) _toggleEyesFollowUser.isOn = data.EyesFollowUser;
                if (_toggleStandOnFloorPlane != null) _toggleStandOnFloorPlane.isOn = data.StandOnFloorPlane;

                UpdateUiFromModel();
            }
            else
            {
                if (_toggleStandOnFloorPlane != null) _toggleStandOnFloorPlane.isOn = true;
            }
        }

        private void OnVoiceInstructionChanged(string text)
        {
            _voiceInstruction = text;
        }

        //private void RegisterEvents()
        //{
        //	for (int i = 0; i < audioToggles.Length; i++)
        //	{
        //		int index = i;
        //		audioToggles[i].onValueChanged.AddListener(isOn =>
        //		{
        //			if (isOn) HandleAudioToggleChange(index);
        //		});
        //	}
        //}

        protected override void OnAccept()
        {
            SaveAndCloseAsync().Forget();
        }

        private async UniTaskVoid SaveAndCloseAsync()
        {
            bool noCharacterSelected =
                (!_useReadyPlayerMe && string.IsNullOrEmpty(_prefabName))
                || _useReadyPlayerMe && string.IsNullOrEmpty(_characterModelUrl);

            if (noCharacterSelected && !IsContentUpdate)
            {
                Toast.Instance.Show("[Instructor] No character selected.");
                return;
            }

            string threadId = null;
            string assistantId = null;
            bool isPersistent = _togglePersistentThread != null && _togglePersistentThread.isOn;

            var prompt = GetPrompt();
            var charName = string.IsNullOrEmpty(_prefabName) ? defaultCharacter : _prefabName;
            var llm = GetLLM();

            bool needsReinitialization = false;

            if (IsContentUpdate && Content is Content<InstructorContentData> existingInstructor && existingInstructor.ContentData != null)
            {
                var prevData = existingInstructor.ContentData;
                threadId = prevData.ThreadId;
                assistantId = prevData.AssistantId;

                bool promptChanged = prevData.Prompt != prompt;
                bool nameChanged = prevData.CharacterName != charName;
                bool modelChanged = prevData.LanguageModel?.ApiName != llm?.ApiName;
                bool toggledToPersistent = (prevData.PersistentThread != true) && isPersistent;

                if (promptChanged || nameChanged || modelChanged || toggledToPersistent)
                {
                    needsReinitialization = true;
                }
            }
            else
            {
                needsReinitialization = true;
            }

            if (isPersistent)
            {
                if (needsReinitialization || string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(assistantId))
                {
                    try
                    {
                        var initResp = await RootObject.Instance.LEE.ArtificialIntelligenceManager.InitializeInstructorAsync(
                            charName,
                            prompt,
                            llm?.ApiName);
                        if (initResp != null)
                        {
                            threadId = initResp.ThreadId;
                            assistantId = initResp.AssistantId;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Instructor] Failed to pre-initialize persistent thread: {ex.Message}");
                    }
                }
            }
            else
            {
                threadId = null;
                assistantId = null;
            }

            var data = new InstructorContentData
            {
                AnimationClip = "Idle", // todo temp (UI reqest!)
                CharacterName = charName,
                TextToSpeechModel = GetTTS(),
                Prompt = prompt,
                LanguageModel = llm,
                SpeechToTextModel = GetSTT(),
                UseReadyPlayerMe = _useReadyPlayerMe,
                CharacterModelUrl = _characterModelUrl,
                VoiceInstruction = _voiceInstruction,
                PersistentThread = isPersistent,
                ThreadId = threadId,
                AssistantId = assistantId,
                StartConversation = _toggleStartConversation != null && _toggleStartConversation.isOn,
                FollowUser = _toggleFollowUser != null && _toggleFollowUser.isOn,
                EyesFollowUser = _toggleEyesFollowUser != null && _toggleEyesFollowUser.isOn,
                StandOnFloorPlane = _toggleStandOnFloorPlane == null || _toggleStandOnFloorPlane.isOn
            };

            var content = CreateContent<InstructorContentData>(DataModelContentType.Instructor);
            content.ContentData = data;

            if (IsContentUpdate)
            {
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            }
            else
            {
                RootObject.Instance.LEE.ContentManager.AddContent(content);
            }

            Close();
        }

        //private void HandleAudioToggleChange(int index)
        //{
        //	audioMenuText.text = index switch
        //	{
        //		0 => "Idle",
        //		1 => "Audio recording",
        //		2 => "AI",
        //		_ => "Unknown"
        //	};

        //	aiMenu.SetActive(index == 2);
        //	audioRecodingMenu.SetActive(index == 1);
        //	noSpeech.SetActive(index == 0);
        //	audioSetting.SetActive(false);
        //}

        private void OnDestroy()
        {
            RootView_v2.Instance.ShowBaseView();
        }

        /// <inheritdoc/>
        protected override void UpdateUiFromModel()
        {
            string prompt = GetPrompt();
            promptSettingsElement.CurrentValue = prompt == defaultPrompt ? "Enter Text" : "Change Text";

            AIModel tts = GetTTS();
            voiceSettingsElement.CurrentValue = tts.Name;

            AIModel llm = GetLLM();
            llmModelSettingsElement.CurrentValue = llm.Name;

            AIModel stt = GetSTT();
            languageSettingsElement.CurrentValue = stt.Name;
        }

        public override void Close()
        {
            // we are reusing the close button for sub-menus
            // if we are in the general settings, we can close the popup menu as normal
            // if we are in a sub-menu, first return one hierarchy level
            if (ShownSubMenu == VirtualInstructorSubMenu.GeneralSettings)
            {
                base.Close();
            }
            else
            {
                ShownSubMenu = VirtualInstructorSubMenu.GeneralSettings;
            }
        }

        private void OnAvatarModelSelected(string characterModelId)
        {
            _useReadyPlayerMe = true;
            _characterModelUrl = AvatarLoadUtils.IdToModelUrl(characterModelId);
            characterModelSelectionElement.Thumbnail.ElementId = characterModelId;
            ShownSubMenu = VirtualInstructorSubMenu.GeneralSettings;
        }

        private void OpenCharacterModelSettingPanel() => ShownSubMenu = VirtualInstructorSubMenu.CharacterModelSettings;

        public void OpenPromptSettingsPanel() => ShownSubMenu = VirtualInstructorSubMenu.PromptSettings;

        public void OpenVoiceSettingsPanel() => ShownSubMenu = VirtualInstructorSubMenu.VoiceSettings;

        public void OpenAIModelSettingsPanel() => ShownSubMenu = VirtualInstructorSubMenu.AIModelSettings;

        public void OpenLanguageSettingsPanel() => ShownSubMenu = VirtualInstructorSubMenu.LanguageSettings;

        private void EnsureTogglesInitialized()
        {
            if (_togglePersistentThread == null) _togglePersistentThread = FindToggleInSettings("Persistent thread");
            if (_toggleStartConversation == null) _toggleStartConversation = FindToggleInSettings("Start conversation immediately");
            if (_toggleFollowUser == null) _toggleFollowUser = FindToggleInSettings("Follow user");
            if (_toggleEyesFollowUser == null) _toggleEyesFollowUser = FindToggleInSettings("Eyes follow user");
            if (_toggleStandOnFloorPlane == null) _toggleStandOnFloorPlane = FindToggleInSettings("Stand on floor plane");
        }

        private Toggle FindToggleInSettings(string label)
        {
            if (settingsPanel == null) return null;
            var texts = settingsPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                if (text.text.Equals(label, StringComparison.OrdinalIgnoreCase))
                {
                    var toggle = text.transform.parent != null ? text.transform.parent.GetComponentInChildren<Toggle>(true) : null;
                    if (toggle != null) return toggle;
                }
            }
            return null;
        }
    }

    [Serializable]
    public enum VirtualInstructorSubMenu
    {
        GeneralSettings,
        CharacterModelSettings,
        PromptSettings,
        VoiceSettings,
        AIModelSettings,
        LanguageSettings,
    }
}
