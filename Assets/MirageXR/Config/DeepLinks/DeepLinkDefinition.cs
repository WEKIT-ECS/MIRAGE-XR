using i5.Toolkit.Core.DeepLinkAPI;
using MirageXR;
using System;
using Cysharp.Threading.Tasks;
using MemoryPack.Internal;

/// <summary>
/// Defines deep link paths to which the application should react
/// </summary>
public class DeepLinkDefinition
{
    /// <summary>
    /// Load an activity via a deep link, e.g. wekit://call?activity=123&dowload=somePath
    /// </summary>
    /// <param name="args">Arguments that are passed with the deep link;
    /// should contain the parameters id and download</param>
    [Preserve]
    [DeepLink(path: "load")]
    public void LoadActivity(DeepLinkArgs args)
    {
        LoadActivityAsync(args).Forget();
    }

    private async UniTask LoadActivityAsync(DeepLinkArgs args)
    {
        await RootObject.Instance.LEE.WaitForInitialization();

        if (args.Parameters.TryGetValue("activity", out var activityId))
        {
            await Open(activityId);
        }
        else
        {
            Debug.LogError("Deep Link is missing 'activity' guid parameter");
            DialogWindow.Instance.Show("Info!", "Activity launch failed, parameter 'activity' is missing.", new DialogButtonContent("Ok"));
        }
    }

    /// <summary>
    /// Creates a new activity via a deep link, e.g. using wekit:/new
    /// </summary>
    [Preserve]
    [DeepLink(path: "new")]
    public void NewActivity()
    {
        NewActivityAsync().Forget();
    }

    private async UniTask NewActivityAsync()
    {
        await RootObject.Instance.LEE.WaitForInitialization();

        var baseCamera = RootObject.Instance.BaseCamera;
        var position = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;   //TODO: move to Manager
        RootObject.Instance.LEE.ActivityManager.CreateNewActivity(position);
    }

    // opens the given activity
    private async UniTask Open(string activityID)
    {
        if (Guid.TryParse(activityID, out var guid))
        {
            try
            {
                var loadView = LoadView.Instance;
                if (loadView)
                {
                    loadView.Show();
                }
                //TODO: add ActivityLoadStartedEvent
                await RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(guid);
                //TODO: add ActivityLoadStartedEvent
                if (loadView)
                {
                    loadView.Hide();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }
    }
}
