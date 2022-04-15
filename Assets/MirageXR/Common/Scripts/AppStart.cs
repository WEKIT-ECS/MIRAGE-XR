using MirageXR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID
using UnityEngine.Android;
#elif UNITY_IOS
using UnityEngine.iOS;
#endif

/// <summary>
/// Describes how the app should proceed from the start scene
/// </summary>
public class AppStart : MonoBehaviour
{
    // starts the initialization
    private void Start()
    {
        StartCoroutine(Init());
    }

    // initializes the app
    private IEnumerator Init()
    {
        yield return RequestPermissions();
        yield return CreateIBMWatsonCredential();
        yield return LoadScene();
    }

    IEnumerator CreateIBMWatsonCredential()
    {
        var ibmCredentialsFile = Path.Combine(Application.persistentDataPath, "ibm-credentials.env");
        if (!File.Exists(ibmCredentialsFile))
        {
            var ibmCredentials = Resources.Load("ibm-credentials") as TextAsset;
            if (ibmCredentials)
                File.WriteAllText(ibmCredentialsFile, ibmCredentials.text);
        }

        Environment.SetEnvironmentVariable("IBM_CREDENTIALS_FILE", ibmCredentialsFile);

        yield return null;
    }

    private void OnApplicationQuit()
    {
        var ibmCredentialsFile = Path.Combine(Application.persistentDataPath, "ibm-credentials.env");
        if (File.Exists(ibmCredentialsFile))
            File.Delete(ibmCredentialsFile);
    }

    // requests necessary permissions on Android and iOS if they have not yet been granted
    private IEnumerator RequestPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) 
               || !Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(0.5f);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                yield return new WaitForSeconds(0.5f);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                yield return new WaitForSeconds(0.5f);
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        while (!Application.HasUserAuthorization(UserAuthorization.Microphone)
               || !Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
                yield return new WaitForSeconds(0.5f);
            }
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
                yield return new WaitForSeconds(0.5f);
            }
        }
#else
        yield return null;
#endif
    }

    // loads the next scene, the activity selection, and waits for the scene to be loaded
    private IEnumerator LoadScene()
    {
        var loader = SceneManager.LoadSceneAsync(PlatformManager.Instance.ActivitySelectionScene, LoadSceneMode.Additive);
        while (!loader.isDone)
        {
            yield return null;
        }

    }
}
