using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelManager : MonoBehaviour
    {
        private const int ITEMS_PER_PAGE = 10;

        private int _currentSearchPage = 0;

        private readonly List<ModelPreviewItem> _localModels = new List<ModelPreviewItem>();
        private List<ModelPreviewItem> _currentModels = new List<ModelPreviewItem>();

        [SerializeField]
        [Tooltip("Indeterminate loader to hide / show for LazyLoad")]
        private GameObject loader;

        [SerializeField] private Text ResultsShownText;

        public void IncrementPage()
        {
            var maxPage = _localModels.Count / ITEMS_PER_PAGE - 1;
            if (_localModels.Count % ITEMS_PER_PAGE > 0)
            {
                maxPage++;
            }

            if (_currentSearchPage < maxPage)
            {
                _currentSearchPage++;
                PopulateModelPreview();
            }
        }

        /// <summary>
        /// Selects the previous page of models (default page size is 10).
        /// </summary>
        public void DecrementPage()
        {
            var minPage = 0;

            if (_currentSearchPage > minPage)
            {
                _currentSearchPage--;
                PopulateModelPreview();
            }
        }

        public void PopulateModelPreview()
        {
            _localModels.Clear();

            // find all local models
            var modelsFolderPath = Path.Combine(Application.persistentDataPath, "models");
            if (!Directory.Exists(modelsFolderPath))
            {
                Debug.Log($"creating model folder at {modelsFolderPath}");
                Directory.CreateDirectory(modelsFolderPath);
            }

            var localModelDirs = Directory.GetDirectories(modelsFolderPath, "*", SearchOption.TopDirectoryOnly);
            var localModelZips = Directory.GetFiles(modelsFolderPath, "*.zip", SearchOption.TopDirectoryOnly);

            Debug.Log($"Found {localModelZips.Length} zipped models in the folder.");

            List<string> arrangedModelItems = new List<string>();

            foreach (string dir in localModelDirs)
            {
                string[] prevInfo = Directory.GetFiles(dir, "previewInfo.json", SearchOption.AllDirectories);

                if (prevInfo.Length > 0)
                {
                    string json = File.ReadAllText(prevInfo[0]);
                    ModelPreviewItem mpi = JsonUtility.FromJson<ModelPreviewItem>(json);
                    _localModels.Add(mpi);
                }
                else
                {
                    ModelPreviewItem mpiBuilt = BuildLocalModelPreview(dir);
                    SavePreviewInfo(mpiBuilt, dir);
                    _localModels.Add(mpiBuilt);
                }

                arrangedModelItems.Add(Path.GetFileNameWithoutExtension(dir));
            }

            foreach (string zipFile in localModelZips)
            {
                string zipFileName = Path.GetFileNameWithoutExtension(zipFile);

                if (!arrangedModelItems.Contains(zipFileName))
                {
                    ModelPreviewItem mpiFromZip = BuildModelPreviewFromZip(modelsFolderPath, zipFileName);
                    _localModels.Add(mpiFromZip);
                }
            }

            _currentModels = GetLocalItems(_currentSearchPage);

            var listPopulator = GetComponent<ScrollableListPopulator>();
            listPopulator.ClearPreviousSearches(true);
            listPopulator.MakeScrollingList(_currentModels, true);
        }

        /// <summary>
        /// Function to take a ${ITEMS_PER_PAGE}-model slice of the current local models for results paging
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private List<ModelPreviewItem> GetLocalItems(int page = 0)
        {
            var minPage = 0;
            var maxPage = _localModels.Count / ITEMS_PER_PAGE - 1;
            if (_localModels.Count % ITEMS_PER_PAGE > 0)
            {
                maxPage++;
            }

            page = Mathf.Clamp(page, minPage, maxPage);

            var startIndex = page * ITEMS_PER_PAGE;
            var listSize = Math.Min(ITEMS_PER_PAGE, _localModels.Count - startIndex);
            var endIndex = startIndex + listSize;

            ResultsShownText.text = $"{startIndex + 1} - {endIndex}";

            return _localModels.GetRange(startIndex, listSize);
        }

        private ModelPreviewItem BuildLocalModelPreview(string modelDirectory)
        {
            Debug.Log("building preview item for: " + modelDirectory);

            // prepare json object
            string modelUrl = GetModelUrl(modelDirectory);
            string modelName = Path.GetFileName(modelDirectory);
            ThumbnailImage modelImage = GetThumbnail(modelDirectory);

            // send modelPreviewItem
            ModelPreviewItem item = new ModelPreviewItem
            {
                name = modelName,
                description = modelName,
                resourceUrl = "file://" + modelUrl,
                resourceImage = modelImage
            };

            return item;
        }

        private ModelPreviewItem BuildModelPreviewFromZip(string modelDirectory, string fileName)
        {
            string modelName = Path.GetFileNameWithoutExtension(fileName);
            string modelUrl = Path.Combine(modelDirectory, fileName);

            ThumbnailImage modelImage = new ThumbnailImage
            {
                height = 2,
                width = 3,
                uid = "",
                url = ""
            };

            // send modelPreviewItem
            ModelPreviewItem item = new ModelPreviewItem
            {
                name = Path.GetFileNameWithoutExtension(fileName),
                description = "",
                resourceUrl = "file://" + modelUrl,
                resourceImage = modelImage
            };

            return item;
        }

        private string GetModelUrl(string modelDirectory)
        {
            string[] modelsInDir = Directory.GetFiles(modelDirectory, "*.gltf", SearchOption.TopDirectoryOnly);

            if (modelsInDir.Length > 1)
            {
                Debug.Log("warning: multiple models in root folder, using 'scene.gltf' or the first one.");
                if (string.Join(",", modelsInDir).Contains("scene.gltf"))
                {
                    return Path.Combine(modelDirectory, "scene.gltf");
                }

                return Path.Combine(modelDirectory, modelsInDir[0]);
            }

            if (modelsInDir.Length > 0)
            {
                return Path.Combine(modelDirectory, modelsInDir[0]);
            }

            Debug.Log("model not found");
            return null;
        }

        private ThumbnailImage GetThumbnail(string modelDirectory)
        {
            string[] imagesInDirPng = Directory.GetFiles(modelDirectory, "*.png", SearchOption.TopDirectoryOnly);
            string[] imagesInDirJpg = Directory.GetFiles(modelDirectory, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] imagesInDir = new string[imagesInDirPng.Length + imagesInDirJpg.Length];
            imagesInDirPng.CopyTo(imagesInDir, 0);
            imagesInDirJpg.CopyTo(imagesInDir, imagesInDirPng.Length);

            string imageUrl = "";

            if (imagesInDir.Length > 0)
            {
                int bestFitImageIndex = IndexOfClosestImageName(imagesInDir, modelDirectory);
                imageUrl = Path.Combine(modelDirectory, imagesInDir[bestFitImageIndex]);
            }

            if (File.Exists(imageUrl))
            {
                ThumbnailImage thumb = new ThumbnailImage();

                byte[] imageBytes = File.ReadAllBytes(imageUrl);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(imageBytes);

                thumb.uid = tex.name;
                thumb.url = "file://" + imageUrl;
                thumb.width = tex.width;
                thumb.height = tex.height;
                return thumb;
            }

            Debug.Log("no thumbnails found");
            return null;
        }

        /// <summary>
        /// Select the image file name that most closely matches the name of the model,
        /// first prioritising where the model name is contained in the string or, second,
        /// using the Levenshtein Distance algorithm.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="target"></param>
        /// <returns>The index of the 'options' array where the closest match is found.</returns>
        private int IndexOfClosestImageName(string[] options, string target)
        {
            // if list has only one item, return index 0
            if (options.Length == 1)
            {
                return 0;
            }

            int bestFitIndex = 0;
            int levenDistMin = 100;
            foreach (string s in options)
            {
                if (target.Contains(s) || s.Contains(target))
                {
                    bestFitIndex = Array.IndexOf(options, s);
                }
                else
                {
                    int newLevenDist = LevenshteinDistance.Compute(s, target);
                    Debug.Log($"Comparing:{s} and {target}. \nLevenshtein Distance: {newLevenDist}");

                    if (newLevenDist < levenDistMin)
                    {
                        levenDistMin = newLevenDist;
                        bestFitIndex = Array.IndexOf(options, s);
                    }
                }
            }

            return bestFitIndex;
        }

        public async void DownloadModel(string url, ModelPreviewItem modelPreview)
        {
            OnDownloadBegin();
            await Sketchfab.DownloadModelAndExtractAsync(url, modelPreview, OnDownload);
            OnDownloadEnd(modelPreview);
        }

        public void OnDownloadBegin()
        {
            SetLoaderFillAmount(0, 100);
            loader.SetActive(true);
        }

        public void OnDownload(float progress)
        {
            SetLoaderFillAmount(progress * 100, 100);
        }

        public void OnDownloadEnd(ModelPreviewItem modelPreview)
        {
            loader.SetActive(false);
            GetComponent<ScrollableListPopulator>().UpdatePreviewHandler(modelPreview);
        }

        /// <summary>
        /// Writes json object preview information to a file in the model's folder, including a reference to the thumbnail, whose url should begin with either 'http' or 'file'.
        /// </summary>
        /// <param name="previewItem"></param>
        /// <param name="modelFolder"></param>
        private static void SavePreviewInfo(ModelPreviewItem previewItem, string modelFolder)
        {
            var output = Newtonsoft.Json.JsonConvert.SerializeObject(previewItem, Newtonsoft.Json.Formatting.Indented);
            var modelPath = Path.Combine(modelFolder, "previewInfo.json");

            using (var fs = new FileStream(modelPath, FileMode.OpenOrCreate))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(output);
                }
            }
        }

        public async void LoadModel(ButtonHandler handler, ModelPreviewItem modelPreview)
        {
            await Sketchfab.LoadModelAsync(modelPreview.name);
            GetComponent<ModelEditor>().Create(modelPreview.name);
        }

        private void SetLoaderFillAmount(float numberItemsLoaded, int numberPerPage)
        {
            if (loader.activeInHierarchy)
            {
                // calculate value (0-1) for amount loaded
                float x = numberItemsLoaded / (float)numberPerPage;

                var loaderImage = loader.GetComponent<UnityEngine.UI.Image>();
                loaderImage.fillAmount = x;
            }
        }
    }
}