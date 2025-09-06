using System;
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
			confirmation.SetActive(false);
		}

		private void AddCharacterToLibrary(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return;
			}
			RootObject.Instance.AvatarLibraryManager.AddAvatar(url);
			CharacterSelected?.Invoke(url);
			confirmation.SetActive(true);
			inputField.text = "";
		}
	}
}
