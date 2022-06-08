using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class CharacterSettings : MonoBehaviour
    {

        [SerializeField] private Toggle trigger;
        [SerializeField] private DialogRecorder dialogRecorder;

        [SerializeField] private ToggleGroup audioModeToggleGroup;
        [SerializeField] private Toggle aiToggle;
        [SerializeField] private Toggle preRecordToggle;
        [SerializeField] private GameObject dialogRecorderPanel;
        [SerializeField] private GameObject aiSettingsPanel;

        [SerializeField] private MovementManager movemenetManager;
        [SerializeField] private Dropdown animationMenu;

        [SerializeField] private Button assignImageButton;
        [SerializeField] private Button resetImageButton;
        [SerializeField] private Toggle animationLoopToggle;


        [SerializeField] private Text hoverGuide;

        public readonly string defaultImageDisplayAnimationName = "Image Display";

        private void Start()
        {
            AddHoverGuide(trigger.gameObject, "Enable this to move to the next action step when the dialog or animation is fully played. The longest will be selected as the trigger.");
            AddHoverGuide(dialogRecorder.RecordButton.gameObject, "Record a voice to be played as the character speech.");
            AddHoverGuide(dialogRecorder.PlayButton.gameObject, "Play the recorded voice.");
            AddHoverGuide(dialogRecorder.LoopToggle.gameObject, "Replay the audio file while the character exist.");
            AddHoverGuide(dialogRecorder.CloseButton.gameObject, "Close the dialog recorder. This can be used in playmode when only the dialog recorder is visible and the user want to close it as well.");
            AddHoverGuide(dialogRecorder.OpenButton.gameObject, "Open the dialog setting.");
            AddHoverGuide(movemenetManager.PathLoop.gameObject, "The character will turn back at the end of the path, and repeat it again. Path loop will be activated only in play mode.");
            AddHoverGuide(movemenetManager.FollowPlayer.gameObject, "The character will follow the player whereever he/she goes. It holds two meters distance with the camera.");
            AddHoverGuide(animationMenu.gameObject, "Select an animation clip to be played by the character.");
            AddHoverGuide(assignImageButton.gameObject, "In case \"Image Display\" is selected as the animation type, you can assign an image annotation to it. Activate this button and then select the image annotation button from annotaiton list.");
            AddHoverGuide(resetImageButton.gameObject, "Reset the image augmentation which is assinged to this character.");
            AddHoverGuide(aiToggle.gameObject, "Select for artificial intelligence mode. The dialogue can be set by changing the assistant ID in the character model JSON in the LMS. AI can be activated only for one of the existing characters in this action step.");
            AddHoverGuide(preRecordToggle.gameObject, "If enabled, you are able to record an audio and the character will play it as a dialogue.");

            animationMenu.onValueChanged.AddListener(delegate{ OnAnimationMenuOOptionChanged(); });
            trigger.onValueChanged.AddListener(delegate { OnTriggerValueChanged(); });
            aiToggle.onValueChanged.AddListener(delegate { OnAiToggleValueChanged(); });
            preRecordToggle.onValueChanged.AddListener(delegate { OnPreRecordToggleValueChanged(); });


        }


        private void OnEnable()
        {
            EventManager.OnEditModeChanged += SetEditMode;
        }


        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= SetEditMode;
        }

        public MovementManager MovementManager => movemenetManager;

        public Dropdown AnimationMenu => animationMenu;

        public DialogRecorder DialogRecorder => dialogRecorder;


        public Toggle AnimationLoopToggle => animationLoopToggle;


        public Button ResetImageButton => resetImageButton;


        public Button AssignImageButton => assignImageButton;


        public Toggle Trigger => trigger;

        public Toggle AIToggle => aiToggle;


        public Toggle PreRecordToggle => preRecordToggle;

        public ToggleGroup AudioModeToggleGroup => audioModeToggleGroup;

        /// <summary>
        /// Change animation by animation name if needed (eg. IBM Watson assinstant)
        /// </summary>
        /// <param name="animationName"></param>
        public void ChangeAnimation(string animationName)
        {
            animationMenu.value = animationMenu.options.FindIndex(option => option.text == animationName);
        }

        private void OnAnimationMenuOOptionChanged()
        {

            assignImageButton.interactable = animationMenu.options[animationMenu.value].text == defaultImageDisplayAnimationName;
            resetImageButton.interactable = animationMenu.options[animationMenu.value].text == defaultImageDisplayAnimationName;
        }

        private void OnTriggerValueChanged()
        {
            // if trigger is on the aniamtion loop and audio loop should be disabled
            if (trigger.isOn)
            {
                dialogRecorder.LoopToggle.isOn = false;
                animationLoopToggle.isOn = false;
            }

            // loops should be disable if trigger is on
            dialogRecorder.LoopToggle.interactable = !trigger.isOn;
            animationLoopToggle.interactable = !trigger.isOn;
        }


        private void OnAiToggleValueChanged()
        {
            dialogRecorderPanel.SetActive(!aiToggle.isOn);
            aiSettingsPanel.SetActive(aiToggle.isOn);
        }

        public void OnPreRecordToggleValueChanged()
        {
            dialogRecorderPanel.SetActive(preRecordToggle.isOn);
            aiSettingsPanel.SetActive(!preRecordToggle.isOn);
        }


        private void AddHoverGuide(GameObject obj, string hoverMessage)
        {
            var HoverGuilde = obj.AddComponent<HoverGuilde>();
            HoverGuilde.SetGuildText(hoverGuide);
            HoverGuilde.SetMessage(hoverMessage);

        }


        private void SetEditMode(bool editMode)
        {
            if (!editMode)
                hoverGuide.transform.parent.gameObject.SetActive(false);
        }
    }

}
