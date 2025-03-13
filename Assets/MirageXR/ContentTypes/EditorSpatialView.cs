using System;
using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public abstract class EditorSpatialView : PopupBase
    {
        [SerializeField] private Button _btnAccept;
        [SerializeField] private Button _btnClose;

        protected Content Content;
        protected bool IsContentUpdate;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _btnAccept?.onClick.AddListener(OnAccept);
            _btnClose?.onClick.AddListener(Close);
        }

        protected abstract void OnAccept();

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args is { Length: 1 } && args[0] is Content obj)
            {
                Content = obj;
                IsContentUpdate = true;
            }

            return true;
        }

        protected Content<T> CreateContent<T>(ContentType type) where T : ContentData, new()
        {
            if (IsContentUpdate)
            {
                if (Content is not Content<T> content)
                {
                    return null;
                }

                var copy = content.ShallowCopy();
                copy.ContentData = new T();
                return copy;
            }

            var step = RootObject.Instance.LEE.StepManager.CurrentStep;
            return new Content<T>
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                Steps = new List<Guid> { step.Id },
                Type = type,
                ContentData = new T(),
                Location = Location.GetIdentityLocation()
            };
        }
    }
}
