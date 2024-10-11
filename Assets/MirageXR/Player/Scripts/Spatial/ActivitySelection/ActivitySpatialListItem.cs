using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivitySpatialListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text textLabel;
        [SerializeField] private TMP_Text deadline;
        [SerializeField] private TMP_Text author;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonDelete;

        private Activity _activity;
        private UnityAction<Activity> _onItemClicked;
        private UnityAction<Activity> _onItemDeleteClicked;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked, UnityAction<Activity> onItemDeleteClicked)
        {
            _activity = activity;
            _onItemClicked = onItemClicked;
            _onItemDeleteClicked = onItemDeleteClicked;
            button.onClick.AddListener(OnItemClicked);
            buttonDelete.onClick.AddListener(OnItemDeleteClicked);
            
            UpdateView();
        }

        private void OnItemClicked()
        {
            _onItemClicked?.Invoke(_activity);
        }

        private void OnItemDeleteClicked()
        {
            _onItemDeleteClicked?.Invoke(_activity);
        }

        private void UpdateView()
        {
            if (_activity == null)
            {
                return;
            }

            gameObject.name = _activity.Name;
            textLabel.text = _activity.Name;
            author.text = _activity?.Creator?.Name;
        }
    }
}
