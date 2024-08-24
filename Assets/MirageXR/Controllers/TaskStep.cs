using LearningExperienceEngine;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class TaskStep : MonoBehaviour
    {
        private LearningExperienceEngine.Action _actionObject;

        private Text _title;
        private Image _titleBg;

        private GameObject _featureContainer;
        private GameObject _checkboxes;

        private GameObject _voice;
        private GameObject _touch;
        private GameObject _iot;

        private GameObject _completedIcon;
        private GameObject _incompletedIcon;

        public bool IsActive;

        [SerializeField] private Color32 ActiveColor;
        [SerializeField] private Color32 InactiveColor;

        public LearningExperienceEngine.Action Get_actionObject()
        {
            return _actionObject;
        }

        private void OnEnable()
        {
            EventManager.OnClearAll += Clear;
            EventManager.OnPlayerReset += Reset;
        }

        private void OnDisable()
        {
            EventManager.OnClearAll -= Clear;
            EventManager.OnPlayerReset -= Reset;
        }

        // Use this for initialization
        void Awake()
        {
            _title = transform.FindDeepChild("TitleText").GetComponent<Text>();
            _titleBg = GetComponent<Image>();

            _voice = transform.FindDeepChild("VoiceableIcon").gameObject;
            _touch = transform.FindDeepChild("TouchableIcon").gameObject;
            _iot = transform.FindDeepChild("IoTIcon").gameObject;

            _completedIcon = transform.FindDeepChild("CompletedIcon").gameObject;
            _incompletedIcon = transform.FindDeepChild("IncompletedIcon").gameObject;

            _incompletedIcon.GetComponent<Button>().enabled = false;

            _featureContainer = transform.FindDeepChild("FeatureContainer").gameObject;
            _checkboxes = transform.FindDeepChild("Checkboxes").gameObject;

            // Set default state...
            _voice.SetActive(false);
            _touch.SetActive(false);
            _iot.SetActive(false);
        }

        public void SetupStep(LearningExperienceEngine.Action action)
        {
            _actionObject = action;

            _title.text = action.instruction.title;

            // Go through the triggers.
            foreach (var trigger in action.triggers)
            {
                // Click trigger handling
                if (trigger.mode.Equals("click"))
                {
                    _touch.SetActive(true);
                }

                // Voice trigger handling.
                else if (trigger.mode.Equals("voice"))
                {
                    _voice.SetActive(true);
                }

                else if (trigger.mode.Equals("sensor"))
                {
                    _iot.SetActive(true);
                }
            }
        }

        public void Next()
        {
            EventManager.Next("touch");
        }

        private void Update()
        {
            _featureContainer.SetActive(IsActive);
            //_checkboxes.SetActive(IsActive);

            _completedIcon.SetActive(_actionObject.isCompleted);
            _incompletedIcon.SetActive(!_actionObject.isCompleted);

            if (IsActive)
            {
                _titleBg.color = ActiveColor;

                if (_touch.activeSelf)
                    _incompletedIcon.gameObject.GetComponent<Button>().enabled = true;
            }

            else
            {
                _titleBg.color = InactiveColor;

                _incompletedIcon.gameObject.GetComponent<Button>().enabled = false;
            }
        }

        private void Reset()
        {
            Invoke(nameof(DoReset), 0.5f);
        }

        private void DoReset()
        {
            _featureContainer.SetActive(false);
            _titleBg.color = InactiveColor;
            _completedIcon.SetActive(false);
            _incompletedIcon.SetActive(true);
            _incompletedIcon.gameObject.GetComponent<Button>().enabled = false;
            Debug.LogInfo("RESET!");
        }


        private void Clear()
        {
            Destroy(gameObject);
        }
    }
}