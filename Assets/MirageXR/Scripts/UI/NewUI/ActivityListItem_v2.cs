using System.IO;
using System.Threading.Tasks;
//using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
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

        private LearningExperienceEngine.SessionContainer _container;
        private bool _interactable = true;

        public string activityName => _container.Name;

        public string activityAuthor => _container.author;

        public bool userIsAuthor => _container.userIsOwner;

        public bool userIsEnroled => _container.hasDeadline;

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

        public void Init(LearningExperienceEngine.SessionContainer container)
        {
            _container = container;
            _btnMain.onClick.AddListener(OnBtnMain);

            UpdateView();
        }

        private void UpdateView()
        {
            _txtLabel.text = _container.Name;
            _txtDeadline.text = _container.deadline;
            _txtAuthor.text = _container.author;

            var isLocal = !_container.ExistsRemotely && _container.ExistsLocally && !_container.IsDownloading;
            var isDownloaded = _container.ExistsRemotely && _container.ExistsLocally && !_container.IsDownloading;
            var isOnClouds = _container.ExistsRemotely && !_container.ExistsLocally && !_container.IsDownloading;
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
            if (_container.ExistsLocally)
            {
                DeleteLocal();
                return;
            }

            if (_container.userIsOwner)
            {
                RootView_v2.Instance.dialog.ShowMiddle(
                    "Warring!",
                    $"You are trying to delete activity \"{_container.Name}\" from the server. Are you sure?",
                    "Yes", DeleteFromServer,
                    "No", null);
            }
        }

        private async void DeleteFromServer()
        {
            var result = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.DeleteArlem(_container.ItemID, _container.FileIdentifier);
            if (result)
            {
                RootView.Instance.activityListView.UpdateListView();
            }
        }

        private void DeleteLocal()
        {
            if (_container.Activity == null) return;

            if (LearningExperienceEngine.LocalFiles.TryDeleteActivity(_container.Activity.id))
            {
                if (_container.ExistsRemotely)
                {
                    _container.Activity = null;
                    UpdateView();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private async void OnBtnMain()
        {
            _interactable = false;
            if (!_container.ExistsLocally)
            {
                await DownloadActivityAsync();
            }
            else
            {
                ShowPopup();
            }

            _interactable = true;
        }

        private void ShowPopup()
        {
            RootView_v2.Instance.dialog.ShowMiddleMultiline("Open Activity", true,
                ("Open to edit", () => OpenActivity(true), false),
                ("Open to view", () => OpenActivity(false), false),
                ("Cancel", null, true));
        }

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

        private async Task PlayActivityAsync()
        {
            LoadView.Instance.Show();
            var activityJsonFileName = LearningExperienceEngine.LocalFiles.GetActivityJsonFilename(_container.FileIdentifier);
            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.UpdateViewsOfActivity(_container.ItemID, _container.ExistsRemotely);
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity(activityJsonFileName);
            LoadView.Instance.Hide();
        }

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
        }
    }
}
