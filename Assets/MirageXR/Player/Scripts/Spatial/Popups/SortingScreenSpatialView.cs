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

            _btnClose?.onClick.AddListener(Close);
            if (_toggleGroupByTags == null)
            {
                var allToggles = GetComponentsInChildren<Toggle>(true);
                foreach (var toggle in allToggles)
                {
                    var parentText = toggle.transform.parent != null ? toggle.transform.parent.GetComponentInChildren<TMPro.TextMeshProUGUI>(true) : null;
                    if (parentText != null && parentText.text != null && parentText.text.IndexOf("Group by tags", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _toggleGroupByTags = toggle;
                        break;
                    }
                }

                if (_toggleGroupByTags == null)
                {
                    foreach (Transform t in GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == "ToggleText_Item (4)")
                        {
                            _toggleGroupByTags = t.GetComponentInChildren<Toggle>(true);
                            if (_toggleGroupByTags != null) break;
                        }
                    }
                }

                if (_toggleGroupByTags == null && allToggles.Length > 0)
                {
                    _toggleGroupByTags = allToggles[allToggles.Length - 1]; // ToggleText_Item (4) is the last toggle in Panel_Groups
                }
            }

            if (_toggleGroupByTags != null)
            {
                _toggleGroupByTags.group = null;
                _toggleGroupByTags.onValueChanged.RemoveListener(OnGroupByTagsChanged);
                _toggleGroupByTags.isOn = LearningExperienceEngine.UserSettings.groupByTags;
                _toggleGroupByTags.onValueChanged.AddListener(OnGroupByTagsChanged);
            }
        }

        private void OnGroupByTagsChanged(bool value)
        {
            LearningExperienceEngine.UserSettings.groupByTags = value;
            PlayerPrefs.Save();
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync().Forget();
        }
    }
}
