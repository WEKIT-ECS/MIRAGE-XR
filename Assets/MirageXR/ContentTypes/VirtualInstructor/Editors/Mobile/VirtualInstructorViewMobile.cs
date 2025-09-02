using System;
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
	/// Mobile-specific implementation of the instructor setup view.
	/// Inherits shared logic from AbstractVirtualInstructorMenu and handles the 
	/// mobile UI for selecting characters, configuring AI models, and setting prompts.
	/// Provides user interactions for audio mode selection, character listing, and 
	/// updates the instructor content based on user inputs.
	/// </summary>
	public class VirtualInstructorViewMobile : AbstractVirtualInstructorMenu	//TODO: change as AddEditVirtualInstructor
	{
		[FormerlySerializedAs("_panel")]
		[Header("Panels")]
		[SerializeField] private RectTransform panel;
		[SerializeField] private GameObject settingsPanel;
		[SerializeField] private ReplaceModel avatarModelSettingPanel;

		[Header("Character List")]
		[SerializeField] private Button btnArrow;
		[SerializeField] private GameObject arrowDown;
		[SerializeField] private GameObject arrowUp;
		[SerializeField] private CharacterListItem characterListItemPrefab;
		[SerializeField] private CharacterObject[] characterObjects;

		[Header("UI Elements")]
		[SerializeField] private CharacterModelSelectionElement characterModelSelectionElement;

		[Header("Audio Mode Toggle")]
		[SerializeField] private TextMeshProUGUI audioMenuText;
		[SerializeField] private GameObject audioRecodingMenu;
		[SerializeField] private GameObject aiMenu;
		[SerializeField] private GameObject noSpeech;

		[Header("Magic Numbers")]
		private const float HidedSize = 100f;
		private const float HideAnimationTime = 0.5f;
		[SerializeField] private string defaultCharacter = "Hanna";
		[Space]
		[SerializeField] private Button aiModeButton;
		[SerializeField] private Button speechSettingsButton;
		[SerializeField] private GameObject speechSettingsPanel;
		[SerializeField] private TMP_InputField voiceInstructionInputField;

		private string _voiceInstruction;
		private string _prefabName;
		private bool _useReadyPlayerMe;
		private string _characterModelUrl;
		private SpeechType _speechType;

		private enum SpeechType
		{
			noSpech,
			audioRecording,
			aiMode
		}

		private VirtualInstructorSubMenu _shownSubMenu = VirtualInstructorSubMenu.GeneralSettings;

		private VirtualInstructorSubMenu ShownSubMenu
		{
			get => _shownSubMenu;
			set
			{
				settingsPanel.SetActive(value == VirtualInstructorSubMenu.GeneralSettings);
				avatarModelSettingPanel.gameObject.SetActive(value == VirtualInstructorSubMenu.CharacterModelSettings);
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

			voiceInstructionInputField.onValueChanged.AddListener(text => _voiceInstruction = text);;
			speechSettingsButton.onClick.AddListener(OnSpeechSettingsClicked);
			aiModeButton.onClick.AddListener(AIModeClicked);
			characterModelSelectionElement.CharacterModelSelectionStarted += OpenCharacterModelSettingPanel;
			avatarModelSettingPanel.CharacterModelSelected += OnAvatarModelSelected;

			InitializeDefaults();
			btnArrow.onClick.AddListener(OnArrowButtonPressed);

			RootView_v2.Instance.HideBaseView();

			ShownSubMenu = VirtualInstructorSubMenu.GeneralSettings;
		}

		private void AIModeClicked()
		{
			RootView_v2.Instance.dialog.ShowBottomMultilineToggles("Communication settings", 
				("Idle", OnIdleModeClicked, false, _speechType == SpeechType.noSpech),
				//("Audio Recording", OnAudioRecordingModeClicked, false, _speechType == SpeechType.audioRecording),
				("AI Mode", OnAIModeModeClicked, false, _speechType == SpeechType.aiMode)
			);
		}

		private void OnIdleModeClicked()
		{
			_speechType = SpeechType.noSpech;
			aiMenu.SetActive(false);
			noSpeech.SetActive(true);
		}

		private void OnAudioRecordingModeClicked()
		{
			_speechType = SpeechType.audioRecording;
			aiMenu.SetActive(false);
			noSpeech.SetActive(false);
		}

		private void OnAIModeModeClicked()
		{
			_speechType = SpeechType.aiMode;
			aiMenu.SetActive(true);
			noSpeech.SetActive(false);
		}

		private void OnSpeechSettingsClicked()
		{
			speechSettingsPanel.SetActive(true);	//TODO: spawn speech settings panel via PopupsViewer.Instance.Show(prefab, callback)
		}

		protected override void OnAccept()
		{
			var noCharacterSelected = (!_useReadyPlayerMe && string.IsNullOrEmpty(_prefabName))
			                              || _useReadyPlayerMe && string.IsNullOrEmpty(_characterModelUrl);

			if (noCharacterSelected && !IsContentUpdate)
			{
				Debug.LogWarning("[Instructor] No character selected.");
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
				VoiceInstruction = _voiceInstruction,
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

		private void OnDestroy()
		{
			RootView_v2.Instance.ShowBaseView();
		}

		/// <inheritdoc/>
		protected override void UpdateUiFromModel()
		{

		}

		public override void Close()
		{
			// we are reusing the close button
			// if we are in the general settings, we can close the popup menu as normal
			// if we are in a sub-menu, first return one hierarchy level
			//TODO: use PopupsViewer.Instance.Show(prefab, callback, ...) for showing the sub-menus
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

		private void OpenCharacterModelSettingPanel()
		{
			ShownSubMenu = VirtualInstructorSubMenu.CharacterModelSettings;
		}

		//private void ResetPanel()
		//{
		//	settingsPanel.SetActive(false);
		//          avatarModelSettingPanel.gameObject.SetActive(false);
		//          communicationSettingPanel.SetActive(false);
		//          animationSettingPanel.SetActive(false);
		//          pathSettingPanel.SetActive(false);
		//      }

		enum VirtualInstructorSubMenu
		{
			GeneralSettings,
			CharacterModelSettings
		}
	}
}
