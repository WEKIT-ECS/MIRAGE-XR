using System;
using System.Collections.Generic;
using DG.Tweening;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DataModelContentType = LearningExperienceEngine.DataModel.ContentType;

namespace MirageXR
{
    /// <summary>
    /// Concrete instructor setup view for mobile platform.
    /// Inherits shared logic from AbstractVirtualInstructorMenu and handles mobile-specific UI.
    /// </summary>
    public class VirtualInstructorViewMobile : AbstractVirtualInstructorMenu
    {
        [FormerlySerializedAs("_panel")]
        [Header("Panels")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject togglePanel;
        [SerializeField] private GameObject tabsPanel;

        [Header("Character List")]
        [SerializeField] private Button btnArrow;
        [SerializeField] private GameObject arrowDown;
        [SerializeField] private GameObject arrowUp;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private CharacterListItem characterListItemPrefab;
        [SerializeField] private CharacterObject[] characterObjects;

        [Header("Tabs")]
        [SerializeField] private Toggle toggleMyCharacters;
        [SerializeField] private Toggle toggleLibrary;
        [SerializeField] private GameObject charactersTab;
        [SerializeField] private GameObject libraryTab;

        [Header("Audio Mode Toggle")]
        [SerializeField] private Toggle[] audioToggles;
        [SerializeField] private GameObject audioSetting;
        [SerializeField] private TextMeshProUGUI audioMenuText;
        [SerializeField] private GameObject audioRecodingMenu;
        [SerializeField] private GameObject aiMenu;
        [SerializeField] private GameObject noSpeech;

        [Header("Magic Numbers")]
        [SerializeField] private const float HidedSize = 100f;
        [SerializeField] private const float HideAnimationTime = 0.5f;
        [SerializeField] private string defaultCharacter = "Hanna";
        
        private string _prefabName;

        public override DataModelContentType editorForType => DataModelContentType.Instructor;

        public override async void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            await RootObject.Instance.WaitForInitialization();
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

            InitializeDefaults();
            UpdateCharacterList();
            RegisterEvents();

            settingsPanel.SetActive(false);
            togglePanel.SetActive(true);
            tabsPanel.SetActive(true);

            RootView_v2.Instance.HideBaseView();
        }

        private void UpdateCharacterList()
        {
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

            foreach (var characterObject in characterObjects)
            {
                var item = Instantiate(characterListItemPrefab, contentContainer);
                item.Init(characterObject, OnCharacterSelected);
            }
        }

        private void RegisterEvents()
        {
            btnArrow.onClick.AddListener(OnArrowButtonPressed);
            toggleMyCharacters.onValueChanged.AddListener(OnToggleMyCharacters);
            toggleLibrary.onValueChanged.AddListener(OnToggleLibrary);

            for (int i = 0; i < audioToggles.Length; i++)
            {
                int index = i;
                audioToggles[i].onValueChanged.AddListener(isOn =>
                {
                    if (isOn) HandleAudioToggleChange(index);
                });
            }
        }

        private void OnCharacterSelected(string prefabName)
        {
            _prefabName = prefabName;
            settingsPanel.SetActive(true);
            togglePanel.SetActive(false);
            tabsPanel.SetActive(false);
        }

        protected override void OnAccept()
        {
            if (_content != null)
            {
                LearningExperienceEngine.EventManager.DeactivateObject(_content);
            }
            else
            {
                _content = augmentationManager.AddAugmentation(_step, GetOffset());
            }

            var data = new InstructorContentData
            {
                AnimationClip = "Idle",
                CharacterName = string.IsNullOrEmpty(_prefabName) ? defaultCharacter : _prefabName,
                TextToSpeechModel = GetTTS(),
                Prompt = GetPrompt(),
                LanguageModel = GetLLM(),
                SpeechToTextModel = GetSTT(),
                UseReadyPlayerMe = false,
                CharacterModelUrl = ""
            };

            RootObject.Instance.LEE.ContentManager.AddContent(CreateInstructorContent(data, RootObject.Instance.LEE.StepManager.CurrentStep.Id));
            Close();
        }

        private Content<InstructorContentData> CreateInstructorContent(InstructorContentData data, Guid stepID)
        {
            return new Content<InstructorContentData>
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                Steps = new List<Guid> { stepID },
                Type = DataModelContentType.Instructor,
                IsVisible = true,
                Location = Location.GetIdentityLocation(),
                ContentData = data
            };
        }

        private void OnArrowButtonPressed()
        {
            if (arrowDown.activeSelf)
            {
                panel.DOAnchorPosY(-panel.rect.height + HidedSize, HideAnimationTime);
                arrowDown.SetActive(false);
                arrowUp.SetActive(true);
            }
            else
            {
                panel.DOAnchorPosY(0.0f, HideAnimationTime);
                arrowDown.SetActive(true);
                arrowUp.SetActive(false);
            }
        }

        private void OnToggleLibrary(bool value)
        {
            libraryTab.SetActive(value);
            charactersTab.SetActive(!value);
        }

        private void OnToggleMyCharacters(bool value)
        {
            charactersTab.SetActive(value);
            libraryTab.SetActive(!value);
        }
        
        private void HandleAudioToggleChange(int index)
        {
            audioMenuText.text = index switch
            {
                0 => "No speech",
                1 => "Audio recording",
                2 => "AI",
                _ => "Unknown"
            };

            aiMenu.SetActive(index == 2);
            audioRecodingMenu.SetActive(index == 1);
            noSpeech.SetActive(index == 0);
            audioSetting.SetActive(false);
        }

        private void OnDestroy()
        {
            RootView_v2.Instance.ShowBaseView();
            toggleMyCharacters.onValueChanged.RemoveAllListeners();
            toggleLibrary.onValueChanged.RemoveAllListeners();
        }

        protected override void OnPromptUpdated(string prompt)
        {
            Debug.Log($"[InstructorViewMobile] Prompt set to: {prompt}");
        }

        protected override void OnTTSUpdated(AIModel model)
        {
            Debug.Log($"[InstructorViewMobile] TTS model: {model?.Name}");
        }

        protected override void OnSTTUpdated(AIModel model)
        {
            Debug.Log($"[InstructorViewMobile] STT model: {model?.Name}");
        }

        protected override void OnLLMUpdated(AIModel model)
        {
            Debug.Log($"[InstructorViewMobile] LLM model: {model?.Name}");
        }
    }
}
