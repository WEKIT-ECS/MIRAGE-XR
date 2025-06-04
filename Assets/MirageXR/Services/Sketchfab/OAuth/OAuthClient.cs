using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

// using Windows.Data.Json;
// using Windows.Security.Authentication.Web;
// using Windows.UI.Xaml;
// using Windows.UI.Xaml.Controls;
// using Windows.Web.Http;


public class OAuthClient : MonoBehaviour
{
    private static string loopbackURL = "https://wekit-community.org/sketchfab/callback.php";
    public static string LoopbackURL
    {
        get
        {
            return loopbackURL;
        }
        set
        {
            loopbackURL = value;
        }
    }

    private static string loopbackURLlocal = "http://127.0.0.1:52072";
    public static string LoopbackURLlocal
    {
        get
        {
            return loopbackURLlocal;
        }
        set
        {
            loopbackURLlocal = value;
        }
    }

    private static string apiURL = "https://sketchfab.com/oauth2/";
    public static string ApiURL
    {
        get
        {
            return apiURL;
        }
        set
        {
            apiURL = value;
        }
    }

    public const string authorizeEndpoint = "authorize/";
    public const string tokenEndpoint = "token/";
    public const string userInfoEndpoint = "userinfo/";
    public const string clientParam = "client_id";
    public const string redirectUriParam = "redirect_uri";
    public const string scopeParam = "scope";
    public const string stateParam = "state";

