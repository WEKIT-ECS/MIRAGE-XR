using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelLibraryManager : MonoBehaviour
    {
        public const string LibraryKeyword = "library";

        [SerializeField] private GameObject[] items;
        [SerializeField] private LibraryObject[] objects;
        [SerializeField] private GameObject itemPrefab;

        private readonly List<GameObject> _instantiatedItems = new List<GameObject>();

        private ModelEditorView _modelEditorView;

        [SerializeField] private GameObject listOfLibrariesTab;
        [SerializeField] private GameObject libraryTab;
        [SerializeField] private Transform libraryContent;

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
            GenerateLibrary(category);
        }



        /// <summary>
        /// Enable the category buttons
        /// </summary>
        public void EnableCategoryButtons(ModelEditorView modelEditorView)
        {
            _modelEditorView = modelEditorView;
            
            DisableCategoryButtons();
            for (var i = 0; i < items.Length; i++)
            {
                var categoryButton = Instantiate(items[i], transform);
                categoryButton.TryGetComponent<Button>(out var button);

                if (button)
                {
                    var capturedIndex = i;  // Capture the loop variable's value
                    button.onClick.AddListener(() => OnItemClicked((ModelLibraryCategory)capturedIndex));
                }
            }
        }



        public void DisableCategoryButtons()
        {
            //destroy the existing items
            foreach (var child in GetComponentsInChildren<Button>())
            {
                Destroy(child.gameObject);
            }
            listOfLibrariesTab.SetActive(true);
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

            foreach (var obj in objects)
            {
                if (obj.category == selectedCategory)
                {
                    var item = Instantiate(itemPrefab, libraryContent);
                    item.TryGetComponent<ModelLibraryListItem>(out var libraryListItem);

                    if (libraryListItem)
                    {
                        libraryListItem.Title.text = " " + obj.label;
                        libraryListItem.Thumbnail.sprite = obj.sprite;
                       
                        var fbxFilePath = AssetDatabase.GetAssetPath(obj.model);
                        var fileInfo = new FileInfo(fbxFilePath);
                        var byteSize = fileInfo.Length;
                        var kilobyteSize = Math.Round(byteSize / 1024f , 1);
                        libraryListItem.TxtSize.text = " " + kilobyteSize.ToString(CultureInfo.InvariantCulture) + " Kb";
                        
                        libraryListItem.AddButtonListener(() => _modelEditorView.AddAugmentation(obj.prefabName, true));
                        _instantiatedItems.Add(item);
                    }
                }
            }
        }
    }
}