using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListItem_v2 : MonoBehaviour
    {
        private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";

        private static ActivityManager activityManager => RootObject.Instance.activityManager;

        [SerializeField] private TMP_Text _txtLabel;
        [SerializeField] private TMP_Text _txtDeadline;
        [SerializeField] private TMP_Text _txtAuthor;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _defaultThumbnail;
        [SerializeField] private Button _btnMain;
        [SerializeField] private OpenActivityModeSelect activityModeSelect;


        private SessionContainer _container;
        private bool _interactable = true;

        public string activityName => _container.Name;

        public string activityAuthor => _container.author;

        public bool interactable
        {
            get
            {
                return _interactable;
            }

            set
            {
                _interactable = value;
            }
        }

        public void Init(SessionContainer container)
        {
            _container = container;
            _btnMain.onClick.AddListener(OnBtnMain);
            //_btnDelete.onClick.AddListener(OnBtnDelete);

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
            var result = await RootObject.Instance.moodleManager.DeleteArlem(_container.ItemID, _container.FileIdentifier);
            if (result)
            {
                RootView.Instance.activityListView.UpdateListView();
            }
        }

        private void DeleteLocal()
        {
            if (_container.Activity == null) return;

            if (LocalFiles.TryDeleteActivity(_container.Activity.id))
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
            PopupsViewer.Instance.Show(activityModeSelect, this);
        }

        public async void OpenActivity(bool value)
        {
            await PlayActivityAsync();
            activityManager.EditModeActive = value;
        }

        private async Task PlayActivityAsync()
        {
            LoadView.Instance.Show();
            var activityJsonFileName = LocalFiles.GetActivityJsonFilename(_container.FileIdentifier);
            await RootObject.Instance.editorSceneService.LoadEditorAsync();
            await RootObject.Instance.moodleManager.UpdateViewsOfActivity(_container.ItemID);
            await RootObject.Instance.activityManager.LoadActivity(activityJsonFileName);
            LoadView.Instance.Hide();
        }

        private async Task DownloadActivityAsync()
        {
            _container.HasError = false;
            _container.IsDownloading = true;
            UpdateView();

            LoadView.Instance.Show();
            var (result, activity) = await MoodleManager.DownloadActivity(_container.Session);
            LoadView.Instance.Hide();

            _container.HasError = !result;
            _container.Activity = activity;
            _container.IsDownloading = false;
            Toast.Instance.Show(result ? "Download succeeded." : "Something went wrong.");
            UpdateView();
        }
    }
}