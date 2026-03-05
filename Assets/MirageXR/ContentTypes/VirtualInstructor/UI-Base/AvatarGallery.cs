using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class AvatarGallery : MonoBehaviour
    {
        [SerializeField] private Transform thumbnailGrid;
        [SerializeField] private GameObject characterThumbnailPrefab;
        [SerializeField] private GameObject placeholderForEmptyGallery;

        private List<CharacterThumbnailView> _characterThumbnails = new List<CharacterThumbnailView>();

        public delegate void ModelSelectedHandler(string characterId);
        public event ModelSelectedHandler CharacterModelSelected;

        private List<string> _avatars = new List<string>();

        public List<string> Avatars
        {
            get => _avatars;
            set
            {
                _avatars = value;
                RefreshThumbnails();
            }
        }

        public void RefreshThumbnails()
        {
            placeholderForEmptyGallery.SetActive(Avatars.Count == 0);

            for (int i = 0; i < _characterThumbnails.Count; i++)
            {
                bool visible = i < Avatars.Count;
                _characterThumbnails[i].gameObject.SetActive(visible);
            }

            for (int i = 0; i < Avatars.Count; i++)
            {
                string avatarId = Avatars[i];
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

                _characterThumbnails[i].CharacterModelId = avatarId;
            }
        }

        private void OnCharacterSelected(string characterId)
        {
            CharacterModelSelected?.Invoke(characterId);
        }
    }
}
