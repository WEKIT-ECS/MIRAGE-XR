using MirageXR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceModel : MonoBehaviour
{
    [SerializeField] private GallerySelectionElement addNewCharacter;
    [SerializeField] private AddModelPanel addCharacterPanel;
    [SerializeField] private AvatarGallery localCharacterGallery;
    [SerializeField] private AvatarGallery serverCharacterGallery;

    [SerializeField] private Button close;

    public delegate void ModelSelectedHandler(string characterId);
    public event ModelSelectedHandler CharacterModelSelected;

    void Start()
    {
        addNewCharacter.GalleryElementSelectionStarted += OpenAddCharacterMenu;
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
        localCharacterGallery.ElementSelected += OnLocalCharacterSelected;
        serverCharacterGallery.ElementSelected += OnServerCharacterSelected;
        await InitializeGalleriesAsync();
    }

    private void OnDisable()
    {
        addCharacterPanel.CharacterSelected -= NewCharacterAdded;
        localCharacterGallery.ElementSelected -= OnLocalCharacterSelected;
        serverCharacterGallery.ElementSelected -= OnServerCharacterSelected;
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private async Task InitializeGalleriesAsync()
    {
        RefreshLocalGallery();
        serverCharacterGallery.ElementIds = new List<string>(await RootObject.Instance.AvatarLoadManager.GetListOfAvatarsAsync());
    }

    private void RefreshLocalGallery()
    {
        localCharacterGallery.ElementIds = RootObject.Instance.AvatarLibraryManager.AvatarList;
    }

    private void NewCharacterAdded(string characterId)
    {
        RefreshLocalGallery();
    }

    private void OnLocalCharacterSelected(string characterId)
    {
        CharacterModelSelected?.Invoke(characterId);
    }

    private void OnServerCharacterSelected(string characterId)
    {
        RootObject.Instance.AvatarLibraryManager.AddAvatar(characterId);
        RefreshLocalGallery();
        CharacterModelSelected?.Invoke(characterId);
    }
}
