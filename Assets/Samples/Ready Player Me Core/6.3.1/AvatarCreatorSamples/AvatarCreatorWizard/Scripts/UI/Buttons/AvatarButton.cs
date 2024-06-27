using System;
using System.Threading;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarCreator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.AvatarCreatorWizard
{
    public class AvatarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RawImage image;
        [SerializeField] private GameObject loading;
        [SerializeField] private GameObject buttonsPanel;
        [SerializeField] private Button customizeButton;
        [SerializeField] private Button selectButton;

        private RectTransform rawImageRectTransform;
        private string avatarId;
        private bool showButtons;

        private CancellationTokenSource ctxSource;

        private async void Start()
        {
            rawImageRectTransform = image.GetComponent<RectTransform>();
            while (string.IsNullOrEmpty(avatarId))
            {
                await Task.Yield();
            }
            LoadImage();
            AuthManager.OnSignedOut += () => ctxSource?.Cancel();
        }

        public void Init(string id, Action onCustomize, Action onSelect, bool isCurrentPartner)
        {
            avatarId = id;
            customizeButton.onClick.AddListener(() => onCustomize());
            selectButton.onClick.AddListener(() => onSelect());
            showButtons = isCurrentPartner;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showButtons)
            {
                buttonsPanel.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showButtons)
            {
                buttonsPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            ctxSource?.Cancel();
        }

        private async void LoadImage()
        {
            loading.SetActive(true);
            try
            {
                ctxSource = new CancellationTokenSource();
                var previousSize = rawImageRectTransform.sizeDelta;
                image.texture = await AvatarRenderHelper.GetPortrait(avatarId, ctxSource.Token);
                rawImageRectTransform.sizeDelta = previousSize;
            }
            catch (Exception)
            {
                // ignored
            }

            if (ctxSource.IsCancellationRequested)
            {
                return;
            }

            loading.SetActive(false);
        }
    }
}
