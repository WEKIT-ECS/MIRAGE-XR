using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListView : BaseView
    {
        [SerializeField] private Button _btnLogin;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnHelp;
        [SerializeField] private Button _btnAddActivity;
        [SerializeField] private TMP_InputField _inputFieldSearch;
        [SerializeField] private Transform _listTransform;
        [SerializeField] private ActivityListItem _listListItemPrefab;
        [SerializeField] private LoginView _loginViewPrefab;
        [SerializeField] private SettingsView _settingsViewPrefab;

        public Button BtnAddActivity => _btnAddActivity;

        private List<LearningExperienceEngine.SessionContainer> _content;
        private readonly List<ActivityListItem> _items = new List<ActivityListItem>();
        private bool _interactable = true;

        public bool interactable
        {
            get
            {
                return _interactable;
            }
            set
            {
                _interactable = value;
                _btnAddActivity.interactable = value;
                _items.ForEach(t => t.interactable = value);
            }
        }

        public override async void Initialization(BaseView parentView)
        {
            base.Initialization(parentView);
            _btnLogin.onClick.AddListener(OnLoginClick);
            _btnSettings.onClick.AddListener(OnSettingsClick);
            _btnAddActivity.onClick.AddListener(OnAddActivityClick);
            _inputFieldSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);
            if (!LearningExperienceEngine.UserSettings.LoggedIn && LearningExperienceEngine.UserSettings.rememberUser)
            {
                await AutoLogin();
            }
            UpdateListView();
        }

        private async Task AutoLogin()
        {
            if (!LearningExperienceEngine.UserSettings.TryToGetUsernameAndPassword(out var username, out var password)) return;

            LoadView.Instance.Show();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.Login(username, password);
            LoadView.Instance.Hide();
        }

        private static async Task<List<LearningExperienceEngine.SessionContainer>> GetContent()
        {
            var dictionary = new Dictionary<string, LearningExperienceEngine.SessionContainer>();

            var localList = await LearningExperienceEngine.LocalFiles.GetDownloadedActivities();
            localList.ForEach(t =>
            {
                if (dictionary.ContainsKey(t.id))
                {
                    dictionary[t.id].Activity = t;
                }
                else
                {
                    dictionary.Add(t.id, new LearningExperienceEngine.SessionContainer { Activity = t });
                }
            });

            var remoteList = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.GetArlemList();
            remoteList?.ForEach(t =>
            {
                if (dictionary.ContainsKey(t.sessionid))
                {
                    dictionary[t.sessionid].Session = t;
                }
                else
                {
                    dictionary.Add(t.sessionid, new LearningExperienceEngine.SessionContainer { Session = t });
                }
            });

            return dictionary.Values.ToList();
        }

        public async void UpdateListView()
        {
            _content = await GetContent();
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return;
#endif
            _items.ForEach(item => Destroy(item.gameObject));
            _items.Clear();
            _content.ForEach(content =>
            {
                var item = Instantiate(_listListItemPrefab, _listTransform);
                item.Initialization(content);
                _items.Add(item);
            });
        }

        private void OnSettingsClick()
        {
            PopupsViewer.Instance.Show(_settingsViewPrefab);
        }

        private void OnLoginClick()
        {
            var dontShowLoginMenu = false;
            PopupsViewer.Instance.Show(_loginViewPrefab, dontShowLoginMenu);
        }

        private async void OnAddActivityClick()
        {
            LoadView.Instance.Show();
            interactable = false;
            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.CreateNewActivity();
            interactable = true;
            LoadView.Instance.Hide();
            EventManager.NotifyOnNewActivityCreationButtonPressed();
        }

        private void OnInputFieldSearchChanged(string text)
        {
            foreach (var item in _items)
            {
                var enable = string.IsNullOrEmpty(text) || item.activityName.ToLower().Contains(text.ToLower());
                item.gameObject.SetActive(enable);
            }
        }
    }
}