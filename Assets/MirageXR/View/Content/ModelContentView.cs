using System.Threading;
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
        private CancellationToken _cancellationToken;
            
        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);
            _cancellationToken = transform.GetCancellationTokenOnDestroy();

            if (content is Content<ModelContentData> modeContent)
            {
                Initialized = await InitializeContentAsync(modeContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        private async UniTask<bool> InitializeContentAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.IsLibraryModel)
            {
                return await InitializeLibraryModelAsync(content);
            }

            return await InitializeModelAsync(content);
        }

        protected override void InitializeBoxCollider()
        {
            if (!Initialized)
            {
                return;
            }

            var bounds = BoundsUtilities.GetTargetBounds(gameObject);
            BoxCollider = gameObject.GetComponent<BoxCollider>();
            if (!BoxCollider)
            {
                BoxCollider = gameObject.AddComponent<BoxCollider>();
            }
            BoxCollider.size = bounds.size;
            BoxCollider.center = bounds.center;
        }

        protected override async UniTask OnContentUpdatedAsync(Content content)
        {
            if (content is not Content<ModelContentData> newModelContent || Content is not Content<ModelContentData> oldModelContent)
            {
                return;
            }

            if (newModelContent.ContentData.ModelUid != oldModelContent.ContentData.ModelUid)
            {
                Destroy(_model.gameObject);
                Initialized = false;
                Initialized = await InitializeContentAsync(newModelContent);
                if (Initialized)
                {
                    InitializeBoxCollider();
                }
            }

            await base.OnContentUpdatedAsync(content);
        }

        public override async UniTask PlayAsync()
        {
            await base.PlayAsync();

            var animationClips = _model.AnimationClips;
            if (animationClips is { Length: > 0 })
            {
                _model.PlayAnimationClip(animationClips[0], WrapMode.Loop);
            }
        }

        private async UniTask<bool> InitializeModelAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.ModelUid == null)
            {
                AppLog.LogError("ModelContentData.Model is null");
                return false;
            }

            var activityId = RootObject.Instance.ViewManager.ActivityView.ActivityId;
            var sketchfabManager = RootObject.Instance.LEE.SketchfabManager;
            if (!sketchfabManager.IsModelCached(content.ContentData.ModelUid))
            {
                var result = await sketchfabManager.TryCacheModelFromServerUntilSuccessAsync(activityId, content.ContentData.ModelUid, _cancellationToken);
                if (!result)
                {
                    AppLog.LogError($"model {content.ContentData.ModelUid} doesn't cached");
                    return false;
                }
            }
            _model = await sketchfabManager.LoadCachedModelAsync(content.ContentData.ModelUid, transform, _cancellationToken);

            if (_cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            if (_model is null)
            {
                AppLog.LogError($"Can't load model with id {content.ContentData.ModelUid}");
                return false;
            }

            _model.UpdateView(content.ContentData.ResetPosition, content.ContentData.FitToScreen, content.ContentData.Scale);
            return true;
        }

        private async UniTask<bool> InitializeLibraryModelAsync(Content<ModelContentData> content)
        {
            if (content.ContentData.LibraryModel == null)
            {
                AppLog.LogError("ModelContentData.LibraryModel is null");
                return false;
            }

            var prefabName = $"Library/{content.ContentData.LibraryModel.Catalog}/{content.ContentData.LibraryModel.ModelName}";
            var prefab = await Addressables.LoadAssetAsync<GameObject>(prefabName).Task;
            var item = Instantiate(prefab, transform);
            _model = item.AddComponent<GltfModelController>();
            return true;
        }
    }
}