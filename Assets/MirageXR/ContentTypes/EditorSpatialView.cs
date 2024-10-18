using System;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public abstract class EditorSpatialView : PopupBase
    {
        [SerializeField] private Button _btnAccept;
        [SerializeField] private Button _btnClose;

        protected Content _content;
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _btnAccept.onClick.AddListener(OnAccept);
            _btnClose.onClick.AddListener(Close);
        }

        protected abstract void OnAccept();

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args is { Length: 1 } && args[0] is Content obj)
            {
                _content = obj;
            }

            return true;
        }
    }
}
