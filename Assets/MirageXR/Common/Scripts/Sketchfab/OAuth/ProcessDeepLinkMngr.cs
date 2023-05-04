using UnityEngine;

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
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL); // Cold start and Application.absoluteURL not null so process Deep Link.
            }
            else
            {
                deeplinkURL = "[none]"; // Initialize DeepLink Manager global variable.
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        deeplinkURL = url;

        var qMark = new char[1] { '?' };
        var equal = new char[1] { '=' };

        var codePair = url.Split(qMark)[1];
        var code = codePair.Split(equal)[1];

        Debug.Log("deep link sent:" + code);
        OnDeepLinkReceived?.Invoke(code);
    }
}
