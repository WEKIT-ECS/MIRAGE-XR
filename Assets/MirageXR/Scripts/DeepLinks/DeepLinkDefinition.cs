using i5.Toolkit.Core.DeepLinkAPI;
using MirageXR;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Defines deep link paths to which the application should react
/// </summary>
public class DeepLinkDefinition
{
    /// <summary>
    /// Load an activity via a deep link, e.g. wekit:/load?id=123&dowload=somePath
    /// </summary>
    /// <param name="args">Arguments that are passed with the deep link;
    /// should contain the parameters id and download</param>
    [DeepLink(path: "load")]
    public async void LoadActivity(DeepLinkArgs args)
    {
        if (args.Parameters.TryGetValue("id", out string activityId)
            && args.Parameters.TryGetValue("download", out string downloadPath))
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, activityId)))
            {
                await Open(activityId);
            }
            else
            {
                bool success = await Download(downloadPath, activityId);
                if (success)
                {
                    await Open(activityId);
                }
            }
        }
        else
        {
            Debug.LogError("Deep Link is missing id or download parameter");
            DialogWindow.Instance.Show(
            "Info!",
            "Activity launch protocol is not complete",
            new DialogButtonContent("Ok"));
        }
    }

    /// <summary>
    /// Creates a new activity via a deep link, e.g. using wekit:/new
    /// </summary>
    [DeepLink(path: "new")]
    public async void NewActivity()
    {
        await RootObject.Instance.EditorSceneService.LoadEditorAsync();
        await RootObject.Instance.ActivityManagerOld.CreateNewActivity();
    }

    // opens the given activity
    private async Task Open(string activity)
    {
        string fullActivityJson = $"{activity}-activity.json";
        PlayerPrefs.SetString("activityUrl", fullActivityJson);
        PlayerPrefs.Save();

        await RootObject.Instance.EditorSceneService.LoadEditorAsync();
        await RootObject.Instance.ActivityManagerOld.LoadActivity(fullActivityJson);
    }

    // downloads the zip file of the activity from the given downloadPath and unzips the file
    private async Task<bool> Download(string downloadPath, string activityId)
    {
        Debug.Log("Download session");

        bool success;
        using (SessionDownloader downloader = new SessionDownloader(
            DBManager.domain + "/pluginfile.php/" + downloadPath,
            activityId + ".zip"))
        {
            success = await downloader.DownloadZipFileAsync();

            if (success)
            {
                try
                {
                    await downloader.UnzipFileAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    success = false;
                }
            }
            else
            {
                DialogWindow.Instance.Show(
                "Info!",
                "Activity not found! Please check that you are connected to the correct Moodle repository",
                new DialogButtonContent("Ok"));
            }
        }
        return success;
    }
}
