using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class SessionListView : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;

        private ListView<LearningExperienceEngine.SessionContainer> listView;

        public Dictionary<string, LearningExperienceEngine.SessionContainer> CollectedContainers { get; private set; }
        public List<LearningExperienceEngine.SessionContainer> AllItems { get; private set; }

        public List<LearningExperienceEngine.SessionContainer> DisplayedItems
        {
            get => listView.Items;
            set => listView.Items = value;
        }

        public List<LearningExperienceEngine.SessionContainer> PageItems { get; private set; }

        private int pageStart = 0;
        public int sessionsOnPage = 5;
        private int itemListCount = 0;

        private void Awake()
        {
            listView = new ListView<LearningExperienceEngine.SessionContainer>(transform, itemPrefab);
        }

        private async void Start()
        {
            if (RootObject.Instance.PlatformManager.WorldSpaceUi)
            {
                RefreshActivityList();
                await Task.Delay(1);
            }
        }

        public async Task CollectAvailableSessionsAsync()
        {
            CollectedContainers = new Dictionary<string, LearningExperienceEngine.SessionContainer>();

            // the local activities should be loaded as the first items
            List<LearningExperienceEngine.Activity> activities = await LearningExperienceEngine.LocalFiles.GetDownloadedActivities();
            CollectedContainers = AddActivitiesToDictionary(CollectedContainers, activities);

            // the records on the server should be shown after the local records
            List<LearningExperienceEngine.Session> sessions = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.GetArlemList();
            if (sessions != null)
                CollectedContainers = AddSessionsToDictionary(CollectedContainers, sessions);

            AllItems = CollectedContainers.Values.ToList();

            SetAllItems();
        }


        public async void RefreshActivityList()
        {
            await CollectAvailableSessionsAsync();
        }


        public void UpdateView(int sessions)
        {
            // if the number of the all items is less more than  sessions(number of items on each page)
            if (itemListCount > sessions)
                DisplayedItems = PageItems.GetRange(pageStart, sessions);
            else
                DisplayedItems = PageItems.GetRange(pageStart, itemListCount);

            listView.UpdateView();
        }

        private Dictionary<string, LearningExperienceEngine.SessionContainer> AddSessionsToDictionary(Dictionary<string, LearningExperienceEngine.SessionContainer> collectedContainers, List<LearningExperienceEngine.Session> sessions)
        {

            for (int i = 0; i < sessions.Count; i++)
            {
                string key = sessions[i].sessionid;
                if (collectedContainers.ContainsKey(key))
                {
                    collectedContainers[key].Session = sessions[i];
                }
                else
                {
                    collectedContainers.Add(key, new LearningExperienceEngine.SessionContainer() { Session = sessions[i] });
                }
            }
            return collectedContainers;
        }

        private Dictionary<string, LearningExperienceEngine.SessionContainer> AddActivitiesToDictionary(Dictionary<string, LearningExperienceEngine.SessionContainer> collectedContainers, List<LearningExperienceEngine.Activity> activities)
        {
            for (int i = 0; i < activities.Count; i++)
            {
                string key = activities[i].id;
                if (collectedContainers.ContainsKey(key))
                {
                    collectedContainers[key].Activity = activities[i];
                }
                else
                {
                    collectedContainers.Add(key, new LearningExperienceEngine.SessionContainer() { Activity = activities[i] });
                }
            }
            return collectedContainers;
        }


        public async void ReloadActivityList()
        {
            await CollectAvailableSessionsAsync();
        }


        public void SetAllItems()
        {
            PageItems = AllItems;
            itemListCount = PageItems.Count();
            UpdateView(sessionsOnPage);
            // resets the shown sessions to be all loaded sessions
        }

        public void SetSearchedItems(List<LearningExperienceEngine.SessionContainer> currentItems)
        {
            PageItems = currentItems;
            itemListCount = PageItems.Count();
            UpdateView(sessionsOnPage);
            // sets shown sessions to a given list
        }

        public void NextPage()
        {
            if ((pageStart + sessionsOnPage) < itemListCount)
            {
                pageStart += sessionsOnPage;
                // moves to the next page provided that it is within the bounds of the current session list

                if ((pageStart + sessionsOnPage) > itemListCount)
                {
                    int sessions = itemListCount - pageStart;
                    UpdateView(sessions);
                    // if there are less sessions lest in a session list than the given sessions per page then only show the sessions that are left in the list

                }
                else
                {
                    UpdateView(sessionsOnPage);
                    // show the page
                }
            }
        }

        public void PrevPage()
        {
            if (pageStart > 0)
            {
                pageStart -= sessionsOnPage;

                UpdateView(sessionsOnPage);
            }
        }
        // moves to previous page
    }
}