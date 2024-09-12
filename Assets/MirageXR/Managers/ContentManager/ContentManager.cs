using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class ContentManager : IContentManager
    {
        private IAssetsManager _assetsManager;

        private readonly UnityEventContents _onContentActivated = new();

        private readonly List<Content> _contents = new();
        private List<Content> _activeContent;

        public event UnityAction<List<Content>> OnContantActivated
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

        public UniTask InitializeAsync(IAssetsManager assetsManager)
        {
            _assetsManager = assetsManager;
            return UniTask.CompletedTask;
        }

        public async UniTask LoadContentAsync(Activity activity)
        {
            foreach (var content in activity.Content)
            {
                await _assetsManager.PrepareContent(content);
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

        public void ShowContent(ActivityStep currentStep)
        {
            _activeContent = _contents.Where(content => content.Steps.Contains(currentStep.Id)).ToList();
            _onContentActivated.Invoke(_activeContent);
        }

        public void AddContent(Content content)
        {
            _contents.Add(content);
        }

        public Content CreateContent()
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
                Name = "Image example",
                IsVisible = true,
                Steps = new List<Guid>(),
                ContentData = new ImageContentData
                {
                    Image = new File
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        Version = Application.version,
                        Name = "Image example",
                        FileHash = HashCode.Combine(Guid.NewGuid(), Guid.NewGuid()).ToString()  //temp
                    } 
                },
                CreationDate = DateTime.UtcNow,
                Version = Application.version,
            };
        }
    }
}