using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

using MirageXR;

namespace MirageXR.View
{
    public class ModelContentView : ContentView
    {
        public Guid Id => Content?.Id ?? Guid.Empty;
        private static ISketchfabManager sketchfabManager => RootObject.Instance.LEE.SketchfabManager;
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

            BoxCollider = gameObject.GetComponent<BoxCollider>();
            if (!BoxCollider)
            {
                BoxCollider = gameObject.AddComponent<BoxCollider>();
            }

            var bounds = GetModelRendererBounds();
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
            else
            {
               _model.UpdateView(newModelContent.ContentData.ResetPosition, newModelContent.ContentData.FitToScreen, newModelContent.ContentData.Scale);
               InitializeBoxCollider();
            }

            await base.OnContentUpdatedAsync(content);
        }

        private Bounds GetModelRendererBounds()
        {
            if (_model == null)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            var renderers = _model.GetComponentsInChildren<Renderer>();
            var first = true;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (var renderer in renderers)
            {
                if (ShouldSkipBoundsRenderer(renderer))
                {
                    continue;
                }

                if (TryGetRendererLocalBounds(renderer, out var rendererBounds))
                {
                    EncapsulateTransformedBounds(ref bounds, ref first, rendererBounds, renderer.transform.localToWorldMatrix);
                }
                else
                {
                    EncapsulateWorldBounds(ref bounds, ref first, renderer.bounds);
                }
            }

            return first ? new Bounds(Vector3.zero, Vector3.one) : bounds;
        }

        private static bool ShouldSkipBoundsRenderer(Renderer renderer)
        {
            if (renderer.GetComponent<LineRenderer>() != null)
            {
                return true;
            }
            if (renderer.GetComponent<ParticleSystem>() != null)
            {
                return true;
            }
            return renderer.GetComponentInParent<BoundingBoxHandle>() != null;
        }

        private static bool TryGetRendererLocalBounds(Renderer renderer, out Bounds bounds)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                bounds = skinnedMeshRenderer.localBounds;
                return true;
            }
            
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                bounds = meshFilter.sharedMesh.bounds;
                return true;
            }

            bounds = new Bounds();
            return false;
        }

        private void EncapsulateTransformedBounds(ref Bounds bounds, ref bool first, Bounds sourceBounds, Matrix4x4 localToWorld)
        {
            var min = sourceBounds.min;
            var max = sourceBounds.max;

            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z)));
            EncapsulatePoint(ref bounds, ref first, localToWorld.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z)));
        }

        private void EncapsulateWorldBounds(ref Bounds bounds, ref bool first, Bounds worldBounds)
        {
            var min = worldBounds.min;
            var max = worldBounds.max;

            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, min.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, min.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, max.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(min.x, max.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, min.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, min.y, max.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, max.y, min.z));
            EncapsulatePoint(ref bounds, ref first, new Vector3(max.x, max.y, max.z));
        }

        private void EncapsulatePoint(ref Bounds bounds, ref bool first, Vector3 worldPoint)
        {
            var localPoint = transform.InverseTransformPoint(worldPoint);
            if (first)
            {
                bounds = new Bounds(localPoint, Vector3.zero);
                first = false;
            }
            else
            {
                bounds.Encapsulate(localPoint);
            }
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

            // Capture initial local rotation (e.g. loader validation)
            Quaternion initialChildRotation = _model.transform.localRotation;
            
            // Add Custom Bounding Box
            var bb = _model.gameObject.AddComponent<SimpleBoundingBox>();
            bb.Target = _model.transform;
            bb.Setup(content.Id.ToString());

            bb.OnScaleEnded += scale =>
            {
                // We use the first component as uniform scale
                float newScale = scale.x;
                
                content.ContentData.Scale = newScale;
                content.ContentData.FitToScreen = false;
                content.ContentData.ResetPosition = false;

                sketchfabManager.Scale = newScale;
                sketchfabManager.FitToScreen = false;
                sketchfabManager.ResetPosition = false;

                InitializeBoxCollider();
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            };
            
            bb.OnRotationEnded += rotation =>
            {
                // Apply rotation to Parent (this object) s.t. Parent * InitialChild = TargetRotation
                // Parent = Target * Inverse(InitialChild)
                transform.rotation = rotation * Quaternion.Inverse(initialChildRotation);
                
                // Reset Child to its initial local rotation (preserving loader fix)
                _model.transform.localRotation = initialChildRotation;
                
                content.Location.Rotation = transform.rotation.eulerAngles;
                
                content.ContentData.FitToScreen = false;
                content.ContentData.ResetPosition = false;
                sketchfabManager.FitToScreen = false;
                sketchfabManager.ResetPosition = false;

                InitializeBoxCollider();
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            };

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
            
            // Capture initial
            Quaternion initialChildRotation = _model.transform.localRotation;
            
            // Add Custom Bounding Box
            var bb = item.AddComponent<SimpleBoundingBox>();
            bb.Target = item.transform;
            bb.Setup(content.Id.ToString());
            bb.OnScaleEnded += scale =>
            {
                // We use the first component as uniform scale
                float newScale = scale.x;
                
                content.ContentData.Scale = newScale;
                content.ContentData.FitToScreen = false;
                content.ContentData.ResetPosition = false;

                sketchfabManager.Scale = newScale;
                sketchfabManager.FitToScreen = false;
                sketchfabManager.ResetPosition = false;

                InitializeBoxCollider();
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            };

            bb.OnRotationEnded += rotation =>
            {
                transform.rotation = rotation * Quaternion.Inverse(initialChildRotation);
                item.transform.localRotation = initialChildRotation;

                content.Location.Rotation = transform.rotation.eulerAngles;
                
                content.ContentData.FitToScreen = false;
                content.ContentData.ResetPosition = false;
                sketchfabManager.FitToScreen = false;
                sketchfabManager.ResetPosition = false;

                InitializeBoxCollider();
                RootObject.Instance.LEE.ContentManager.UpdateContent(content);
            };

            return true;
        }
    }
}
