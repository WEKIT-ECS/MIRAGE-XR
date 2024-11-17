using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace MirageXR.View
{
    public class ModelContentView : ContentView
    {
        private GltfModelController _model;

        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);

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

        protected override void InitializeBoxCollider()
        {
            var bounds = BoundsUtilities.GetTargetBounds(gameObject);
            BoxCollider = gameObject.GetComponent<BoxCollider>();
            if (BoxCollider == null)
            {
                BoxCollider = gameObject.AddComponent<BoxCollider>();
            }
            BoxCollider.size = bounds.size;
            BoxCollider.center = bounds.center;
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
                return;
            }

            _model.UpdateView(content.ContentData.ResetPosition, content.ContentData.FitToScreen, content.ContentData.Scale);
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
            var item = Instantiate(prefab, transform);
            _model = item.AddComponent<GltfModelController>();
        }
    }
}