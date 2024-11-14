using System;

namespace MirageXR.DataModel
{
    [Serializable]
    public class SketchfabModel
    {
        public string name = "";
        public string description = "";
        public string license = "";

        public string editorUrl = "";
        public string viewerUrl = "";
        public string embedUrl = "";


        public string uri = "";
        public string uid = "";

        public string createdAt = "";
        public string publishedAt = "";
        public string staffpickedAt = "";
    
        public int likeCount = -1;
        public int viewCount = -1;
        public int commentCount = -1;
        public int downloadCount = -1;

        public int animationCount = -1;
        public int soundCount = -1;

        public long vertexCount = -1;
        public long faceCount = -1;

        public bool isAgeRestricted = false;
        public bool isDownloadable = false;

        public ModelTag[] tags;
        public ModelCategory[] categories;
        public ModelThumbnails thumbnails;

        public SketchfabUser user;

    }

    [Serializable]
    public class ModelTag
    {
        public string name = "";
        public string slug = "";
        public string uri = "";
    }

    [Serializable]
    public class ModelCategory
    {
        public string name = "";
    }

    [Serializable]
    public class ModelThumbnails
    {
        public ThumbnailImage[] images;
    }

    [Serializable]
    public class ThumbnailImage
    {
        public string url = "";
        public string uid = "";
        public int width = 0;
        public int height = 0;
    }

    [Serializable]
    public class ModelArchive
    {
        public GltfArchive gltf;
    }

    [Serializable]
    public class GltfArchive
    {
        public int size = 0;
    }
}