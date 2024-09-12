using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.NewDataModel
{
    public class AssetsManager : IAssetsManager
    {
        private INetworkDataProvider _networkDataProvider;

        public UniTask InitializeAsync(INetworkDataProvider networkDataProvider)
        {
            _networkDataProvider = networkDataProvider;
            return UniTask.CompletedTask;
        }

        public UniTask PrepareContent(Content content)
        {
            return UniTask.CompletedTask;
        }

        public File GetDefaultThumbnail()
        {
            return new File
            {
                Id = Guid.NewGuid(),
                Name = "Thumbnail",
                FileHash = HashCode.Combine(Guid.NewGuid(), Guid.NewGuid()).ToString(),  //temp,
                Version = Application.version,
                CreationDate = DateTime.UtcNow
            };
        }
    }
}