using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SessionContainerListItem : ListViewItem<LearningExperienceEngine.SessionContainer>
    {
        [SerializeField] private Text textLabel;
        [SerializeField] private Text sizeLabel;
        [SerializeField] private Text deadline;
        [SerializeField] private Text author;
        [SerializeField] private GameObject iconLocal;
        [SerializeField] private GameObject iconCloud;
        [SerializeField] private GameObject iconDownloaded;
        [SerializeField] private GameObject iconEdit;
        [SerializeField] private GameObject iconDelete;
        [SerializeField] private GameObject waitSpinner;
        [SerializeField] private GameObject iconError;

        private bool showError;

        public bool IsDownloading
        {
            get => Content.IsDownloading;
            set => Content.IsDownloading = value;
        }

        public override void ShowData(LearningExperienceEngine.SessionContainer data, int index)
        {
            base.ShowData(data, index);
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (Content == null)
            {
                return;
            }
            gameObject.name = Content.Name;
            textLabel.text = Content.Name;
            sizeLabel.text = Content.Filesize;
            deadline.text = Content.deadline;
            author.text = Content.author;
            waitSpinner.SetActive(Content.IsDownloading);
            iconLocal.SetActive(!Content.ExistsRemotely && Content.ExistsLocally && !Content.IsDownloading);
            iconDownloaded.SetActive(Content.ExistsRemotely && Content.ExistsLocally && !Content.IsDownloading);

            iconDelete.SetActive(iconDownloaded.activeInHierarchy || iconLocal.activeInHierarchy || Content.userIsOwner);
            // set the event of the delete button
            DeleteButtonEvent();

            iconCloud.SetActive(Content.ExistsRemotely && !Content.ExistsLocally && !Content.IsDownloading);
            iconEdit.SetActive(Content.IsEditable);
            iconError.SetActive(Content.HasError);
        }


        private void DeleteButtonEvent()
        {
            var deleteButton = iconDelete.GetComponent<Button>();
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.GetComponent<PressableButton>().ButtonPressed.RemoveAllListeners();

            var sessionButton = GetComponent<SessionButton>();

            if (iconDownloaded.activeInHierarchy)
            {
                deleteButton.onClick.AddListener(sessionButton.OnDeleteButtonPressed);
                deleteButton.GetComponent<PressableButton>().ButtonPressed.AddListener(sessionButton.OnDeleteButtonPressed);
            }
            else if (Content.userIsOwner)
            {
                deleteButton.onClick.AddListener(() => DeleteActivityDialog(Content));
                deleteButton.GetComponent<PressableButton>().ButtonPressed.AddListener(() => DeleteActivityDialog(Content));
            }
        }

        private static void DeleteActivityDialog(LearningExperienceEngine.SessionContainer activity)
        {
            // Confirm and delete
            DialogWindow.Instance.Show("Warning!",
                $"You are trying to delete activity \"{activity.Name}\" from the server. Are you sure?",
                new DialogButtonContent("Yes", () => DeleteFromServer(activity)),
                new DialogButtonContent("No"));
        }

        private static async void DeleteFromServer(LearningExperienceEngine.SessionContainer activity)
        {
            var result = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.DeleteArlem(activity.ItemID, activity.FileIdentifier);
            if (result)
            {
                var sessionListView = FindObjectOfType<SessionListView>();
                if (sessionListView) { sessionListView.RefreshActivityList(); }
            }
        }
    }
}