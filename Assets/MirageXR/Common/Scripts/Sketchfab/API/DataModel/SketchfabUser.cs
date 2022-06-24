using System;

[Serializable]
public class SketchfabUser
{

    public string username = "";
    public string profileUrl = "";
    public string account = "";
    public string displayName = "";

    public string uid = "";
    public string url = "";

    public UserAvatar avatar;

}

[Serializable]
public class UserAvatar
{
    public string uid = "";
    public string url = "";
    public UserAvatarImages[] images;

}

[Serializable]
public class UserAvatarImages
{
    public string url;
    public string uid;
    public int width = 0;
    public int height = 0;
}


