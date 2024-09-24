using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using Unity.SharpZipLib.Zip;
using UnityEngine;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

namespace MirageXR.NewDataModel
{
    public class AssetsManager : IAssetsManager
    {
        private const string ZipExtension = ".zip";

        private INetworkDataProvider _networkDataProvider;

        public UniTask InitializeAsync(INetworkDataProvider networkDataProvider)
        {
            _networkDataProvider = networkDataProvider;
            return UniTask.CompletedTask;
        }

        public async UniTask PrepareContent(Guid activityId, Content content)
        {
            var list = GetFilesToLoad(content);

            foreach (var fileModel in list)
            {
                if (!await IsHashEqual(activityId, content.Id, fileModel))
                {
                    _networkDataProvider.DownloadContentAsync(activityId, content.Id, fileModel.Id);
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

            await using (var stream = new FileStream(folderPath, FileMode.OpenOrCreate))
            await using (var zipStream = new ZipOutputStream(stream))
            {
                await ZipUtilities.CompressFolderAsync(GetFilePath(activityId, contentId, fileId), zipStream);
            }

            return new FileModel
            {
                Id = fileId,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                FileHash = GetFileHash(activityId, contentId, fileId)
            };
        }

        private bool IsFileExists(Guid activityId, Guid contentId, Guid fileId)
        {
            return File.Exists(GetFilePath(activityId, contentId, fileId) + ZipExtension);
        }

        private async UniTask<bool> IsHashEqual(Guid activityId, Guid contentId, FileModel fileModel)
        {
            if (IsFileExists(activityId, contentId, fileModel.Id))
            {
                return false;
            }

            var remoteHash = await _networkDataProvider.GetContentHashAsync(activityId, contentId, fileModel.Id);
            var localHash = GetFileHash(activityId, contentId, fileModel.Id);
            return string.Equals(localHash, remoteHash);
        }

        public string GetFilePath(Guid activityId, Guid contentId, Guid fileId)
        {
            return Path.Combine(Application.persistentDataPath, activityId.ToString(), contentId.ToString(), fileId.ToString());
        }

        private string GetFileHash(Guid activityId, Guid contentId, Guid fileId)
        {
            return CalculateMD5(GetFilePath(activityId, contentId, fileId));
        }

        private static string CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            return Encoding.Default.GetString(md5.ComputeHash(stream));
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
    }
}