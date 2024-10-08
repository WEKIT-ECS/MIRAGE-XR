using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using MirageXR.View;
using Unity.SharpZipLib.Zip;
using UnityEngine;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

namespace MirageXR.NewDataModel
{
    public class AssetsManager : IAssetsManager
    {
        private const string ZipExtension = ".zip";
        private const string MirageXRAssetsBundle = "MirageXRAssetsBundle";

        public bool IsInitialized => _isInitialized;

        private INetworkDataProvider _networkDataProvider;
        private IActivityManager _activityManager;
        private AssetsBundle _assetsBundle;
        private bool _isInitialized;

        public UniTask WaitForInitialization()
        {
            return UniTask.WaitUntil(() => _isInitialized);
        }

        public async UniTask InitializeAsync(INetworkDataProvider networkDataProvider, IActivityManager activityManager)
        {
            _assetsBundle = await Resources.LoadAsync<AssetsBundle>(MirageXRAssetsBundle) as AssetsBundle;

            if (_assetsBundle == null)
            {
                throw new Exception("MirageXR assets bundle not found");
            }

            _networkDataProvider = networkDataProvider;
            _activityManager = activityManager;
            _isInitialized = true;
        }

        public async UniTask PrepareContent(Guid activityId, Content content)
        {
            var list = GetFilesToLoad(content);

            foreach (var fileModel in list)
            {
                if (!await IsHashEqual(activityId, content.Id, fileModel))
                {
                    _networkDataProvider.DownloadAssetAsync(activityId, fileModel.Id);
                }
            }
        }

        public FileModel GetDefaultThumbnail()
        {
            return new FileModel
            {
                Id = Guid.Empty,
                //Name = "Thumbnail",
                FileHash = HashCode.Combine(Guid.NewGuid(), Guid.NewGuid()).ToString(),  //temp,
                Version = Application.version,
                CreationDate = DateTime.UtcNow
            };
        }

        public async UniTask<FileModel> CreateFileAsync(string folderPath, Guid activityId, Guid contentId)
        {
            var fileId = Guid.NewGuid();

            await using (var stream = new FileStream(GetZipPath(activityId, contentId, fileId), FileMode.OpenOrCreate))
            await using (var zipStream = new ZipOutputStream(stream))
            {
                await ZipUtilities.CompressFolderAsync(GetFolderPath(activityId, contentId, fileId), zipStream);
            }

            return new FileModel
            {
                Id = fileId,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                FileHash = GetFileHash(activityId, contentId, fileId)
            };
        }

        public StepView GetStepViewPrefab()
        {
            return _assetsBundle.GetStepViewPrefab();
        }

        public CalibrationTool GetCalibrationToolPrefab()
        {
            return _assetsBundle.GetCalibrationToolPrefab();
        }

        public ContentView GetContentViewPrefab(ContentType contentType)
        {
            return _assetsBundle.GetContentViewPrefab(contentType);
        }

        public string GetFolderPath(Guid contentId, Guid fileId)
        {
            return Path.Combine(Application.persistentDataPath, _activityManager.ActivityId.ToString(), contentId.ToString(), fileId.ToString());
        }

        public string GetFolderPath(Guid activityId, Guid contentId, Guid fileId)
        {
            return Path.Combine(Application.persistentDataPath, activityId.ToString(), contentId.ToString(), fileId.ToString());
        }

        public string GetZipPath(Guid activityId, Guid contentId, Guid fileId)
        {
            return GetFolderPath(activityId, contentId, fileId) + ZipExtension;
        }

        private bool IsFileExists(Guid activityId, Guid contentId, Guid fileId)
        {
            return File.Exists(GetZipPath(activityId, contentId, fileId));
        }

        private async UniTask<bool> IsHashEqual(Guid activityId, Guid contentId, FileModel fileModel)
        {
            return false;

            /*if (IsFileExists(activityId, contentId, fileModel.Id))
            {
                return false;
            }

            var remoteHash = await _networkDataProvider.GetContentHashAsync(activityId, contentId, fileModel.Id);
            var localHash = GetFileHash(activityId, contentId, fileModel.Id);
            return string.Equals(localHash, remoteHash);*/
        }

        private string GetFileHash(Guid activityId, Guid contentId, Guid fileId)
        {
            return GetMD5Checksum(GetFolderPath(activityId, contentId, fileId));
        }

        private static string GetMD5Checksum(string filename)
        {
            using var stream = File.OpenRead(filename);
            stream.Seek(0, SeekOrigin.Begin);
            using var md5Instance = MD5.Create();
            var hashResult = md5Instance.ComputeHash(stream);
            return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
        }

        private static List<FileModel> GetFilesToLoad(Content content)
        {
            var list = new List<FileModel>();

            if (content.Location.TargetMarker != null)
            {
                list.Add(content.Location.TargetMarker.Image);
            }

            switch (content.Type)
            {
                case ContentType.Unknown:
                case ContentType.Label:
                case ContentType.Action:
                case ContentType.Effects:
                case ContentType.Character:
                case ContentType.Interaction:
                case ContentType.Drawing:
                case ContentType.Instructor:
                    break;
                case ContentType.Image:
                    list.Add(((Content<ImageContentData>)content).ContentData.Image);
                    break;
                case ContentType.Video:
                    list.Add(((Content<VideoContentData>)content).ContentData.Video);
                    break;
                case ContentType.Audio:
                    list.Add(((Content<AudioContentData>)content).ContentData.Audio);
                    break;
                case ContentType.Ghost:
                    list.Add(((Content<GhostContentData>)content).ContentData.Audio);
                    break;
                case ContentType.Model:
                    var modelContent = (Content<ModelContentData>)content;
                    if (modelContent.ContentData.Model != null)
                    {
                        list.Add(((Content<ModelContentData>)content).ContentData.Model);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return list;
        }

        private static async UniTask<string> FilePathToBase64Async(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}