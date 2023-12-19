using System;

[Serializable]
public class SketchfabCollection
{
    public string name = "";
    public string uid = "";
    public string collectionUrl = "";

    public int modelCount = -1;

    public string createdAt = "";
    public string updatedAt = "";

    public string embedUrl = "";
    public string slug = "";

    public SketchfabUser user;

}
