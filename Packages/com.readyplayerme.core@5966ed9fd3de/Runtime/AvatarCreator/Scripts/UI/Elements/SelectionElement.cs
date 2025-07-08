using System;
using System.Collections.Generic;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// Base class for managing selection elements in a UI, responsible for creating and managing buttons based on asset data.
    /// It provides functionalities to create buttons dynamically, handle their selection, and maintain a reference to them.
    /// </summary>
    public abstract class SelectionElement : MonoBehaviour
    {
        private const string TAG = nameof(TemplateSelectionElement);
        [Header("UI Elements")]
        [Space(5)]
        [SerializeField] private SelectionButton buttonElementPrefab;
        [SerializeField] private GameObject selectedIconPrefab;
        [SerializeField] private Transform buttonContainer;

        private GameObject _selectedIcon;
        private GameObject selectedIcon
        {
            get
            {
                if (_selectedIcon)
                {
                    return _selectedIcon;
                }
                _selectedIcon = Instantiate(selectedIconPrefab, buttonContainer);
                selectedIcon.SetActive(false);
                return _selectedIcon;
            }
        }
        

        [Space(5)]
        [Header("Events")]
        public UnityEvent<IAssetData> OnAssetSelected;
        private readonly Dictionary<string, SelectionButton> buttonElementById = new Dictionary<string, SelectionButton>();

        protected Transform ButtonContainer => buttonContainer;

        /// <summary>
        /// Creates button elements for each asset in the provided array.
        /// This function is generic and can work with any asset type that implements the IAssetData interface.
        /// </summary>
        /// <param name="assets">An array of assets of type T, where T implements the IAssetData interface. </param>
        /// <param name="onButtonCreated">An optional callback action that is invoked for each button created, passing the created ButtonElement and the corresponding asset as arguments.
        /// This can be used for additional setup or customization of the buttons.</param>
        /// <typeparam name="T">A generic type of the asset data. Must implement the IAssetData interface.</typeparam>
        public void CreateButtons<T>(T[] assets, Action<SelectionButton, T> onButtonCreated = default) where T : IAssetData
        {
            if (assets.Length == 0)
            {
                SDKLogger.LogWarning(TAG, "No assets provided.");
                return;
            }
            ClearButtons();
            for (var i = 0; i < assets.Length; i++)
            {
                var button = CreateButton(assets[i].Id);
                var asset = assets[i];
                button.AddListener(() => OnAssetSelected?.Invoke(asset));
                onButtonCreated?.Invoke(button, asset);
            }
        }

        /// <summary>
        /// Instantiates a new button element with a specified ID and adds it to the buttonElementById dictionary.
        /// </summary>
        /// <param name="id">The unique identifier for the button.</param>
        /// <returns>The created ButtonElement instance.</returns>
        public SelectionButton CreateButton(string id)
        {
            var button = Instantiate(buttonElementPrefab, buttonContainer);
            button.name = id;
            buttonElementById.Add(id, button);
            button.AddListener(() => SetButtonSelected(button.transform));
            return button;
        }

        public void AddClearButton(SelectionButton button, AssetType assetType, bool setAsFirstChild = true)
        {
            var clearButton = Instantiate(button, ButtonContainer);
            if (setAsFirstChild)
            {
                clearButton.transform.SetAsFirstSibling();
            }
            var assetData = new PartnerAsset { Id = "", AssetType = assetType };
            clearButton.AddListener(() => OnAssetSelected?.Invoke(assetData));
            clearButton.AddListener(() => SetButtonSelected(clearButton.transform));
        }

        /// <summary>
        /// Clears all button elements from the UI and the buttonElementById dictionary.
        /// </summary>
        public void ClearButtons()
        {
            ResetSelectIcon();
            foreach (var button in buttonElementById)
            {
                Destroy(button.Value.gameObject);
            }
            buttonElementById.Clear();
        }

        private void ResetSelectIcon()
        {
            selectedIcon.transform.SetParent(buttonContainer);
            selectedIcon.SetActive(false);
        }

        /// <summary>
        /// Retrieves a button element by its unique ID. Useful for accessing specific buttons for updates or manipulation.
        /// </summary>
        /// <param name="id">The unique ID associated with the button.</param>
        /// <returns>A ButtonElement with the specified ID if found; otherwise, null.</returns>
        public SelectionButton GetButton(string id)
        {
            if (buttonElementById.TryGetValue(id, out SelectionButton value))
            {
                return value;
            }
            Debug.LogWarning($"No button found with id {id}");
            return null;
        }

        /// <summary>
        /// Sets the position and parent of the SelectedIcon to indicate which button was last selected.
        /// </summary>
        /// <param name="button"></param>
        private void SetButtonSelected(Transform button)
        {
            selectedIcon.transform.SetParent(button);
            selectedIcon.transform.localPosition = Vector3.zero;
            var rect = selectedIcon.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
            selectedIcon.SetActive(true);
        }

        protected void SetButtonSelected(string assetId)
        {
            var button = GetButton(assetId);
            if (button == null)
            {
                Debug.Log($"No button found with id {assetId}");
                return;
            }
            Debug.Log($"Set {button.name} selected");
            SetButtonSelected(button.transform);
        }
    }
}
