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
		[SerializeField] private CharacterModelSelectionElement characterModelSelectionElement;
		[SerializeField] private ChangeableSettingsPanel promptSettingsElement;
		[SerializeField] private ChangeableSettingsPanel voiceSettingsElement;
		[SerializeField] private ChangeableSettingsPanel llmModelSettingsElement;
		[SerializeField] private ChangeableSettingsPanel languageSettingsElement;

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
			characterModelSelectionElement.CharacterModelSelectionStarted += OpenCharacterModelSettingPanel;
			avatarModelSettingsPanel.CharacterModelSelected += OnAvatarModelSelected;

			InitializeDefaults();
			//RegisterEvents();

			RootView_v2.Instance.HideBaseView();

			ShownSubMenu = VirtualInstructorSubMenu.GeneralSettings;

			if (IsContentUpdate)
			{
				var content = Content as Content<InstructorContentData>;
				voiceInstructionInputField.text = content?.ContentData?.VoiceInstruction ?? string.Empty;
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
			bool noCharacterSelected =
				(!_useReadyPlayerMe && string.IsNullOrEmpty(_prefabName))
				|| _useReadyPlayerMe && string.IsNullOrEmpty(_characterModelUrl);

			if (noCharacterSelected && !IsContentUpdate)
			{
				Toast.Instance.Show("[Instructor] No character selected.");
				return;
			}

			var data = new InstructorContentData
			{
				AnimationClip = "Idle", // todo temp (UI reqest!)
				CharacterName = string.IsNullOrEmpty(_prefabName) ? defaultCharacter : _prefabName,
				TextToSpeechModel = GetTTS(),
				Prompt = GetPrompt(),
				LanguageModel = GetLLM(),
				SpeechToTextModel = GetSTT(),
				UseReadyPlayerMe = _useReadyPlayerMe,
				CharacterModelUrl = _characterModelUrl,
				VoiceInstruction = _voiceInstruction
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

		private void OnAvatarModelSelected(string characterModelUrl)
		{
			_useReadyPlayerMe = true;
			_characterModelUrl = characterModelUrl;
			characterModelSelectionElement.Thumbnail.CharacterModelUrl = _characterModelUrl;
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
