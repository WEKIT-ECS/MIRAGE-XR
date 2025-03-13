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
		[SerializeField] private GameObject close;
		[SerializeField] private GameObject customLinkOpen;
		[SerializeField] private GameObject customLinkClose;
		[SerializeField] private GameObject conformation;

		[Header("Buttons")]
		[SerializeField] private Button addModelBtn;
		[SerializeField] private Button closeBtn;
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
			openCustomLink.onClick.AddListener(() => { customLinkOpen.SetActive(true); customLinkClose.SetActive(false); });
			closeCustomLink.onClick.AddListener(() => { customLinkClose.SetActive(true); customLinkOpen.SetActive(false); });
			closeBtn.onClick.AddListener(() => this.gameObject.SetActive(false));
		}

		private void AddCharacterToLibrary(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return;
			}
			RootObject.Instance.AvatarLibraryManager.AddAvatar(url);
			CharacterSelected?.Invoke(url);
			conformation.SetActive(true);
			close.SetActive(true);
			inputField.text = "";
		}
	}
}
