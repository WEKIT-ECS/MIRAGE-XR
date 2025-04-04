using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DTOs;
//using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListItem_v2 : MonoBehaviour
    {
        private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";

        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        [SerializeField] private TMP_Text _txtLabel;
        [SerializeField] private TMP_Text _txtDeadline;
        [SerializeField] private TMP_Text _txtAuthor;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _defaultThumbnail;
        [SerializeField] private Button _btnMain;

        private bool _interactable = true;
        private Activity _activity;
        private UnityAction<Activity> _onClick;
        private UnityAction<Activity> _onDelete;

        public string activityName => _activity.Name;

        public string activityAuthor => _activity.Creator.Name;

        public bool userIsAuthor => false; //TODO: _activity.Creator.Id == RootObject.Instance.LEE.AuthorizationManager.UserId;

        public bool userIsEnroled => false;//_container.hasDeadline;

        public Button BtnMain => _btnMain;

        /*public void Init(Activity activity)
        {
            _btnMain.onClick.AddListener(() =>
            {
                RootObject.Instance.ActivityManager.LoadActivityAsync(activity.Id);
            });

            _txtLabel.text = _container.Name;
            _txtDeadline.text = _container.deadline;
            _txtAuthor.text = _container.author;
        }*/

        public void Init(Activity activity, UnityAction<Activity> onClick, UnityAction<Activity> onDelete)
        {
            _onClick = onClick;
            _onDelete = onDelete;
            _activity = activity;
            _btnMain.onClick.AddListener(OnBtnMain);

            UpdateView();
        }

        private void UpdateView()
        {
            _txtLabel.text = _activity.Name;
            //_txtDeadline.text = _container.deadline;
            _txtAuthor.text = _activity.Creator.Name;
        }

        private void LoadThumbnail()
        {
            var path = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
            if (!File.Exists(path))
            {
                _image.sprite = _defaultThumbnail;
                return;
            }

            var texture = Utilities.LoadTexture(path);
            var sprite = Utilities.TextureToSprite(texture);
            _image.sprite = sprite;
        }

        private void OnBtnDelete()
        {
            _onDelete?.Invoke(_activity);
        }

        private void DeleteFromServer()
        {
            DeleteFromServerAsync().Forget();
        }

        private async UniTask DeleteFromServerAsync()
        {
            await RootObject.Instance.LEE.ActivityManager.DeleteActivityAsync(_activity.Id);
            RootView.Instance.activityListView.UpdateListView();
        }

        private void OnBtnMain()
        {
            _onClick.Invoke(_activity);
        }

        /*private void ShowPopup()
        {
            RootView_v2.Instance.dialog.ShowMiddleMultiline("Open Activity", true,
                ("Open to edit", () => OpenActivity(true), false),
                ("Open to view", () => OpenActivity(false), false),
                ("Cancel", null, true));
        }*/
/*
        public async void OpenActivity(bool value)
        {
            await PlayActivityAsync();
            RootView_v2.Instance.activityView.SetSessionInfo(_container);
            activityManager.EditModeActive = value;
            if (!value)
            {
                RootView_v2.Instance.activityView.stepsListView.SetCalibrationToggle(true);
            }
        }
*/
/*
        private async Task PlayActivityAsync()
        {
            LoadView.Instance.Show();
            var activityJsonFileName = LearningExperienceEngine.LocalFiles.GetActivityJsonFilename(_container.FileIdentifier);
            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.UpdateViewsOfActivity(_container.ItemID, _container.ExistsRemotely);
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity(activityJsonFileName);
            LoadView.Instance.Hide();
        }
*/
/*
        private async Task DownloadActivityAsync()
        {
            _container.HasError = false;
            _container.IsDownloading = true;
            UpdateView();

            LoadView.Instance.Show();
            var (result, activity) = await LearningExperienceEngine.MoodleManager.DownloadActivity(_container.Session);
            LoadView.Instance.Hide();

            _container.HasError = !result;
            _container.Activity = activity;
            _container.IsDownloading = false;
            string message = result ? "<color=#00000000>space</color>Downloaded.<color=#00000000>space</color>" : "Something went wrong.";
            Toast.Instance.Show(message, result);
            UpdateView();
        }*/
    }
}
