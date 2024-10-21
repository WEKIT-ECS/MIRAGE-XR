using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirageXR.View
{
    public class InstructorContentView : ContentView
    {
        private Content<InstructorContentData> _instructorContent;
        private Instructor _instructor;

        public override async UniTask InitializeAsync(Content content)
        {
            base.InitializeAsync(content);

            if (content is Content<InstructorContentData> instructorContent)
            {
                await InitializeContentAsync(instructorContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<instructorContent>");
            }
        }

        private async UniTask InitializeContentAsync(Content<InstructorContentData> content)
        {
            _instructorContent = content;

            if (string.IsNullOrEmpty(_instructorContent.ContentData.CharacterName)) //TODO: temp
            {                                                                       //
                _instructorContent.ContentData.CharacterName = "Sara";              //
            }                                                                       //
            
            var prefabPath = $"Instructors/{_instructorContent.ContentData.CharacterName}_instructor";
            var handle = Addressables.LoadAssetAsync<GameObject>(prefabPath);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instructor = Instantiate(handle.Result, transform);
                var oldInstructor = instructor.GetComponent<VirtualInstructor>();
                if (oldInstructor != null)
                {
                    Destroy(oldInstructor);
                }

                _instructor = instructor.GetComponent<Instructor>();
                _instructor.Initialize(_instructorContent);
            }
            else
            {
                Debug.LogError("FATAL ERROR: Could not instantiate ContentAugmentation prefab " + prefabPath);
            }
        }

        protected override void InitializeBoxCollider() { }

        protected override void InitializeManipulator() { }

        protected override void InitializeBoundsControl() { }
    }
}
