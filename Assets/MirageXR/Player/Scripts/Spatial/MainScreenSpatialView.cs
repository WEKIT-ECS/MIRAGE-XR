using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.MVC;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class MainScreenSpatialView : ScreenView
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonSidebarCollapse;
        [SerializeField] private Button _buttonSidebarExpand;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonSorting;
        [SerializeField] private Button _buttonAddNewActivity;
        [Header("GameObjects")]
        [SerializeField] private GameObject _sidebarOpened;
        [SerializeField] private GameObject _sidebarClosed;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _searchField;

#if UNITY_VISIONOS
        public void SetActionOnButtonSidebarCollapseClick(UnityAction action) => _buttonSidebarCollapse.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonSidebarExpandClick(UnityAction action) => _buttonSidebarExpand.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonSortingClick(UnityAction action) => _buttonSorting.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonAddNewActivityClick(UnityAction action) => _buttonAddNewActivity.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);

#else
        public void SetActionOnButtonSidebarCollapseClick(UnityAction action) => _buttonSidebarCollapse.SafeSetListener(action);
        public void SetActionOnButtonSidebarExpandClick(UnityAction action) => _buttonSidebarExpand.SafeSetListener(action);
        public void SetActionOnButtonSortingClick(UnityAction action) => _buttonSorting.SafeSetListener(action);
        public void SetActionOnButtonAddNewActivityClick(UnityAction action) => _buttonAddNewActivity.SafeSetListener(action);
#endif

		public void SetActionOnInputFieldSearchValueChanged(UnityAction<string> action) => _searchField.SafeSetListener(action);

    }
}
