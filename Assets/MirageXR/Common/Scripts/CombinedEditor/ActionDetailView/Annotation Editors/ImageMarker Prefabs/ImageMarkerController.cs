using System.IO;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public class ImageMarkerController : MirageXRPrefab
    {
        private string _imageName;
        private ToggleObject _content;

        public override bool Init(ToggleObject content)
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

            if (!GameObject.Find(_imageName))
            {
                await LoadImage();
            }

            DetectableAsChild();

            return base.Init(_content);
        }

        private async Task<bool> LoadImage()
        {
            var imagePath = Path.Combine(RootObject.Instance.activityManager.ActivityPath, _imageName);
            var byteArray = await File.ReadAllBytesAsync(imagePath);
            var texture = new Texture2D(2, 2);

            if (!texture.LoadImage(byteArray))
            {
                return false;
            }

            var model = new ImageTargetModel
            {
                name = _imageName,
                prefab = null,
                width = 0.5f,
                texture2D = texture,
                useLimitedTracking = true,
            };

            var imageTarget = await RootObject.Instance.imageTargetManager.AddImageTarget(model);
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            primitive.transform.SetParent(((ImageTargetBase)imageTarget).transform);
            primitive.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            return imageTarget != null;
        }

        private void DetectableAsChild()
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            var taskStationId = workplaceManager.GetPlaceFromTaskStationId(_content.id);
            var detectable = workplaceManager.GetDetectable(taskStationId);
            var augmentation = GameObject.Find(detectable.id);
            augmentation.transform.SetParent(GameObject.Find(_imageName).transform);
            augmentation.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        public void PlatformOnDestroy()
        {
            var place = RootObject.Instance.workplaceManager.GetPlaceFromTaskStationId(_content.id);
            var detectable = RootObject.Instance.workplaceManager.GetDetectable(place);

            var detectableObj = GameObject.Find(detectable.id);
            var detectableParentObj = GameObject.Find("Detectables");

            detectableObj.transform.SetParent(detectableParentObj.transform);
            Destroy(gameObject);
        }

        public override void Delete()
        {
            Debug.Log("-------Delete");
            // changed Delete to a virtual method so I could overide it for Image markers as they were being deleted twice when changing activities causeing the new activity not to load
        }
    }
}