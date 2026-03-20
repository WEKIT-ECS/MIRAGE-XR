using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public abstract class GalleryBase : MonoBehaviour
    {
        [SerializeField] private Transform thumbnailGrid;
        [SerializeField] private GameObject thumbnailPrefab;
        [SerializeField] private GameObject placeholderForEmptyGallery;
        [SerializeField] private bool elementsDeleteable = false;

        private List<ThumbnailView> _thumbnails = new List<ThumbnailView>();

        public delegate void GalleryElementEventHandler(string elementId);
        public event GalleryElementEventHandler ElementSelected;

        private List<string> _elementIds = new List<string>();

        public List<string> ElementIds
        {
            get => _elementIds;
            set
            {
                _elementIds = value;
                RefreshThumbnails();
            }
        }

        protected abstract IThumbnailProvider ThumbnailProvider { get; }

        public virtual void RefreshThumbnails()
        {
            placeholderForEmptyGallery.SetActive(ElementIds.Count == 0);

            for (int i = 0; i < _thumbnails.Count; i++)
            {
                bool visible = i < ElementIds.Count;
                _thumbnails[i].gameObject.SetActive(visible);
            }

            for (int i = 0; i < ElementIds.Count; i++)
            {
                string avatarId = ElementIds[i];
                ThumbnailView thumbnailView;
                if (i < _thumbnails.Count)
                {
                    thumbnailView = _thumbnails[i];
                }
                else
                {
                    GameObject thumbnailGo = Instantiate(thumbnailPrefab, thumbnailGrid);
                    thumbnailView = thumbnailGo.GetComponent<ThumbnailView>();
                    thumbnailView.ThumbnailProvider = ThumbnailProvider;
                    thumbnailView.ElementSelected += OnElementSelected;
                    thumbnailView.Deleteable = elementsDeleteable;
                    if (elementsDeleteable)
                    {
                        thumbnailView.ElementDeleted += OnElementDeleted;
                    }
                    _thumbnails.Add(thumbnailView);
                }

                _thumbnails[i].ElementId = avatarId;
            }
        }

        protected virtual void OnElementSelected(string elementId)
        {
            ElementSelected?.Invoke(elementId);
        }

        protected virtual void OnElementDeleted(string elementId)
        {
            DeleteElement(elementId);
            RefreshThumbnails();
        }

        protected abstract void DeleteElement(string elementId);
    }
}
