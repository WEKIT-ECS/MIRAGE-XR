
[System.Serializable]
public class AccessTokenFailJson
{
    public string message;
}


[System.Serializable]
public class AccessTokenResponseJson
{
    public string access_token;
    public string token_type;
    public string scope;
    public long expires_in;
    public string refresh_token;
}


[System.Serializable]
public class AuthorizationCodeResponseJson
{
    public string code;
    //public bool iserror;
    //public string error_description;
}


[System.Serializable]
public class UserInfoResponseJson
{
    public string sub;
    public string nickname;
    public string given_name;
    public string family_name;
    public string email;
}




