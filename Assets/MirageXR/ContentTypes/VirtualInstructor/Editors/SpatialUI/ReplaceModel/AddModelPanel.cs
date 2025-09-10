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

		public delegate void CharacterSelectedHandler(string characterUrl);
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
		}

		private async void AddCharacterToLibrary(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return;
			}

			string avatarId;
			// full URL:
			if (url.StartsWith("https://models.readyplayer.me/"))
			{
				avatarId = RPMUtils.GetAvatarID(url);
			}
			// could also be just the ID or a shortcode
			else
			{
				avatarId = Regex.Replace(url, "[^a-zA-Z0-9]", "");
			}

			waitSpinner.SetActive(true);
			addModelBtn.interactable = false;

			bool valid = await RPMUtils.IsValidIDAsync(avatarId);

			waitSpinner.SetActive(false);
			addModelBtn.interactable = true;

			if (valid)
			{
				RootObject.Instance.AvatarLibraryManager.AddAvatar(url);
				CharacterSelected?.Invoke(url);				
				inputField.text = "";
			}
			confirmation.SetActive(valid);
			errorMessage.SetActive(!valid);
		}
	}
}
