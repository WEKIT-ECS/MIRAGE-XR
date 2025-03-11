using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	public class CharacterModelSelectionElement : MonoBehaviour
	{
		[SerializeField] private Button _addCharacterModelButton;
		[SerializeField] private TMP_Text _infoText;

		public CharacterThumbnailView Thumbnail { get; private set; }

		public Action CharacterModelSelectionStarted;

		private void Awake()
		{
			Thumbnail = GetComponent<CharacterThumbnailView>();
		}

		private void OnEnable()
		{
			Thumbnail.CharacterModelUrlChanged += OnCharacterUrlChanged;
			Thumbnail.CharacterModelSelected += OnThumbnailClicked;
			UpdateView();
		}

		private void OnDisable()
		{
			Thumbnail.CharacterModelUrlChanged -= OnCharacterUrlChanged;
			Thumbnail.CharacterModelSelected -= OnThumbnailClicked;
		}

		private void OnThumbnailClicked(string characterModelUrl)
		{
			StartCharacterModelSelection();
		}


		private void OnCharacterUrlChanged(string characterUrl)
		{
			UpdateView();
		}

		private void UpdateView()
		{
			bool characterModelSelected = !string.IsNullOrEmpty(Thumbnail.CharacterModelUrl);
			_addCharacterModelButton.gameObject.SetActive(!characterModelSelected);
			if (characterModelSelected)
			{
				_infoText.text = "Replace character model";
			}
			else
			{
				_infoText.text = "Set character model";
			}
		}

		public void StartCharacterModelSelection()
		{
			CharacterModelSelectionStarted?.Invoke();
		}
	}
}
