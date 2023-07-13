using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class Pick : MonoBehaviour
    {
        private const string _LockHelpText = "When locked the arrow (or 3D model) will bounce back to this location if it is not correctly placed on the target";
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

        private bool _isTrigger = false;
        private int _triggerStepIndex;
        private float _triggerDuration;
        private MeshRenderer _arrowRenderer;

        private static ActivityManager _activityManager => MirageXR.RootObject.Instance.activityManager;

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

            _moveMode = RootObject.Instance.activityManager.EditModeActive;
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
                return;
            }

            var correctDistance = _targetTransform.transform.localScale.x * 0.25f;
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

        private void OnPlacedIncorrectly()
        {
            transform.localPosition = _resetPosition;
            transform.localRotation = _resetRotation;

            SetArrowWrongColor();
            PlayAudio(_incorrectAudio);
        }

        private void OnPlacedCorrectly()
        {
            transform.localPosition = _targetTransform.localPosition;
            SetArrowRightColor();
            PlayAudio(_correctAudio);

            if (_isTrigger)
            {
                StartCoroutine(TriggerAction());
            }

            EventManager.NotifyOnPickPlacedCorrectly();
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

        private IEnumerator TriggerAction()
        {
            yield return new WaitForSeconds(_triggerDuration);

            _activityManager.ActivateActionByIndex(_triggerStepIndex);
        }

        public void SetTrigger(Trigger trigger)
        {
            _isTrigger = trigger != null;

            if (_isTrigger)
            {
                var stepIndex = int.Parse(trigger.value) - 1;

                if (stepIndex > _activityManager.ActionsOfTypeAction.Count)
                {
                    stepIndex = _activityManager.ActionsOfTypeAction.Count - 1;
                }

                _triggerStepIndex = stepIndex;
                _triggerDuration = trigger.duration;
            }
        }
    }
}
