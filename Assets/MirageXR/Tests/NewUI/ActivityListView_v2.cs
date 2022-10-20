using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
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

        [SerializeField] private Toggle _toggleNewActivity;

        [SerializeField] private SortingView _sortingPrefab;

        [SerializeField] private StepsListView_v2 _stepsListView;

        private List<SessionContainer> _content;
        private readonly List<ActivityListItem_v2> _items = new List<ActivityListItem_v2>();
        private bool _interactable = true;

        public List<SessionContainer> content 
        {
            get
            {
                return _content;
            }
        }

        public List<ActivityListItem_v2> items
        {
            get
            {
                return _items;
            }
        }

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

        private void Init()
        {
            _btnFilter.onClick.AddListener(OnByDateClick);
            _toggleNewActivity.onValueChanged.AddListener(OnNewActivityChanged);

            EventManager.OnActivityStarted += UpdateStepsView;
            EventManager.OnActivitySaved += FetchAndUpdateView;

            FetchAndUpdateView();
        }

        private static async Task<List<SessionContainer>> FetchContent()
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

        public async void FetchAndUpdateView()
        {
            _content = await FetchContent();
            UpdateView();
        }

        public void UpdateView()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return;
#endif
            _items.ForEach(item => Destroy(item.gameObject));
            _items.Clear();

            var prefab = !DBManager.showBigCards ? _smallItemPrefab : _bigItemPrefab;
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

        private async void OnNewActivityChanged(bool value)
        {
            LoadView.Instance.Show();
            interactable = false;
            await RootObject.Instance.editorSceneService.LoadEditorAsync();
            RootObject.Instance.activityManager.CreateNewActivity();
            interactable = true;
            LoadView.Instance.Hide();
            EventManager.NotifyOnNewActivityCreationButtonPressed();
        }

        private void UpdateStepsView()
        {
            _stepsListView.UpdateView();
        }
    }
}