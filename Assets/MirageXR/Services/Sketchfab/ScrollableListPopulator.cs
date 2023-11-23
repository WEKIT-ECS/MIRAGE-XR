using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    [AddComponentMenu("MirageXR/ScrollableListPopulator")]
    public class ScrollableListPopulator : MonoBehaviour
    {
        [Tooltip("The ScrollingObjectCollection to populate.")]
        [SerializeField] private ScrollingObjectCollection webScrollView;
        [SerializeField] private GridObjectCollection webGridObjectCollection;

        [Tooltip("The ScrollingObjectCollection to populate.")]
        [SerializeField] private ScrollingObjectCollection localScrollView;

        [SerializeField] private GridObjectCollection localGridObjectCollection;

        [Tooltip("Object to duplicate in ScrollCollection")]
        [SerializeField] private GameObject dynamicItem;

        [Tooltip("Indeterminate loader to hide / show for LazyLoad")]
        [SerializeField] private GameObject loader;

        private readonly List<GameObject> currentWebPreviewObjects = new List<GameObject>();
        private List<GameObject> currentLocalPreviewObjects = new List<GameObject>();

        public List<ModelPreviewItem> currentLocalPreviewList = new List<ModelPreviewItem>();
        private readonly List<Sprite> spritesInUse = new List<Sprite>();

        private int numItems;

        public void ClearPreviousSearches(bool isLocal)
        {
            if (!isLocal)
            {
                for (int ch = 0; ch < webGridObjectCollection.transform.childCount; ch++)
                {
                    Destroy(webGridObjectCollection.transform.GetChild(ch).gameObject);
                }
                webScrollView.UpdateContent();
                webGridObjectCollection.UpdateCollection();

                spritesInUse.Clear();
                currentWebPreviewObjects.Clear();
            }
            else
            {
                for (int ch = 0; ch < localGridObjectCollection.transform.childCount; ch++)
                {
                    Destroy(localGridObjectCollection.transform.GetChild(ch).gameObject);
                }
                localScrollView.UpdateContent();
                localGridObjectCollection.UpdateCollection();
            }
        }

        /// <summary>
        /// Entry point to build scrollable lists.
        /// </summary>
        /// <param name="itemList">A list of type ModelPreviewItem.</param>
        /// <param name="isLocal">Whether the file is for local models or sketchfab.</param>
        public void MakeScrollingList(List<ModelPreviewItem> itemList, bool isLocal)
        {
            numItems = itemList.Count;
            if (loader != null)
            {
                SetLoaderFillAmount(0, numItems);
                loader.SetActive(true);
            }

            if (isLocal)
            {
                currentLocalPreviewList = itemList;
            }

            StartCoroutine(UpdateListOverTime(loader, numItems, itemList, isLocal));
        }

        private IEnumerator UpdateListOverTime(GameObject loaderImage, int pageSize, List<ModelPreviewItem> previewItems, bool isLocal)
        {
            int loadingFrame = 0;

            loadingFrame++;
            for (int i = 0; i < numItems; i++)
            {
                if (i < previewItems.Count)
                {
                    yield return MakeItem(dynamicItem, previewItems[i], isLocal);
                }

                SetLoaderFillAmount(i + 1, pageSize);
                yield return new WaitForSeconds(0.1f);
            }

            // Now that the list is populated, hide the loader and show the list
            if (loaderImage != null)
            {
                loaderImage.SetActive(false);
            }

            // Finally, update objects to set up the collection
            if (!isLocal)
            {
                webGridObjectCollection.UpdateCollection();
                webScrollView.gameObject.SetActive(true);
                webScrollView.UpdateContent();
            }
            else
            {
                localGridObjectCollection.UpdateCollection();
                localScrollView.gameObject.SetActive(true);
                localScrollView.UpdateContent();
            }
        }

        private IEnumerator MakeItem(GameObject item, ModelPreviewItem previewItem, bool isLocalFile)
        {
            GameObject itemInstance;

            if (isLocalFile)
            {
                itemInstance = Instantiate(item, localGridObjectCollection.transform);
                currentLocalPreviewObjects.Add(itemInstance);
                localGridObjectCollection.UpdateCollection();
            }
            else
            {
                itemInstance = Instantiate(item, webGridObjectCollection.transform);
                currentWebPreviewObjects.Add(itemInstance);
                webGridObjectCollection.UpdateCollection();

                // check if item is already in local storage and create ui element with local urls.
                foreach (ModelPreviewItem mpi in currentLocalPreviewList)
                {
                    if (previewItem.uid == mpi.uid)
                    {
                        previewItem.resourceUrl = mpi.resourceUrl;
                        previewItem.resourceImage = mpi.resourceImage;
                        isLocalFile = true;
                        break;
                    }
                }
            }

            // set gameobject properties
            itemInstance.name = previewItem.name;
            var nameComp = itemInstance.GetComponentInChildren<TextMesh>();
            nameComp.text = previewItem.name;

            // set item-level data
            var bH = itemInstance.GetComponent<ButtonHandler>();
            bH.PreviewItem = previewItem;
            bH.IsLocalObject = isLocalFile;

            // register events
            bH.OnItemDownloadClick += GetComponent<SketchfabManager>().BeginModelDownload;
            bH.OnItemLoadClick += GetComponent<ModelManager>().LoadModel;

            // retrieve and set thumbnail image
            var previewImage = itemInstance.GetComponentInChildren<SpriteRenderer>();
            yield return SketchfabManager.DownloadImage(previewItem.resourceImage.url, sprite =>
            {
                previewImage.sprite = sprite;
                spritesInUse.Add(sprite);
            });

            itemInstance.SetActive(true);
        }

        public void SetWebButtonTextures()
        {
            foreach (var item in currentWebPreviewObjects)
            {
                item.GetComponentInChildren<SpriteRenderer>().sprite = spritesInUse[currentWebPreviewObjects.IndexOf(item)];
            }
        }

        public void UpdatePreviewHandler(ModelPreviewItem modelPreview)
        {
            foreach (var item in currentWebPreviewObjects)
            {
                var handler = item.GetComponent<ButtonHandler>();
                if (handler.PreviewItem.uid == modelPreview.uid)
                {
                    handler.PreviewItem = modelPreview;
                    handler.IsLocalObject = true;
                }
            }
        }

        private void SetLoaderFillAmount(float numberItemsLoaded, int numberPerPage)
        {
            if (loader.activeInHierarchy)
            {
                // calculate value (0-1) for amount loaded
                float x = numberItemsLoaded / numberPerPage;

                var loaderImage = loader.GetComponent<UnityEngine.UI.Image>();
                loaderImage.fillAmount = x;
            }
        }
    }
}