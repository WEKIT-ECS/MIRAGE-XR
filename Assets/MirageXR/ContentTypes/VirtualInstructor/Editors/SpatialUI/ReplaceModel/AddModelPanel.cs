using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
	public class AddModelPanel : MonoBehaviour
	{

		[Header("GameObject")]
		[SerializeField] private GameObject customLinkOpen;
		[SerializeField] private GameObject customLinkClose;
		[SerializeField] private GameObject confirmation;
		[SerializeField] private GameObject errorMessage;
		[SerializeField] private GameObject waitSpinner;

		[Header("Buttons")]
		[SerializeField] private Button addModelBtn;
		[SerializeField] private Button closeWindowBtn;
		[SerializeField] private Button openCustomLink;
		[SerializeField] private Button closeCustomLink;

		[FormerlySerializedAs("_inputField")]
		[Header("Input Field")]
		[SerializeField]
		private TMP_InputField inputField;

		public delegate void CharacterSelectedHandler(string characterId);
		public event CharacterSelectedHandler CharacterSelected;

		void Start()
		{
			closeWindowBtn.onClick.AddListener(() => this.gameObject.SetActive(false));
			addModelBtn.onClick.AddListener(() => AddCharacterToLibrary(inputField.text));
			if (openCustomLink)
			{
				openCustomLink.onClick.AddListener(() => { customLinkOpen.SetActive(true); customLinkClose.SetActive(false); });
			}
			else if (RootObject.Instance.PlatformManager.GetUiType() == UiType.Spatial)
			{
				Debug.LogWarning(nameof(openCustomLink) + " is not assigned but we are in the spatial UI.", this);
			}
			if (closeCustomLink)
			{
				closeCustomLink.onClick.AddListener(() => { customLinkClose.SetActive(true); customLinkOpen.SetActive(false); });
			}
			else if (RootObject.Instance.PlatformManager.GetUiType() == UiType.Spatial)
			{
				Debug.LogWarning(nameof(closeCustomLink) + " is not assigned but we are in the spatial UI.", this);
			}
		}

		private void OnEnable()
		{
			addModelBtn.interactable = true;
			confirmation.SetActive(false);
			errorMessage.SetActive(false);
			waitSpinner.SetActive(false);
		}

		private async void AddCharacterToLibrary(string urlOrId)
		{
			if (string.IsNullOrWhiteSpace(urlOrId))
			{
				return;
			}

			string avatarId = RPMUtils.GetId(urlOrId);

			waitSpinner.SetActive(true);
			addModelBtn.interactable = false;

			RPMMetaData metaData = await RPMUtils.GetMetadataAsync(avatarId);
			bool valid = metaData != null;

			waitSpinner.SetActive(false);
			addModelBtn.interactable = true;

			if (valid)
			{
				// we take the id from the metaData because it is guaranteed to be readable by RPM
				// this way, we avoid adding shortcodes here because they don't seem to work with the thumbnail API
				RootObject.Instance.AvatarLibraryManager.AddAvatar(metaData.id);
				CharacterSelected?.Invoke(metaData.id);
				inputField.text = "";
			}
			confirmation.SetActive(valid);
			errorMessage.SetActive(!valid);
		}
	}
}
