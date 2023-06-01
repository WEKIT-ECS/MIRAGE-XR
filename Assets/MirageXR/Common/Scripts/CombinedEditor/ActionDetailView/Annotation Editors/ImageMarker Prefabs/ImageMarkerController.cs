using i5.Toolkit.Core.VerboseLogging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class ImageMarkerController : MirageXRPrefab
    {
        private ImageTargetManagerWrapper imageTargetManager => RootObject.Instance.imageTargetManager;

        private string _imageName;
        private ToggleObject _content;
        public IImageTarget target;
        private bool _clearAll;

        public override bool Init(ToggleObject content)
        {
            _clearAll = false;
            _content = content;
            InitAsync().AsAsyncVoid();
            return true;
        }

        private async Task<bool> InitAsync()
        {
            if (string.IsNullOrEmpty(_content.url))
            {
                AppLog.LogError("Content URL not provided.");
                return false;
            }

            if (!SetParent(_content))
            {
                AppLog.LogError("Couldn't set the parent.");
                return false;
            }

            var id = _content.url.Split('/').Last();

            name = $"{_content.predicate}_{id}";
            _imageName = _content.url.StartsWith("resources://") ? _content.url.Replace("resources://", string.Empty) : _content.url;

            var myPoiEditor = transform.parent.GetComponent<PoiEditor>();
            var defaultScale = new Vector3(0.5f, 0.5f, 0.5f);
            transform.parent.localScale = GetPoiScale(myPoiEditor, defaultScale);

            var imageTarget = imageTargetManager.GetImageTarget(_imageName) as ImageTargetBase;

            if (!imageTarget)
            {
                imageTarget = await LoadImage();

                if (imageTarget == null)
                {
                    AppLog.LogError("Can't add image target");
                    return false;
                }
            }

            MoveDetectableToImage(imageTarget.transform);

            return base.Init(_content);
        }

        private async Task<ImageTargetBase> LoadImage()
        {
            var imagePath = Path.Combine(RootObject.Instance.activityManager.ActivityPath, _imageName);
            var byteArray = await File.ReadAllBytesAsync(imagePath);
            var texture = new Texture2D(2, 2);

            if (!texture.LoadImage(byteArray))
            {
                AppLog.LogError($"Can't load image. path: {imagePath}");
                return null;
            }

            var model = new ImageTargetModel
            {
                name = _imageName,
                prefab = null,
                width = 0.5f,
                texture2D = texture,
                useLimitedTracking = true,
            };

            target = await RootObject.Instance.imageTargetManager.AddImageTarget(model);

            return target as ImageTargetBase;
        }

        private void MoveDetectableToImage(Transform targetHolder)
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            var taskStationId = workplaceManager.GetPlaceFromTaskStationId(_content.id);
            var detectable = workplaceManager.GetDetectable(taskStationId);
            var detectableObj = GameObject.Find(detectable.id); // TODO: replace GameObject.Find(...)
            if (detectableObj)
            {
                var detectableBehaviour = detectableObj.GetComponent<DetectableBehaviour>();
                detectableBehaviour.SetTrackable(targetHolder);
            }
            else
            {
                AppLog.LogError($"Can't find detectable {detectable.id}");
            }
        }

        public void MoveDetectableBack()
        {
            if (!_clearAll)
            {
                var place = RootObject.Instance.workplaceManager.GetPlaceFromTaskStationId(_content.id);
                var detectable = RootObject.Instance.workplaceManager.GetDetectable(place);
                var detectableObj = GameObject.Find(detectable.id); // TODO: replace GameObject.Find(...)
                if (detectableObj)
                {
                    var detectableBehaviour = detectableObj.GetComponent<DetectableBehaviour>();
                    detectableBehaviour.RemoveTrackable();
                }
                else
                {
                    AppLog.LogError($"Can't find detectable {detectable.id}");
                }
            }
        }

        public override void Delete()
        {
            // changed Delete to a virtual method so I could overide it for Image markers as they were being deleted twice when changing activities causeing the new activity not to load
        }

        private void OnDestroy()
        {
            RootObject.Instance.imageTargetManager.RemoveImageTarget(target);
        }
    }
}