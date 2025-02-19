using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class Pick : MonoBehaviour
    {
        private const string _ModelButtonHelpText = "Click this button and select a 3D model from the augmentation list to change the pick and place object model";

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private Transform _targetTransform;
        [SerializeField] private ObjectManipulator _objectManipulator;
        [SerializeField] private Button _changeModelButton;
        [SerializeField] private Text _hoverGuide;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _correctAudio;
        [SerializeField] private AudioClip _incorrectAudio;

        public string MyModelID;

        private Vector3 _resetPosition;
        private Quaternion _resetRotation;
        private bool _isMoving = false;
        private bool _moveMode = true;
        private Color _originalArrowColor;

        private LearningExperienceEngine.Trigger _correctTrigger;
        private LearningExperienceEngine.Trigger _incorrectTrigger;
        private bool _isCorrectTrigger = false;
        private bool _isIncorrectTrigger = false;

        private MeshRenderer _arrowRenderer;

        private static LearningExperienceEngine.ActivityManager _activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        public Vector3 ResetPosition
        {
            get => _resetPosition;
            set => _resetPosition = value;
        }

        public Quaternion ResetRotation
        {
            get => _resetRotation;
            set => _resetRotation = value;
        }

        public MeshRenderer ArrowRenderer => _arrowRenderer;

        public Button ChangeModelButton => _changeModelButton;

        public bool MoveMode
        {
            get => _moveMode;
            set => _moveMode = value;
        }

        private void Awake()
        {
            _arrowRenderer = GetComponentInChildren<MeshRenderer>();

            _originalArrowColor = _arrowRenderer.material.color;

            _resetPosition = transform.position;
            _resetRotation = transform.rotation;
        }

        private void Start()
        {
            _objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
            _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
            _changeModelButton.onClick.AddListener(CapturePickModel);

            _moveMode = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;
            AddHoverGuide(_changeModelButton.gameObject, _ModelButtonHelpText);
            SetArrowWrongColor();
        }

        /// <summary>
        /// Ready to select a model augmentation for replcaing with the arrow
        /// </summary>
        private void CapturePickModel()
        {
            _changeModelButton.GetComponent<Image>().color = Color.red;
            ActionEditor.Instance.pickArrowModelCapturing = (true, this);
        }

        public void SetMoveMode(bool value)
        {
            _moveMode = value;

            if (value)
            {
                ResetPositions();
                SetArrowWrongColor();
            }
        }

        private void OnManipulationStarted(ManipulationEventData data)
        {
            SetArrowWrongColor();
        }

        private void OnManipulationEnded(ManipulationEventData data)
        {
            if (_moveMode)
            {
                _resetPosition = transform.localPosition;
                _resetRotation = transform.localRotation;
                TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.PICK_POSITION_CHANGED);
                return;
            }

            var correctDistance = _targetTransform.transform.localScale.x * 0.3f;
            var distance = Vector3.Distance(transform.position, _targetTransform.position);

            if (distance <= correctDistance)
            {
                OnPlacedCorrectly();
            }
            else
            {
                OnPlacedIncorrectly();
            }
        }

        private void SetArrowColor(Color color)
        {
            _arrowRenderer.material.SetColor(ColorProperty, color);
        }

        private void SetArrowRightColor()
        {
            SetArrowColor(Color.green);
        }

        private void SetArrowWrongColor()
        {
            SetArrowColor(_originalArrowColor);
        }

        private void ResetPositions()
        {
            transform.localPosition = _resetPosition;
            transform.localRotation = _resetRotation;
        }

        private void OnPlacedIncorrectly()
        {
            ResetPositions();
            SetArrowWrongColor();
            PlayAudio(_incorrectAudio);

            if (_isIncorrectTrigger)
            {
                StartCoroutine(TriggerAction(_incorrectTrigger));
            }
        }

        private void OnPlacedCorrectly()
        {
            transform.localPosition = _targetTransform.localPosition;
            SetArrowRightColor();
            PlayAudio(_correctAudio);

            if (_isCorrectTrigger)
            {
                StartCoroutine(TriggerAction(_correctTrigger));
            }

            EventManager.NotifyOnPickPlacedCorrectly();
            TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.PICK_AND_PLACED);
        }

        private void PlayAudio(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        private void AddHoverGuide(GameObject obj, string hoverMessage)
        {
            var hoverGuilde = obj.AddComponent<HoverGuilde>();
            hoverGuilde.SetGuildText(_hoverGuide);
            hoverGuilde.SetMessage(hoverMessage);
        }

        private IEnumerator TriggerAction(LearningExperienceEngine.Trigger trigger)
        {
            yield return new WaitForSeconds(trigger.duration);

            var stepIndex = int.Parse(trigger.value) - 1;

            if (stepIndex > _activityManager.ActionsOfTypeAction.Count)
            {
                stepIndex = _activityManager.ActionsOfTypeAction.Count - 1;
            }

            _activityManager.ActivateActionByIndex(stepIndex);
        }

        public void SetTrigger(LearningExperienceEngine.Trigger trigger)
        {
            switch (trigger.mode)
            {
                case LearningExperienceEngine.TriggerMode.PickAndPlace:
                    _isCorrectTrigger = true;
                    _correctTrigger = trigger;
                    break;
                case LearningExperienceEngine.TriggerMode.IncorrectPickAndPlace:
                    _isIncorrectTrigger = true;
                    _incorrectTrigger = trigger;
                    break;
                default:
                    Debug.Log("Not a valid pick and place trigger");
                    break;
            }
        }
    }
}