    public const string Key_AccessToken = "access_token";
    private Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private IEnumerator ExecuteFromQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            while (ExecuteOnMainThread.Count > 0)
            {
                Action action = ExecuteOnMainThread.Dequeue();
                action();
            }
        }
    }

    Process pp = new Process();

    private string appID;
    private string appSecret;
    private string scope;
    private string responseType;

    public float LoginProcessTimeOut = 20;
    public float retryMaxCount = 3;
    public float retryCount = 0;


    private string authorizationCode;
    public string AuthorizationCode
    {
        get
        {
            return authorizationCode;
        }
    }

    private static OAuthClient instance;
    public static OAuthClient Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject o = new GameObject("OAuthClient");
                instance = o.AddComponent<OAuthClient>();
                DontDestroyOnLoad(o);
            }
            return instance;
        }
    }

    private static void CheckCode(object sendingProcess,
            DataReceivedEventArgs outLine)
    {
        Debug.Log("line out:" + outLine.Data);
    }

    private static void CaptureError(object sendingProcess,
            DataReceivedEventArgs outLine)
    {
        Debug.Log("err out:" + outLine.Data);
    }


    public void Authenticate(Action<string> callback)
    {
        if (isAuthenticated)
        { return; }

        var url = string.Empty;
#if UNITY_EDITOR
        url = System.String.Format(apiURL + authorizeEndpoint + "?response_type={0}&client_id={1}&redirect_uri={2}",
            responseType, appID, LoopbackURLlocal);
#elif WINDOWS_UWP
		url = System.String.Format(apiURL + authorizeEndpoint + "?response_type={0}&client_id={1}&redirect_uri={2}",
			responseType, appID, LoopbackURL);
#endif
        Debug.Log("starting authorisation " + url + ", " + loopbackURL);

        Application.OpenURL(url);

        // pp.StartInfo = new ProcessStartInfo(url);
        // pp.OutputDataReceived += CheckCode;
        // pp.EnableRaisingEvents = true;
        // pp.Start();

        // StartCoroutine(WaitForAuthButtonPressThenAuthenticate(callback));
    }

    public bool AuthButtonPressed = false;
    public string AuthCode = string.Empty;


    public IEnumerator WaitForAuthButtonPressThenAuthenticate(Action<string> callback)
    {
        var startTime = Time.time;
        yield return new WaitForSeconds(1.0f);

        while (!AuthButtonPressed && (Time.time - startTime) < 30.0f)
            yield return null;

        print("auth button pressed");
        callback(AuthCode);
    }

    public async Task<string> RequestExternalAccessToken(string provider)
    {
        // Creates a redirect URI using an available port on the loopback address.
        var redirectURI = $"http://{IPAddress.Loopback}:{52072}/";

        // Creates an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(redirectURI);
        http.Start();

        // Creates the OAuth 2.0 authorization request.
        var authorizationRequest =
            $"{apiURL}{authorizeEndpoint}?client_id={appID}&redirect_uri={LoopbackURL}&response_type={responseType}";

        // Opens request in the browser.
        Process.Start(authorizationRequest);

        // Waits for the OAuth authorization response.
        var context = await http.GetContextAsync();

        // Sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = string.Format("<html><head></head><body></body></html>");
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
        {
            responseOutput.Close();
            http.Stop();
            Console.WriteLine("HTTP server stopped.");
        });

        // Checks for errors.
        if (context.Request.QueryString.Get("access_token") == null)
        {
            throw new ApplicationException("Error connecting to server");
        }

        var externalToken = context.Request.QueryString.Get("access_token");

        return "/api/Account/GetAccessToken";

    }

    IEnumerator DoOAuth()
    {
        yield return new WaitForSeconds(5.0f);

        using (UnityWebRequest req = UnityWebRequest.Get(LoopbackURL))
        {
            yield return req.SendWebRequest();

            while (!req.isDone)
                yield return null;

            long code = req.responseCode;
            string header = req.GetRequestHeader("code");
            string result = req.downloadHandler.text;

            Debug.Log($"u-web-req info (code, header, length): {code},{header}, {result.Length}");
        }
    }

    public void GetToken(string code, Action<string> callback)
    {
        string url = apiURL + tokenEndpoint;
        Debug.Log($"{url}; {code}");

        ExecuteOnMainThread.Enqueue(() => StartCoroutine(RequestToken(url, code, callback)));
        StartCoroutine(ExecuteFromQueue());
        //StartCoroutine(RequestToken(url, code, callback));
    }

    IEnumerator RequestToken(string url, string code, Action<string> callbackTokenReceived)
    {
        string redirectUri = String.Empty;
#if UNITY_EDITOR
        redirectUri = LoopbackURLlocal;
#elif WINDOWS_UWP
		redirectUri = LoopbackURL;
#endif

        WWWForm form = new WWWForm();
        form.AddField("client_id", appID);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("code", code);
        if (responseType == "code")
            form.AddField("grant_type", "authorization_code");
        if (appSecret != "")
            form.AddField("client_secret", appSecret);
        foreach (KeyValuePair<string, string> field in form.headers)
            Debug.Log($"{field.Key}: {field.Value}");

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        string response;
        if (www.error != null)
        {
            onAuthenticationFailedOrCancelled?.Invoke($"{www.error}: {www.downloadHandler.text}");
            callbackTokenReceived(null);
        }
        else
        {
            response = (www.downloadHandler.text);
            try
            {
                AccessTokenResponseJson tokenJson = JsonUtility.FromJson<AccessTokenResponseJson>(response);
                if (!String.IsNullOrEmpty(tokenJson.access_token))
                {
                    isAuthenticated = true;
                    onTokenReceivedSuccessfully?.Invoke();
                    callbackTokenReceived(tokenJson.access_token);
                }
                else
                {
                    onAuthenticationFailedOrCancelled?.Invoke("Access token is empty");
                    callbackTokenReceived(null);

                }

            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "," + e.StackTrace);
                onAuthenticationFailedOrCancelled?.Invoke($"{e.Message},{e.StackTrace}");
            }
        }
        yield return null;

    }

    public void GetUserInfo(string access_token, Action<UserInfoResponseJson> callback)
    {
        string url = apiURL + userInfoEndpoint;
        StartCoroutine(RequestUserInfo(url, access_token, callback));
    }

    IEnumerator RequestUserInfo(string url, string access_token, Action<UserInfoResponseJson> userInfoReceived)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + access_token);
        yield return www.SendWebRequest();
        string response;
        if (www.error != null)
        {
            onAuthenticationFailedOrCancelled?.Invoke($"{www.error}: {www.downloadHandler.text}");
            userInfoReceived(null);
        }
        else
        {
            response = (www.downloadHandler.text);
            try
            {
                UserInfoResponseJson userInfo = JsonUtility.FromJson<UserInfoResponseJson>(response);
                userInfoReceived(userInfo);
            }
            catch (Exception exception)
            {
                onAuthenticationFailedOrCancelled?.Invoke(exception.Message);
                userInfoReceived(null);
            }
        }

    }


    public void OnReceivedCode(string code)
    {
        authorizationCode = code;
    }


    public void Init(string _appID, string _appSecret, string _scope, string _response_type)
    {
        appID = _appID;
        appSecret = _appSecret;
        scope = _scope;
        responseType = _response_type;
        StartCoroutine(ExecuteFromQueue());
    }


    private bool isAuthenticated;
    public bool IsLoggedIn()
    {
        return isAuthenticated;
    }



    #region External URL Handling
    public void OpenURL(string url)
    {
#if (UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR
			Application.ExternalEval("window.open('"+url+"','_blank')");
#else
        Application.OpenURL(url);
#endif
    }
    #endregion

    #region Callbacks
    public delegate void OnHybURLCallback(string response);

    IEnumerator GetURLCallback(string url, OnHybURLCallback onHybURLCallback)
    {
        using var www = new UnityWebRequest(url);
        yield return www.SendWebRequest();;
        if (www.result != UnityWebRequest.Result.Success)
        {
            var data = $"{{\"iserror\":true,\"error_code\":0,\"error_message\":\"{www.error}\"}}";
            onHybURLCallback(data);
        }
        else
        {
            onHybURLCallback(www.downloadHandler.text);;
        }
    }
    #endregion

    #region Events & Delegates
    public delegate void OnLoggedInSuccessfully();
    public delegate void OnLoginFailedOrCancelled(string reason = null);
    public delegate void OnLoginFailedTimeOut();
    public delegate void OnValidatingUser();
    public delegate void OnValidatingUserFailed();

    public OnLoggedInSuccessfully onTokenReceivedSuccessfully;
    public OnLoginFailedOrCancelled onAuthenticationFailedOrCancelled;
    public OnLoginFailedTimeOut onLoginFailedTimeOut;
    public OnValidatingUser onValidatingUser;
    public OnValidatingUserFailed onValidatingUserFailed;

    public delegate void OnFacebookResponseReceived(FacebookResponse response);

    public delegate void OnHybFBProcessCallback(string response);
    #endregion

    #region Misc


    public delegate void OnImageLoaded(Texture2D texture);
    public void LoadImage(string url, OnImageLoaded onImageLoaded)
    {
        StartCoroutine(LoadImage_I(url, onImageLoaded));
    }

    IEnumerator LoadImage_I(string url, OnImageLoaded onImageLoaded)
    {
        UnityWebRequest request = new UnityWebRequest(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            onImageLoaded(texture);
        }
        else
        {
            onImageLoaded(null);
        }
    }


    #endregion
}


public struct FacebookResponse
{
    public string error;
    public string text;
    public byte[] bytes;
    public Texture2D texture;
    public Texture2D textureNotReadable;

}

