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
                await InitializeContentAsync(audioContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<AudioContentData>");
            }
        }

        public override void Play()
        {
            base.Play();

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

        protected override void InitializeBoundsControl() { }

        private async UniTask InitializeContentAsync(Content<AudioContentData> content)
        {
            await InitializeAudioClipAsync(content);
        }

        private async UniTask InitializeAudioClipAsync(Content<AudioContentData> content)
        {
            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            var folderPath = RootObject.Instance.LEE.AssetsManager.GetContentFileFolderPath(activityId, content.Id, content.ContentData.Audio.Id);
            var filePath = Path.Combine(folderPath, ImageFileName);

            if (File.Exists(filePath))
            {
                _audioClip = await SaveLoadAudioUtilities.LoadAudioFileAsync(filePath);
                if (_audioClip == null)
                {
                    AppLog.LogError($"Failed to load audio file: {filePath}");
                }
            }
            else
            {
                Debug.LogError($"Audio file {filePath} does not exist");
            }
        }

        private void OnDestroy()
        {
            Destroy(_audioClip);
        }
    }
}