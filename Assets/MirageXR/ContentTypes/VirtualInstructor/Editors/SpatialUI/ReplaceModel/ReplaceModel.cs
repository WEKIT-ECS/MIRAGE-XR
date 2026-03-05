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
    [SerializeField] private AvatarGallery localCharacterGallery;
    [SerializeField] private AvatarGallery serverCharacterGallery;

    [SerializeField] private Button close;

    public delegate void ModelSelectedHandler(string characterId);
    public event ModelSelectedHandler CharacterModelSelected;

    void Start()
    {
        addNewCharacter.CharacterModelSelectionStarted += OpenAddCharacterMenu;
        if (close != null)
        {
            close.onClick.AddListener(() => Close());
        }
    }

    private void OpenAddCharacterMenu()
    {
        addCharacterPanel.gameObject.SetActive(true);
    }

    private async void OnEnable()
    {
        addCharacterPanel.CharacterSelected += NewCharacterAdded;
        localCharacterGallery.CharacterModelSelected += OnLocalCharacterSelected;
        serverCharacterGallery.CharacterModelSelected += OnServerCharacterSelected;
        await InitializeGalleriesAsync();
    }

    private void OnDisable()
    {
        addCharacterPanel.CharacterSelected -= NewCharacterAdded;
        localCharacterGallery.CharacterModelSelected -= OnLocalCharacterSelected;
        serverCharacterGallery.CharacterModelSelected -= OnServerCharacterSelected;
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private async Task InitializeGalleriesAsync()
    {
        localCharacterGallery.Avatars = RootObject.Instance.AvatarLibraryManager.AvatarList;
        serverCharacterGallery.Avatars = new List<string>(await RootObject.Instance.AvatarLoadManager.GetListOfAvatarsAsync());
    }

    private void NewCharacterAdded(string characterId)
    {
        localCharacterGallery.Avatars = RootObject.Instance.AvatarLibraryManager.AvatarList;
    }

    private void OnLocalCharacterSelected(string characterId)
    {
        CharacterModelSelected?.Invoke(characterId);
    }

    private void OnServerCharacterSelected(string characterId)
    {
        RootObject.Instance.AvatarLibraryManager.AddAvatar(characterId);
        CharacterModelSelected?.Invoke(characterId);
    }
}
