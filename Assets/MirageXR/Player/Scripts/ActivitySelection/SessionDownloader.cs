using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Utilities;
using i5.Toolkit.Core.VerboseLogging;

namespace MirageXR
{
    public class SessionDownloader : IDisposable
    {
        private readonly string _targetFolder;
        private string _downloadUrl;
        private string _zipFilePath;

        public SessionDownloader(string downloadUrl, string zipFileName)
        {
            _downloadUrl = downloadUrl;
            _targetFolder = Application.persistentDataPath;
            _zipFilePath = Path.Combine(_targetFolder, zipFileName);
        }

        public async Task<bool> DownloadZipFileAsync()
        {
            using (UnityWebRequest req = new UnityWebRequest(_downloadUrl))
            {
                req.method = UnityWebRequest.kHttpVerbGET;
                Debug.LogInfo($"Downloading zip file to " + _zipFilePath);
                DownloadHandlerFile downloadHandler = new DownloadHandlerFile(_zipFilePath) { removeFileOnAbort = true };
                req.downloadHandler = downloadHandler;
                await req.SendWebRequest();
                bool success = !req.isHttpError && !req.isNetworkError;
                downloadHandler.Dispose();
                return success;
            }
        }

        public async Task UnzipFileAsync()
        {
            Debug.LogDebug($"Application persistent data path is {Application.persistentDataPath}");
            Debug.LogInfo($"Unzipping zip file from {_zipFilePath} to {_targetFolder}");
            using (Stream stream = new FileStream(_zipFilePath, FileMode.Open))
            {
                await ZipUtilities.ExtractZipFileAsync(stream, _targetFolder);
            }
        }

        public void Dispose()
        {
            Debug.LogTrace("Disposing session downloader");
            if (File.Exists(_zipFilePath))
            {
                Debug.LogDebug($"Clean up: Deleting zip file at {_zipFilePath}");
                File.Delete(_zipFilePath);
            }
        }
    }
}