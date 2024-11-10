using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MirageXR.View
{
    public class ModelContentView : ContentView
    {
        private GameObject _model;
        
        public override async UniTask InitializeAsync(Content content)
        {
            base.InitializeAsync(content);

            if (content is Content<ModelContentData> modeContent)
            {
                await InitializeContentAsync(modeContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        private async UniTask InitializeContentAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.IsLibraryModel)
            {
                await InitializeLibraryModelAsync(content);
            }
            else
            {
                await InitializeModelAsync(content);
            }
        }

        private async UniTask InitializeModelAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.ModelUid == null)
            {
                AppLog.LogError("ModelContentData.Model is null");
                return;
            }

            var sketchfabManager = RootObject.Instance.LEE.SketchfabManager;
            if (!sketchfabManager.IsModelCached(content.ContentData.ModelUid))
            {
                AppLog.LogError($"model {content.ContentData.ModelUid} doesn't cached");
                return;
            }
            _model = await sketchfabManager.LoadCachedModelAsync(content.ContentData.ModelUid, transform);

            if (_model is null)
            {
                AppLog.LogError($"Can't load model with id {content.ContentData.ModelUid}");
            }
        }

        private async UniTask InitializeLibraryModelAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.LibraryModel == null)
            {
                AppLog.LogError("ModelContentData.LibraryModel is null");
                return;
            }

            var prefabName = $"Library/{content.ContentData.LibraryModel.Catalog}/{content.ContentData.LibraryModel.ModelName}";
            var prefab = await Addressables.LoadAssetAsync<GameObject>(prefabName).Task;
            _model = Instantiate(prefab, transform);
        }
    }
}