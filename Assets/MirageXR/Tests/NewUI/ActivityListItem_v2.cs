using System.Threading.Tasks;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListItem_v2 : MonoBehaviour
    {
        [SerializeField] private TMP_Text _txtLabel;
        [SerializeField] private TMP_Text _txtDeadline;
        [SerializeField] private TMP_Text _txtAuthor;
        [SerializeField] private Image _imgSmall;
        [SerializeField] private Button _btnMain;


        private SessionContainer _container;
        private bool _interactable = true;

        public string activityName => _container.Name;

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

        private void OnBtnDelete()
        {
            if (_container.ExistsLocally)
            {
                DeleteLocal();
                return;
            }

            if (_container.userIsOwner)
            {
                DialogWindow.Instance.Show($"You are trying to delete activity \"{_container.Name}\" from the server. Are you sure?",
                    new DialogButtonContent("Yes", DeleteFromServer),
                    new DialogButtonContent("No"));
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
            if (!_container.ExistsLocally) await DownloadActivityAsync();
            else await PlayActivityAsync();
            _interactable = true;
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

            var (result, activity) = await MoodleManager.DownloadActivity(_container.Session);

            _container.HasError = !result;
            _container.Activity = activity;
            _container.IsDownloading = false;
            Toast.Instance.Show(result ? "Download succeeded." : "Something went wrong.");
            UpdateView();
        }
    }
}