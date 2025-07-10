using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
 
namespace MirageXR.View
{
    public class InstructorContentView : ContentView
    {
        private Content<InstructorContentData> _instructorContent;
        private GameObject _instructor;

        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);

            if (content is Content<InstructorContentData> instructorContent)
            {
                await InitializeContentAsync(instructorContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<instructorContent>");
            }
        }

        protected override async UniTask OnContentUpdatedAsync(Content content)
        {
            if (content is not Content<InstructorContentData> newContent || Content is not Content<InstructorContentData> oldContent)
            {
                return;
            }

            if (newContent.ContentData.CharacterName != oldContent.ContentData.CharacterName ||             //TODO: add add support for reinitializing an existing instructor 
                newContent.ContentData.AnimationClip != oldContent.ContentData.AnimationClip ||             //
                newContent.ContentData.SpeechToTextModel != oldContent.ContentData.SpeechToTextModel ||     //
                newContent.ContentData.LanguageModel != oldContent.ContentData.LanguageModel ||             //
                newContent.ContentData.Prompt != oldContent.ContentData.Prompt)                             //
            {
                Destroy(_instructor);
                Initialized = false;
                Initialized = await InitializeContentAsync(newContent);
            }

            await base.OnContentUpdatedAsync(content);
        }

        private async UniTask<bool> InitializeContentAsync(Content<InstructorContentData> content)
        {
            _instructorContent = content;
            string prefabPath;

            if (_instructorContent.ContentData.UseReadyPlayerMe)
            {
                prefabPath = "ReadyPlayerMe/AvatarContainer";
                var handle = Addressables.LoadAssetAsync<GameObject>(prefabPath);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var avatarContainer = Instantiate(handle.Result, transform);
                    _instructor = avatarContainer;
                    avatarContainer.transform.localPosition = Vector3.zero;
					var initializator = avatarContainer.AddComponent<VirtualInstructorRPMInitializer>();
					var avatarLoader = avatarContainer.GetComponent<AvatarLoader>();
                    avatarLoader.LoadDefaultAvatarOnStart = false;
					var tcs = new TaskCompletionSource<bool>();
					void OnAvatarLoaded(bool res)
                    {
						tcs.SetResult(res);
						var instructor = avatarContainer.AddComponent<Instructor>();
						instructor.Initialize(content);
						avatarLoader.AvatarLoaded -= OnAvatarLoaded;
					}
                    avatarLoader.AvatarLoaded += OnAvatarLoaded;
                    avatarLoader.LoadAvatar(_instructorContent.ContentData.CharacterModelUrl);
                    await tcs.Task;
                    return tcs.Task.Result;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_instructorContent.ContentData.CharacterName)) //TODO: temp
                {                                                                       //
                    _instructorContent.ContentData.CharacterName = "Sara";              //
                }                                                                       //

                prefabPath = $"Instructors/{_instructorContent.ContentData.CharacterName}_instructor";
                var handle = Addressables.LoadAssetAsync<GameObject>(prefabPath);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
					_instructor = Instantiate(handle.Result, transform);
                    var oldInstructor = _instructor.GetComponent<VirtualInstructor>();
                    if (oldInstructor != null)
                    {
                        Destroy(oldInstructor);
                    }

                    var instructor = _instructor.AddComponent<Instructor>();
                    instructor.Initialize(_instructorContent);
                    return true;
                }
            }

            Debug.LogError("FATAL ERROR: Could not instantiate ContentAugmentation prefab " + prefabPath);
            return false;
        }

        protected override void InitializeBoxCollider() { }

        protected override void InitializeManipulator() { }
    }
}
