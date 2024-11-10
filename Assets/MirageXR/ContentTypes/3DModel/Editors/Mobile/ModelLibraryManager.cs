using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelLibraryManager : MonoBehaviour
    {
        public const string LibraryKeyword = "library";

        [SerializeField] private GameObject[] items;
        [SerializeField] private LibraryObject[] objects;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject listOfLibrariesTab;
        [SerializeField] private GameObject libraryTab;
        [SerializeField] private Transform libraryContent;
        [SerializeField] private Transform listOfLibraryContent;
        [SerializeField] private TMP_Text _topLabel;
        [SerializeField] private TMP_InputField _inputSearch;

        private readonly List<GameObject> _instantiatedItems = new();
        private readonly List<ModelLibraryListItem> _currentLibraryItems = new();
        private GameObject _emptyItem;

        private UnityAction<string> _onListItemClickedAction;

        public enum ModelLibraryCategory
        {
            Foods = 0,
            Tools = 1
        }

        public void OnItemClicked(ModelLibraryCategory category)
        {
            DisableCategoryButtons();
            listOfLibrariesTab.SetActive(false);
            libraryTab.SetActive(true);
            _topLabel.text = category.ToString();
            GenerateLibrary(category);
        }
        
        public void OnStartSearch()
        {
            SearchLocal();
        }
        
        public void ClearSearchField()
        {
            _inputSearch.text = "";
        }

        private void SearchLocal()
        {
            foreach (var item in _instantiatedItems)
            {
                item.TryGetComponent<ModelLibraryListItem>(out var libraryListItem);
                var active = string.IsNullOrEmpty(_inputSearch.text) || libraryListItem.Title.text.ToLower().Contains(_inputSearch.text.ToLower());
                item.SetActive(active);
            }
        }

        private void OnInputFieldSearchChanged(string text)
        {
            SearchLocal();
        }

        /// <summary>
        /// Enable the category buttons
        /// </summary>
        public void EnableCategoryButtons(UnityAction<string> onListItemClickedAction)
        {
            _onListItemClickedAction = onListItemClickedAction;
            _inputSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);
            
            for (var i = 0; i < items.Length; i++)
            {
                var categoryButton = Instantiate(items[i], listOfLibraryContent);
                categoryButton.TryGetComponent<Button>(out var button);

                if (button)
                {
                    var capturedIndex = i;  // Capture the loop variable's value
                    button.onClick.AddListener(() => OnItemClicked((ModelLibraryCategory)capturedIndex));
                }
            }
        }

        private void DisableCategoryButtons()
        {
            //destroy the existing items
            foreach (var child in GetComponentsInChildren<Button>())
            {
                Destroy(child.gameObject);
            }
            listOfLibrariesTab.SetActive(true);
            libraryTab.SetActive(false);
        }

        public void CloseLibraryTab()
        {
            libraryTab.SetActive(false);
        }
        
        private void GenerateLibrary(ModelLibraryCategory selectedCategory)
        {
            foreach (var item in _instantiatedItems)
            {
                if(item)
                {
                    Destroy(item);
                }
            }
            _instantiatedItems.Clear();
            if (_emptyItem)
            {
                Destroy(_emptyItem);
            }

            _inputSearch.text = "";

            foreach (var obj in objects)
            {
                if (obj.category == selectedCategory)
                {
                    var item = Instantiate(itemPrefab, libraryContent);
                    item.TryGetComponent<ModelLibraryListItem>(out var libraryListItem);

                    if (libraryListItem)
                    {
                        libraryListItem.Title.text = "  " + obj.label;
                        libraryListItem.Thumbnail.sprite = obj.sprite;

                        // TODO: get fbx file size
                        libraryListItem.TxtSize.text = "   "; // + kilobyteSize.ToString(CultureInfo.InvariantCulture) + " Kb";
                        
                        libraryListItem.AddButtonListener(() => _onListItemClickedAction(obj.prefabName));
                        _instantiatedItems.Add(item);
                        _currentLibraryItems.Add(libraryListItem);
                    }
                }
            }
            // Empty element added for left alignment in case only 1 element is active.
            _emptyItem = Instantiate(itemPrefab, libraryContent);
            foreach (Transform child in _emptyItem.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}