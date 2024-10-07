using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class ContentManager : IContentManager
    {
        private IAssetsManager _assetsManager;
        private IStepManager _stepManager;

        private readonly UnityEventContents _onContentActivated = new();

        private readonly List<Content> _contents = new();
        private List<Content> _activeContent;

        public event UnityAction<List<Content>> OnContentActivated
        {
            add
            {
                _onContentActivated.AddListener(value);
                if (_activeContent != null)
                {
                    value(_activeContent);
                }
            }
            remove => _onContentActivated.RemoveListener(value);
        }

        public UniTask InitializeAsync(IAssetsManager assetsManager, IStepManager stepManager, IActivityManager activityManager)
        {
            _assetsManager = assetsManager;
            _stepManager = stepManager;
            return UniTask.CompletedTask;
        }

        public async UniTask LoadContentAsync(Activity activity)
        {
            foreach (var content in activity.Content)
            {
                await _assetsManager.PrepareContent(activity.Id, content);
                _contents.Add(content);
            }
        }

        public List<Content> GetContents()
        {
            return _contents;
        }

        public void Reset()
        {
            _contents.Clear();
        }

        public void ShowContent(Guid currentStepId)
        {
            _activeContent = _contents.Where(content => content.Steps.Contains(currentStepId)).ToList();
            _onContentActivated.Invoke(_activeContent);
        }

        public void UpdateContent(Content content)
        {
            for (var i = 0; i < _contents.Count; i++)
            {
                if (_contents[i].Id == content.Id)
                {
                    _contents[i] = content;
                }
            }
        }

        public void AddContent(Content content)
        {
            _contents.Add(content);
            if (content.Steps.Contains(_stepManager.CurrentStep.Id))
            {
                ShowContent(_stepManager.CurrentStep.Id);
            }
        }

        /*public Content CreateContent()
        {
            return new Content<ImageContentData>
            {
                Type = ContentType.Image,
                Id = Guid.NewGuid(),
                Location = new Location
                {
                    Position = Vector3.up,
                    Rotation = Quaternion.identity.eulerAngles,
                    Scale = Vector3.one,
                    TargetMarker = null,
                },
                IsVisible = true,
                Steps = new List<Guid>(),
                ContentData = new ImageContentData
                {
                    Image = new FileModel
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        Version = Application.version,
                        //Name = "Image example",
                        FileHash = HashCode.Combine(Guid.NewGuid(), Guid.NewGuid()).ToString()  //temp
                    }
                },
                CreationDate = DateTime.UtcNow,
                Version = Application.version,
            };
        }*/
    }
}