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

        private Activity _activity;
        private UnityAction<Activity> _onItemClicked;

        public void Initialize(Activity activity, UnityAction<Activity> onItemClicked)
        {
            _activity = activity;
            _onItemClicked = onItemClicked;
            button.onClick.AddListener(OnItemClicked);
            
            UpdateView();
        }

        private void OnItemClicked()
        {
            _onItemClicked?.Invoke(_activity);
        }
        
        private void UpdateView()
        {
            if (_activity == null)
            {
                return;
            }

            gameObject.name = _activity.Name;
            textLabel.text = _activity.Name;
            author.text = _activity.Creator.Name;
        }
    }
}
