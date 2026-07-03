using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class RoomScanGallerySpatialView : PopupBase
    {
        [SerializeField] private RoomScanGallery _localRoomScanGallery;
        [SerializeField] private RoomScanGallery _serverRoomScanGallery;
        [SerializeField] private CanvasGroup _dialogWindow;
        [SerializeField] private Button _btnClose;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override async void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.onClick.AddListener(Close);

            _localRoomScanGallery.ElementSelected += LocalRoomScanSelected;
            _serverRoomScanGallery.ElementSelected += ServerRoomScanSelected;

            _localRoomScanGallery.ElementIds = RootObject.Instance.RoomTwinLibraryManager.LibraryContent;
            _serverRoomScanGallery.ElementIds = new List<string>(await RootObject.Instance.RoomTwinManager.GetListOfRoomScansAsync());
        }

        public override void Close()
        {
            _localRoomScanGallery.ElementSelected -= LocalRoomScanSelected;
            _serverRoomScanGallery.ElementSelected -= ServerRoomScanSelected;

            base.Close();
        }

        private async void LocalRoomScanSelected(string elementId)
        {
            await LoadRoomScanAsync(elementId);
        }

        private async void ServerRoomScanSelected(string elementId)
        {
            RootObject.Instance.RoomTwinLibraryManager.Add(elementId);
            await LoadRoomScanAsync(elementId);
        }

        private async UniTask LoadRoomScanAsync(string elementId)
        {
            _dialogWindow.gameObject.SetActive(true);
            _dialogWindow.DOKill();
            await _dialogWindow.DOFade(1f, 0.3f).AsyncWaitForCompletion();
            _dialogWindow.interactable = true;
            _dialogWindow.blocksRaycasts = true;
            await RootObject.Instance.RoomTwinManager.LoadRoomTwinModelFromId(elementId);
            _dialogWindow.interactable = false;
            _dialogWindow.blocksRaycasts = false;
            await _dialogWindow.DOFade(0f, 0.4f).AsyncWaitForCompletion();
            _dialogWindow.gameObject.SetActive(false);
        }
    }
}
