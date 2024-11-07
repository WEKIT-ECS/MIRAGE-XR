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
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonSorting;
        [SerializeField] private Button _buttonAddNewActivity;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _searchField;
        [Header("Container")]
        [SerializeField] private Transform _activityContainer;
        [Header("Prefabs")]
        [SerializeField] private ActivitySpatialListItem _activityListItemPrefab;
        
        public void SetActionOnButtonSortingClick(UnityAction action) => _buttonSorting.SafeSetListener(action);
        public void SetActionOnButtonAddNewActivityClick(UnityAction action) => _buttonAddNewActivity.SafeSetListener(action);
        public void SetActionOnInputFieldSearchValueChanged(UnityAction<string> action) => _searchField.SafeSetListener(action);

        public Transform GetActivityContainer()
        {
            return _activityContainer;
        }

        public ActivitySpatialListItem GetActivityListItemPrefab()
        {
            return _activityListItemPrefab;
        }
    }
}
