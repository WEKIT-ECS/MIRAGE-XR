using LearningExperienceEngine;
using i5.Toolkit.Core.VerboseLogging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class ImageMarkerController : MirageXRPrefab
    {
        private ImageTargetManagerWrapper imageTargetManager => RootObject.Instance.ImageTargetManager;

        private string _imageName;
        private LearningExperienceEngine.ToggleObject _content;
        public IImageTarget _target;

        public override bool Init(LearningExperienceEngine.ToggleObject content)
        {
            _content = content;
            InitAsync().AsAsyncVoid();
            return true;
        }

        private async Task<bool> InitAsync()
        {
            if (string.IsNullOrEmpty(_content.url))
            {
                Debug.LogError("Content URL not provided.");
                return false;
            }

            if (!SetParent(_content))
            {
                Debug.LogError("Couldn't set the parent.");
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
                    Debug.LogError("Can't add image target");
                    return false;
                }
            }

            MoveDetectableToImage(imageTarget.transform);

            return base.Init(_content);
        }

        private async Task<ImageTargetBase> LoadImage()
        {
            var imagePath = Path.Combine(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath, _imageName);
            var byteArray = await File.ReadAllBytesAsync(imagePath);
            var texture = new Texture2D(2, 2);

            if (!texture.LoadImage(byteArray))
            {
                Debug.LogError($"Can't load image. path: {imagePath}");
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

            _target = await RootObject.Instance.ImageTargetManager.AddImageTarget(model);

            return _target as ImageTargetBase;
        }

        private void MoveDetectableToImage(Transform targetHolder)
        {
            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
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
                Debug.LogError($"Can't find detectable {detectable.id}");
            }
        }

        public void MoveDetectableBack()
        {
            var place = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.GetPlaceFromTaskStationId(_content.id);
            var detectable = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.GetDetectable(place);
            var detectableObj = GameObject.Find(detectable.id); // TODO: replace GameObject.Find(...)
            if (detectableObj)
            {
                var detectableBehaviour = detectableObj.GetComponent<DetectableBehaviour>();
                detectableBehaviour.RemoveTrackable();
            }
            else
            {
                Debug.LogError($"Can't find detectable {detectable.id}");
            }
        }

        public override void Delete()
        {
            // changed Delete to a virtual method so I could overide it for Image markers as they were being deleted twice when changing activities causeing the new activity not to load
        }

        private void OnDestroy()
        {
            try
            {
                if (RootObject.Instance.PlatformManager.WorldSpaceUi)
                {
                    MoveDetectableBack();
                    RootObject.Instance.ImageTargetManager.RemoveImageTarget(_target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error when destroying image marker controller" + e.ToString());
            }
        }
    }
}