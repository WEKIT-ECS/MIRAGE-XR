using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
	/// <summary>
	/// UI container for listing and selecting AI models (LLM, TTS, STT) in the mobile setup workflow.
	/// Dynamically populates a list of available models and handles user interactions 
	/// for model selection and audio playback.
	/// Selected models are passed back to the menu component.
	/// </summary>
	public class ContentContainerMobile : MonoBehaviour
	{
		[SerializeField] private ContentTypeEndpoint selectedType;
		[SerializeField] private GameObject prefabTemplate;
		[SerializeField] private RectTransform container;
		[SerializeField] private GameObject audioPlayer;
		[SerializeField] private VirtualInstructorViewMobile settingsMenu;
		[SerializeField] private ScrollRect scrollRect;

		private List<AIModel> _availableModels;
		private readonly List<GameObject> _instantiatedPrefabs = new();

		private void Start()
		{
			// se 
			if (scrollRect)
			{
				scrollRect.movementType = ScrollRect.MovementType.Clamped;
				scrollRect.elasticity = 0.7f;
				scrollRect.inertia = true;
				scrollRect.decelerationRate = 0.05f;
				scrollRect.scrollSensitivity = 1.2f;
			}

			_availableModels = selectedType switch
			{
				ContentTypeEndpoint.Llm => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetLlmModels(),
				ContentTypeEndpoint.Tts => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetTtsModels(),
				ContentTypeEndpoint.Stt => RootObject.Instance.LEE.ArtificialIntelligenceManager.GetSttModels(),
				_ => new List<AIModel>()
			};

			if (_availableModels.Count == 0)
			{
				Debug.LogError("[ContentContainerMobile] No models found for selected type.");
				return;
			}

			foreach (var model in _availableModels)
				CreateModelEntry(model);

			StartCoroutine(FixLayoutNextFrame());
		}

		private System.Collections.IEnumerator FixLayoutNextFrame()
		{
			yield return null;
			LayoutRebuilder.ForceRebuildLayoutImmediate(container);
		}


		private void CreateModelEntry(AIModel model)
		{
			var go = Instantiate(prefabTemplate, container);
			_instantiatedPrefabs.Add(go);

			var texts = go.GetComponentsInChildren<TMP_Text>();
			if (texts.Length > 0) texts[0].text = model.Name;
			if (texts.Length > 1) texts[1].text = model.Description;

			var toggle = go.GetComponentInChildren<Toggle>();
			if (toggle)
			{
				var group = container.GetComponentInChildren<ToggleGroup>();
				if (group) toggle.group = group;
				toggle.onValueChanged.AddListener(isOn =>
				{
					if (isOn) OnModelSelected(model);
				});
			}

			var button = go.GetComponentInChildren<Button>();
			if (button)
			{
				button.onClick.AddListener(() =>
				{
					//audioPlayer.SetActive(true);
					//var player = audioPlayer.GetComponent<AudioStreamPlayer>() ??
					//			 audioPlayer.AddComponent<AudioStreamPlayer>();
					//player.Setup(model);
					GetComponentInParent<VoicePreviewLoader>().PlayVoicePreview(model);
				});
			}
		}

		private void OnModelSelected(AIModel model)
		{
			switch (selectedType)
			{
				case ContentTypeEndpoint.Llm:
					settingsMenu.SetLLM(model);
					break;
				case ContentTypeEndpoint.Tts:
					settingsMenu.SetTTS(model);
					break;
				case ContentTypeEndpoint.Stt:
					settingsMenu.SetSTT(model);
					break;
			}
		}

		/// <summary>
		/// Represents the endpoint types for different AI wrapper. Provides
		/// a standardized way to specify and manage AI model menus.
		/// </summary>
		public enum ContentTypeEndpoint
		{
			/// <summary>
			/// Represents the Text-to-Speech (TTS) endpoint.
			/// </summary>
			Tts,

			/// <summary>
			/// Represents the Speech-to-Text (STT) endpoint.
			/// </summary>
			Stt,

			/// <summary>
			/// Endpoint for interactions with Large Language Models (LLMs) and RAG model.
			/// </summary>
			Llm
		}
	}
}