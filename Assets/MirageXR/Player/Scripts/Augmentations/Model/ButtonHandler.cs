
using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Collections;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{

    [Tooltip("The gameobject to show when the model is available locally.")]
    public GameObject downloadedIndicator;

    public ModelPreviewItem PreviewItem { get; set; }

    private bool isLocalObject = false;
    public bool IsLocalObject
    {
        get
        {
            return isLocalObject;
        }
        set 
        {
            downloadedIndicator.SetActive(value);
            isLocalObject = value; 
        }
    }

    public delegate void ClickEventHandler(ButtonHandler sender, ModelPreviewItem modelPreviewItem);
    public ClickEventHandler OnItemDownloadClick;
    public ClickEventHandler OnItemLoadClick;

    public void ReceiveModelClick()
    {
        if (PreviewItem == null)
        {
            return;
        }

        if (PreviewItem.resourceUrl.StartsWith("http"))
        {
            QueryDownloadModel(PreviewItem);
        }
        else if (PreviewItem.resourceUrl.StartsWith("file"))
        {
            QueryLoadModel(PreviewItem);
        }
        else
        {
            Debug.LogWarning("Resource url was not parsable");
        }
    }

    public void QueryLoadModel(ModelPreviewItem model)
    {
        float startTime = Time.time;

        // truncate description
        if (model.description.Length > 200)
        {
            model.description = model.description.Substring(0, 200);
        }

        /// Confirm and load
        DialogWindow.Instance.Show("Confirm selection",
        $"Do you want to load {model.name}?",
        new DialogButtonContent("Yes", () => { OnItemLoadClick.Invoke(this, PreviewItem); }),
        new DialogButtonContent("No"));
    }

    public void QueryDownloadModel(ModelPreviewItem model)
    {
        float startTime = Time.time;

        // truncate description
        if (model.description.Length > 200)
        {
            model.description = model.description.Substring(0, 200);
        }

        /// Confirm and download
        DialogWindow.Instance.Show("Confirm download",
        $"Are you sure you wish to download {model.name}? \n{model.description}",
        new DialogButtonContent("Yes", () => { OnItemDownloadClick.Invoke(this, PreviewItem); }),
        new DialogButtonContent("No"));
    }

    public void ConfirmLargeFileDownload(ModelPreviewItem model, ModelDownloadInfo downloadInfo, Action<string, ModelPreviewItem> modelDownloadCallback)
    {
        float startTime = Time.time;

        /// Confirm and download
        DialogWindow.Instance.Show($"Warning. Large file requested ({model.fileSize} MB)",
        $"Are you sure you wish to download {model.name}? \n\nLarge files may not display correctly, and may cause performance issues. Where possible, use 'low poly' models (< 20 MB).",
        new DialogButtonContent("Yes", () => { modelDownloadCallback(downloadInfo.gltf.url, model); }),
        new DialogButtonContent("No"));

    }
}
