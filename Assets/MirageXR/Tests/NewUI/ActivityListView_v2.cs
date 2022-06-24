using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityListView_v2 : MonoBehaviour
    {
        public static ActivityListView_v2 Instance { get; private set; }

        [SerializeField] private Button _btnFilter;
        [SerializeField] private Transform _listTransform;
        [SerializeField] private ActivityListItem_v2 _smallItemPrefab;
        [SerializeField] private ActivityListItem_v2 _bigItemPrefab;

        [SerializeField] private SortingView _sortingPrefab;

        private List<SessionContainer> _content;
        private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();
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
            _btnFilter.onClick.AddListener(OnByDateClick);

            //_btnAddActivity.onClick.AddListener(OnAddActivityClick);
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
            await RootObject.Instance.moodleManager.Login(username, password);
            LoadView.Instance.Hide();
        }

        private static async Task<List<SessionContainer>> GetContent()
        {
            var dictionary = new Dictionary<string, SessionContainer>();

            var localList = await LocalFiles.GetDownloadedActivities();
            localList.ForEach(t =>
            {
                if (dictionary.ContainsKey(t.id))
                {
                    dictionary[t.id].Activity = t;
                }
                else
                {
                    dictionary.Add(t.id, new SessionContainer { Activity = t });
                }
            });

            var remoteList = await RootObject.Instance.moodleManager.GetArlemList();
            remoteList?.ForEach(t =>
            {
                if (dictionary.ContainsKey(t.sessionid))
                {
                    dictionary[t.sessionid].Session = t;
                }
                else
                {
                    dictionary.Add(t.sessionid, new SessionContainer { Session = t });
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

            ActivityListItem_v2 prefab;
            if (SortingView.SELECTED_CARD_SIZE == "small")
            {
                prefab = _smallItemPrefab;
            }
            else
            {
                prefab = _bigItemPrefab;
            }
            _content.ForEach(content =>
            {
                var item = Instantiate(prefab, _listTransform);
                item.Init(content);
                _items.Add(item);
            });
        }

        private void OnByDateClick()
        {
            PopupsViewer.Instance.Show(_sortingPrefab);
        }

        private async void OnAddActivityClick()
        {
            LoadView.Instance.Show();
            interactable = false;
            await RootObject.Instance.editorSceneService.LoadEditorAsync();
            RootObject.Instance.activityManager.CreateNewActivity();
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