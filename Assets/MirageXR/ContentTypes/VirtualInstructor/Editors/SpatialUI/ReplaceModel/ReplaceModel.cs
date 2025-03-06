using MirageXR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceModel : MonoBehaviour
{
	[SerializeField] private Button addNewCharacter;
	[SerializeField] private AddModelPanel addCharacterPanel;
	[SerializeField] private Transform thumbnailGrid;
	[SerializeField] private GameObject characterThumbnailPrefab;
	[SerializeField] private GameObject characterChip;

	[SerializeField] private Button close;

	private List<CharacterThumbnailView> _characterThumbnails = new List<CharacterThumbnailView>();

	void Start()
	{
		addCharacterPanel.ModelSelected += NewModelAdded;
		addNewCharacter.onClick.AddListener(() => addCharacterPanel.gameObject.SetActive(true));
		close.onClick.AddListener(() => Close());
	}

	private void OnDestroy()
	{
		addCharacterPanel.ModelSelected -= NewModelAdded;
	}

	private void Close()
	{
		gameObject.SetActive(false);
	}

	private async void NewModelAdded(string modelUrl)
	{
		await RefreshAsync();
	}

	public async Task RefreshAsync()
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
				_characterThumbnails.Add(characterThumbnailView);
			}

			_characterThumbnails[i].DisplayedThumbnail = await RootObject.Instance.AvatarLibraryManager.GetThumbnailAsync(avatarUrl);
		}
	}
}
