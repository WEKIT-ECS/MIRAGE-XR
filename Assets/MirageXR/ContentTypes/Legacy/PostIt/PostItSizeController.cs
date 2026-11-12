using UnityEngine;

namespace MirageXR
{
    public class PostItSizeController : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private RectTransform _rectTransform;

        private void Awake()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }
        }

        private void Start()
        {
            AdjustSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            AdjustSize();
        }

        public void AdjustSize()
        {
            if (_rectTransform == null || _boxCollider == null)
            {
                return;
            }

            var rect = _rectTransform.rect;
            _boxCollider.size = new Vector3(rect.width, rect.height, 0.01f);
            _boxCollider.center = new Vector3(rect.center.x, rect.center.y, 0);

            var boundingBox = GetComponent<SimpleBoundingBox>();
            if (boundingBox != null)
            {
                boundingBox.UpdateBounds();
            }
        }
    }
}
