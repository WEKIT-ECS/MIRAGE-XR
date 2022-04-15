using System;

[Serializable]
public class ModelDownloadInfo
{
    public ModelInfo gltf;
    public ModelInfo usdz;
}

[Serializable]
public class ModelInfo
{
    public string url = "";
    public long size = 0;
    public int expires = 0;
}


[Serializable]
public class ModelPreviewItem
{
    public string name = "";
    public string description = "";
    public string uid = "";
    public string resourceUrl = "";
    public ThumbnailImage resourceImage;
    public float fileSize = 0.0f;
}


[Serializable]
public class SketchfabModelSearchResult
{
    public SearchCursor cursors;
    public string next = "";
    public string previous = "";
    public SketchfabModel[] results;
}

[Serializable]
public class SketchfabCollectionSearchResult
{
    public SearchCursor cursors;
    public string next = "";
    public string previous = "";
    public SketchfabCollection[] results;
}

[Serializable]
public class SketchfabUserSearchResult
{
    public SearchCursor cursors;
    public string next = "";
    public string previous = "";
    public SketchfabUser[] results;
}

[Serializable]
public class SearchCursor
{
    public string next = "";
    public string previous = "";

}


