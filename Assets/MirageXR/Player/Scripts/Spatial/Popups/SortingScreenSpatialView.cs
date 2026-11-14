using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SortingScreenSpatialView : PopupBase
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Toggle _toggleGroupByTags;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.onClick.AddListener(Close);
            if (_toggleGroupByTags != null)
            {
                _toggleGroupByTags.isOn = LearningExperienceEngine.UserSettings.groupByTags;
                _toggleGroupByTags.onValueChanged.AddListener(OnGroupByTagsChanged);
            }
        }

        private void OnGroupByTagsChanged(bool value)
        {
            LearningExperienceEngine.UserSettings.groupByTags = value;
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync().Forget();
        }
    }
}
