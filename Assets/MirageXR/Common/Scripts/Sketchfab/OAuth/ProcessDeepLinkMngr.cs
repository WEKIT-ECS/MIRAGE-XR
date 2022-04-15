using UnityEngine;

using System;

public class ProcessDeepLinkMngr : MonoBehaviour
{
    public static ProcessDeepLinkMngr Instance { get; private set; }
    public string deeplinkURL;

    public delegate void OnDeepLinkReceivedDelegate(string deepLinkResult);
    public event OnDeepLinkReceivedDelegate OnDeepLinkReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += OnDeepLinkActivated;
            if (!String.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        deeplinkURL = url;

        char[] qMark = new char[1] { '?' };
        char[] equal = new char[1] { '=' };

        string codePair = url.Split(qMark)[1];
        string code = codePair.Split(equal)[1];

        Debug.Log("deep link sent:" + code);
        OnDeepLinkReceived?.Invoke(code);
    }
}
