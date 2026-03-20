using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class AvatarGallery : GalleryBase
    {
        protected override IThumbnailProvider ThumbnailProvider { get => RootObject.Instance.AvatarLoadManager; }

        protected override void DeleteElement(string elementId)
        {
            RootObject.Instance.AvatarLibraryManager.RemoveAvatar(elementId);
        }
    }
}
