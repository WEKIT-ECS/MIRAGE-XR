using System.IO;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class AudioContentView : ContentView
    {
        private const string ImageFileName = "audio.wav";

        [SerializeField] private AudioSource audioSource;

        private AudioClip _audioClip;

        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);

            if (content is Content<AudioContentData> audioContent)
            {
                Initialized = await InitializeContentAsync(audioContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<AudioContentData>");
            }
        }

        public override async UniTask PlayAsync()
        {
            await base.PlayAsync();

            PlayAudioClip();
        }

        private void PlayAudioClip()
        {
            if (_audioClip != null)
            {
                audioSource.PlayOneShot(_audioClip);
            }
        }
        
        protected override void InitializeBoxCollider() { }

        protected override void InitializeManipulator() { }

        protected override async UniTask OnContentUpdatedAsync(Content content)
        {
            if (content is not Content<AudioContentData> newContent || Content is not Content<AudioContentData> oldContent)
            {
                return;
            }

            if (newContent.ContentData.Audio.Id != oldContent.ContentData.Audio.Id)
            {
                Initialized = false;
                Initialized = await InitializeContentAsync(newContent);
            }

            await base.OnContentUpdatedAsync(content);
        }

        private async UniTask<bool> InitializeContentAsync(Content<AudioContentData> content)
        {
            return await InitializeAudioClipAsync(content);
        }

        private async UniTask<bool> InitializeAudioClipAsync(Content<AudioContentData> content)
        {
            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            var folderPath = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, content.Id, content.ContentData.Audio.Id);
            var filePath = Path.Combine(folderPath, ImageFileName);

            if (!File.Exists(filePath))
            {
                var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
                var result = await RootObject.Instance.LEE.AssetsManager.TryDownloadAssetUntilSuccessAsync(activityId, content.Id, content.ContentData.Audio.Id, cancellationToken);

                if (!result)
                {
                    Debug.LogError($"Audio file {filePath} does not exist");
                    return false;
                }
            }

            _audioClip = await SaveLoadAudioUtilities.LoadAudioFileAsync(filePath);
            if (_audioClip == null)
            {
                AppLog.LogError($"Failed to load audio file: {filePath}");
                return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            Destroy(_audioClip);
        }
    }
}