using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListView : MonoBehaviour
    {
        public static ActivityListView Instance { get; private set; }
        
        [SerializeField] private Button _btnLogin;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnAddActivity;
        [SerializeField] private TMP_InputField _inputFieldSearch;
        [SerializeField] private Transform _listTransform;
        [SerializeField] private ActivityListItem _listListItemPrefab;
        [SerializeField] private LoginView _loginViewPrefab;
        [SerializeField] private SettingsView _settingsViewPrefab;
        
        private List<SessionContainer> _content;
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

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{nameof(Instance.GetType)} must only be a single copy!");
                return;
            }
        
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Start()
        {
            Init();
        }

        private async void Init()
        {
            _btnLogin.onClick.AddListener(OnLoginClick);
            _btnSettings.onClick.AddListener(OnSettingsClick);
            _btnAddActivity.onClick.AddListener(OnAddActivityClick);
            _inputFieldSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);
            if (!DBManager.LoggedIn && DBManager.rememberUser)
            {
                await AutoLogin();
            }
            UpdateListView();
        }

        private async Task AutoLogin()
        {
            if (!LocalFiles.TryToGetUsernameAndPassword(out var username, out var password)) return;
        
            LoadView.Instance.Show();
            await MoodleManager.Instance.Login(username, password);
            LoadView.Instance.Hide();
        }

        private static async Task<List<SessionContainer>> GetContent()
        {
            var dictionary = new Dictionary<string, SessionContainer>();
            (await LocalFiles.GetDownloadedActivities()).ForEach(t =>
            {
                if (dictionary.ContainsKey(t.id)) dictionary[t.id].Activity = t;
                else dictionary.Add(t.id, new SessionContainer {Activity = t});
            });
            (await MoodleManager.Instance.GetArlemList()).ForEach(t =>
            {
                if (dictionary.ContainsKey(t.sessionid)) dictionary[t.sessionid].Session = t;
                else dictionary.Add(t.sessionid, new SessionContainer {Session = t});
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
                item.Init(content);
                _items.Add(item);
            });
        }

        private void OnSettingsClick()
        {
            PopupsViewer.Instance.Show(_settingsViewPrefab);
        }

        private void OnLoginClick()
        {
            PopupsViewer.Instance.Show(_loginViewPrefab);
        }

        private async void OnAddActivityClick()
        {
            interactable = false;
            await ServiceManager.GetService<EditorSceneService>().LoadEditorAsync();
            EventManager.ParseActivity(string.Empty);
            interactable = true;
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