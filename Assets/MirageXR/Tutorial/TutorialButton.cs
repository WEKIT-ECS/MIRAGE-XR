using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class TutorialButton : MonoBehaviour
    {
        [SerializeField] private Sprite _unselectedIcon;
        [SerializeField] private Sprite _selectedIcon;
        [SerializeField] private Image icon;
        private Button helpButton;

        private void Awake()
        {
            helpButton = GetComponent<Button>();

            if (!RootObject.Instance.platformManager.WorldSpaceUi) // this if statment should be removed after creating the non headset tutorial
            {
                helpButton.interactable = false;
                icon.sprite = _unselectedIcon;
            }
        }

        private void Start()
        {
            int tutorialStatus = PlayerPrefs.GetInt(TutorialManager.PLAYER_PREFS_STATUS_KEY);
            Debug.LogDebug(tutorialStatus.ToString());
            if (tutorialStatus == TutorialManager.STATUS_LOAD_ON_START && RootObject.Instance.platformManager.WorldSpaceUi)
            {
                // TODO: In the future, this should be changed to an event. Like: OnEverythingLoaded
                Invoke(nameof(StartTutorial), 0.2f);
            }

        }

        private void StartTutorial()
        {
            TutorialManager tutorialManager = TutorialManager.Instance;
            tutorialManager.TutorialButton = this;
            tutorialManager.StartTutorial(TutorialManager.TutorialType.HOLOLENS);
        }

        public void ToggleTutorial()
        {
            TutorialManager tutorialManager = TutorialManager.Instance;
            tutorialManager.TutorialButton = this;
            tutorialManager.ToggleTutorial(TutorialManager.TutorialType.HOLOLENS);
        }

        public void SetIconActive()
        {
            icon.sprite = _selectedIcon;
        }

        public void SetIconInactive()
        {
            icon.sprite = _unselectedIcon;
        }

        public void ResetButton()
        {
            icon.sprite = _unselectedIcon;
        }

    }
}