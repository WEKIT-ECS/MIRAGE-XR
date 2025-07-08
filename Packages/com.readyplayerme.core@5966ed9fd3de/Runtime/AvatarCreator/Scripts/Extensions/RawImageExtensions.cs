using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class RawImageExtensions
    {
        public static void SizeToParent(this RawImage image, float padding = 0)
        {
            var width = 0f;
            var height = 0f;
            var parent = image.GetComponentInParent<RectTransform>();
            var imageTransform = image.GetComponent<RectTransform>();

            if (image.texture != null)
            {
                if (!parent)
                {
                    return;
                }

                padding = 1 - padding;

                var ratio = image.texture.width / (float) image.texture.height;
                var parentRect = parent.rect;
                var bounds = new Rect(0, 0, parentRect.width, parentRect.height);

                if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
                {
                    //Invert the bounds if the image is rotated
                    bounds.size = new Vector2(bounds.height, bounds.width);
                }
                //Size by height first
                height = bounds.height * padding;
                width = height * ratio;
                if (width > bounds.width * padding)
                {
                    //If it doesn't fit, fallback to width;
                    width = bounds.width * padding;
                    height = width / ratio;
                }
            }
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
