using LearningExperienceEngine.DataModel;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

            EnsureCheckboxes();

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

        private void EnsureCheckboxes()
        {
            if (_togglePersistentThread == null)
                _togglePersistentThread = transform.Find("Panel/SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_PersistentThread")?.GetComponent<Toggle>()
                    ?? transform.Find("Panel/SettingsPanel/Viewport/Content/Toggle_PersistentThread")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_PersistentThread")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/Toggle_PersistentThread")?.GetComponent<Toggle>();

            if (_toggleStartConversation == null)
                _toggleStartConversation = transform.Find("Panel/SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_StartConversation")?.GetComponent<Toggle>()
                    ?? transform.Find("Panel/SettingsPanel/Viewport/Content/Toggle_StartConversation")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_StartConversation")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/Toggle_StartConversation")?.GetComponent<Toggle>();

            if (_toggleFollowUser == null)
                _toggleFollowUser = transform.Find("Panel/SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_FollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("Panel/SettingsPanel/Viewport/Content/Toggle_FollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_FollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/Toggle_FollowUser")?.GetComponent<Toggle>();

            if (_toggleEyesFollowUser == null)
                _toggleEyesFollowUser = transform.Find("Panel/SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_EyesFollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("Panel/SettingsPanel/Viewport/Content/Toggle_EyesFollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_EyesFollowUser")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/Toggle_EyesFollowUser")?.GetComponent<Toggle>();

            if (_toggleStandOnFloorPlane == null)
                _toggleStandOnFloorPlane = transform.Find("Panel/SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>()
                    ?? transform.Find("Panel/SettingsPanel/Viewport/Content/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/CheckboxesContainer/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>()
                    ?? transform.Find("SettingsPanel/Viewport/Content/Toggle_StandOnFloorPlane")?.GetComponent<Toggle>();

            if (_togglePersistentThread == null || _toggleStartConversation == null || _toggleFollowUser == null || _toggleEyesFollowUser == null || _toggleStandOnFloorPlane == null)
            {
                var content = transform.Find("Panel/SettingsPanel/Viewport/Content")
                    ?? transform.Find("SettingsPanel/Viewport/Content")
                    ?? (settingsPanel != null ? settingsPanel.transform : null);

                if (content != null)
                {
                    var containerTransform = content.Find("CheckboxesContainer");
                    if (containerTransform == null)
                    {
                        var containerGo = new GameObject("CheckboxesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                        containerGo.transform.SetParent(content, false);
                        var vlg = containerGo.GetComponent<VerticalLayoutGroup>();
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                        vlg.spacing = 10f;
                        containerTransform = containerGo.transform;

                        var applyBtn = content.Find("ApplyBtn");
                        if (applyBtn != null)
                        {
                            containerTransform.SetSiblingIndex(applyBtn.GetSiblingIndex());
                        }
                    }

                    if (_togglePersistentThread == null)
                        _togglePersistentThread = CreateCheckbox(containerTransform, "Toggle_PersistentThread", "One persistent, shared conversation for all users", false);
                    if (_toggleStartConversation == null)
                        _toggleStartConversation = CreateCheckbox(containerTransform, "Toggle_StartConversation", "Start conversation when step is loaded", false);
                    if (_toggleFollowUser == null)
                        _toggleFollowUser = CreateCheckbox(containerTransform, "Toggle_FollowUser", "Walk to follow the user", false);
                    if (_toggleEyesFollowUser == null)
                        _toggleEyesFollowUser = CreateCheckbox(containerTransform, "Toggle_EyesFollowUser", "Seek eye contact with the user", false);
                    if (_toggleStandOnFloorPlane == null)
                        _toggleStandOnFloorPlane = CreateCheckbox(containerTransform, "Toggle_StandOnFloorPlane", "Jump to floor plane", true);
                }
            }
        }

        private Toggle CreateCheckbox(Transform parent, string name, string labelText, bool defaultValue = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 32);

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = new Vector2(14, 0);
            bgRt.sizeDelta = new Vector2(24, 24);
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = new Color(0.22f, 0.24f, 0.28f, 1f);

            var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkRt = checkGo.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.5f, 0.5f);
            checkRt.anchorMax = new Vector2(0.5f, 0.5f);
            checkRt.pivot = new Vector2(0.5f, 0.5f);
            checkRt.anchoredPosition = Vector2.zero;
            checkRt.sizeDelta = new Vector2(16, 16);
            var checkImg = checkGo.GetComponent<Image>();
            checkImg.color = new Color(0.38f, 0.65f, 1f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.pivot = new Vector2(0, 0.5f);
            labelRt.offsetMin = new Vector2(36, 0);
            labelRt.offsetMax = new Vector2(0, 0);
            var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
            labelTmp.text = labelText;
            labelTmp.fontSize = 16;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            if (_txtLabel != null && _txtLabel.font != null)
            {
                labelTmp.font = _txtLabel.font;
            }

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = defaultValue;

            return toggle;
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

            EnsureCheckboxes();

            string threadId = null;
            string assistantId = null;
            bool isPersistent = _togglePersistentThread != null && _togglePersistentThread.isOn;

            if (IsContentUpdate && Content is Content<InstructorContentData> existingInstructor && existingInstructor.ContentData != null)
            {
                threadId = existingInstructor.ContentData.ThreadId;
                assistantId = existingInstructor.ContentData.AssistantId;
            }

            var prompt = GetPrompt();
            var charName = string.IsNullOrEmpty(_prefabName) ? defaultCharacter : _prefabName;
            var llm = GetLLM();

            if (isPersistent && (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(assistantId)))
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
