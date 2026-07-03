using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class GallerySelectionElement : MonoBehaviour
    {
        [SerializeField] private GameObject _addPlus;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private string _noElementSelectedText = "Select element";
        [SerializeField] private string _elementSelectedText = "Replace element";

        public ThumbnailView Thumbnail { get; private set; }

        public Action GalleryElementSelectionStarted;

        private void Awake()
        {
            Thumbnail = GetComponent<ThumbnailView>();
        }

        private void OnEnable()
        {
            Thumbnail.ElementIdChanged += OnElementIdChanged;
            Thumbnail.ElementSelected += OnThumbnailClicked;
            UpdateView();
        }

        private void OnDisable()
        {
            Thumbnail.ElementIdChanged -= OnElementIdChanged;
            Thumbnail.ElementSelected -= OnThumbnailClicked;
        }

        private void OnThumbnailClicked(string characterModelId)
        {
            StartGalleryElementSelection();
        }


        private void OnElementIdChanged(string characterId)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            bool characterModelSelected = !string.IsNullOrEmpty(Thumbnail.ElementId);
            _addPlus.SetActive(!characterModelSelected);
            if (characterModelSelected)
            {
                _infoText.text = _elementSelectedText;
            }
            else
            {
                _infoText.text = _noElementSelectedText;
            }
        }

        public void StartGalleryElementSelection()
        {
            GalleryElementSelectionStarted?.Invoke();
        }
    }
}
