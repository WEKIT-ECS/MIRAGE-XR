using UnityEngine;

namespace MirageXR
{
    public class RoomScanGallery : GalleryBase
    {
        protected override IThumbnailProvider ThumbnailProvider => RootObject.Instance.RoomTwinManager;

        protected override void DeleteElement(string elementId)
        {
            RootObject.Instance.RoomTwinLibraryManager.Remove(elementId);
        }
    }
}
