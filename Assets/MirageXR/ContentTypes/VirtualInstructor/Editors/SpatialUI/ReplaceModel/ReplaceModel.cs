using MirageXR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceModel : MonoBehaviour
{
	[SerializeField] private CharacterModelSelectionElement addNewCharacter;
	[SerializeField] private AddModelPanel addCharacterPanel;
	[SerializeField] private Transform thumbnailGrid;
	[SerializeField] private GameObject characterThumbnailPrefab;
	[SerializeField] private GameObject characterChip;

	[SerializeField] private Button close;

	private List<CharacterThumbnailView> _characterThumbnails = new List<CharacterThumbnailView>();

	public delegate void ModelSelectedHandler(string modelUrl);
	public event ModelSelectedHandler CharacterModelSelected;

	void Start()
	{
		addNewCharacter.CharacterModelSelectionStarted += OpenAddCharacterMenu;
		close.onClick.AddListener(() => Close());
	}

	private void OpenAddCharacterMenu()
	{
		addCharacterPanel.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		addCharacterPanel.CharacterSelected += NewCharacterAdded;
		RefreshThumbnails();
	}

	private void OnDisable()
	{
		addCharacterPanel.CharacterSelected -= NewCharacterAdded;
	}

	private void Close()
	{
		gameObject.SetActive(false);
	}

	private void NewCharacterAdded(string characterUrl)
	{
		RefreshThumbnails();
		RootObject.Instance.AvatarLibraryManager.Save();
	}

	public void RefreshThumbnails()
	{
		characterChip.SetActive(RootObject.Instance.AvatarLibraryManager.AvatarList.Count == 0);

		for (int i = 0; i < _characterThumbnails.Count; i++)
		{
			bool visible = i < RootObject.Instance.AvatarLibraryManager.AvatarList.Count;
			_characterThumbnails[i].gameObject.SetActive(visible);
		}

		for (int i = 0; i < RootObject.Instance.AvatarLibraryManager.AvatarList.Count; i++)
		{
			string avatarUrl = RootObject.Instance.AvatarLibraryManager.AvatarList[i];
			CharacterThumbnailView characterThumbnailView;
			if (i < _characterThumbnails.Count)
			{
				characterThumbnailView = _characterThumbnails[i];
			}
			else
			{
				GameObject thumbnailGo = Instantiate(characterThumbnailPrefab, thumbnailGrid);
				characterThumbnailView = thumbnailGo.GetComponent<CharacterThumbnailView>();
				characterThumbnailView.CharacterModelSelected += OnCharacterSelected;
				_characterThumbnails.Add(characterThumbnailView);
			}

			_characterThumbnails[i].CharacterModelUrl = avatarUrl;
		}
	}

	private void OnCharacterSelected(string characterUrl)
	{
		CharacterModelSelected?.Invoke(characterUrl);
	}
}
