namespace MirageXR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class Pick : MonoBehaviour
    {
        [SerializeField] private Transform _placeLocation;
        [SerializeField] private GameObject _pickOb;
        [SerializeField] private float _correctionDistance;
        [SerializeField] private bool _resetOnMiss = true;
        [SerializeField] private SpriteToggle _lockToggle;
        [SerializeField] private Button _changeModelButton;
        [SerializeField] private Text _hoverGuide;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _correctAudio;
        [SerializeField] private AudioClip _incorrectAudio;

        private bool _shouldPlaySound;

        private Vector3 _resetPosition;
        private Quaternion _resetRotation;
        private bool _isMoving = false;
        private bool _moveMode = true;
        private float _targetRadius;
        private Color _originalArrowColor;

        private bool _isTrigger = false;
        private int _triggerStepIndex;
        private float _triggerDuration;

        private bool _editMode = false;

        private const string _LockHelpText = "When locked the arrow (or 3D model) will bounce back to this location if it is not correctly placed on the target";
        private const string _ModelButtonHelpText = "Click this button and select a 3D model from the augmentation list to change the pick and place object model";

        private static MirageXR.ActivityManager _activityManager => MirageXR.RootObject.Instance.activityManager;

        public bool EditMode
        {
            get { return _editMode; }
            set { _editMode = value; }
        }

        public Vector3 ResetPosition
        {
            get { return _resetPosition; }
            set { _resetPosition = value; }
        }

        public Quaternion ResetRotation
        {
            get { return _resetRotation; }
            set { _resetRotation = value; }
        }

        public MeshRenderer ArrowRenderer
        {
            get { return GetComponentInChildren<MeshRenderer>(); }
        }

        public Button ChangeModelButton
        {
            get
            {
                return _changeModelButton;
            }
        }

        public bool MoveMode
        {
            get => _moveMode;
            set => _moveMode = value;
        }

        public bool IsTrigger
        {
            get => IsTrigger;
            set => IsTrigger = value;
        }

        public int TriggerStepIndex
        {
            get => TriggerStepIndex;
            set => TriggerStepIndex = value;
        }

        public string MyModelID
        {
            get; set;
        }

        void Start()
        {
            _targetRadius = _placeLocation.transform.localScale.x / 2;
            _pickOb = this.gameObject;
            ChangeCorrectionDistance(_targetRadius);
            _moveMode = false;
            _lockToggle.IsSelected = true;

            _originalArrowColor = ArrowRenderer.material.color;

            _changeModelButton.onClick.AddListener(CapturePickModel);

            AddHoverGuide(_lockToggle.gameObject, _LockHelpText);
            AddHoverGuide(_changeModelButton.gameObject, _ModelButtonHelpText);
            _shouldPlaySound = false;
        }

        void Update()
        {
            float targetRadiusUpdate = _placeLocation.transform.localScale.x / 2;

            if (!_moveMode)
            {
                if (transform.hasChanged)
                {
                    ManipulationStart();
                }
                else if (!transform.hasChanged)
                {
                    ManipulationStop();
                }
                transform.hasChanged = false;
            }
            else if (_moveMode && ArrowRenderer.material.color != _originalArrowColor)
            {
                ArrowRenderer.material.SetColor("_Color", _originalArrowColor);
            }

            if (_targetRadius != targetRadiusUpdate)
            {
                _targetRadius = targetRadiusUpdate;

                ChangeCorrectionDistance(_targetRadius);
            }
        }

        /// <summary>
        /// Ready to select a model augmentation for replcaing with the arrow
        /// </summary>
        private void CapturePickModel()
        {
            _changeModelButton.GetComponent<Image>().color = Color.red;
            ActionEditor.Instance.pickArrowModelCapturing = (true, this);
        }

        public void SetMoveMode()
        {
            if (_moveMode)
            {
                _moveMode = false;
                ResetPosition = _pickOb.transform.localPosition;
                ResetRotation = _pickOb.transform.localRotation;
                _shouldPlaySound = false;
            }
            else
            {
                _moveMode = true;
            }

            _lockToggle.ToggleValue();
        }

        public void SetRestOnMiss(bool reset)
        {
            _resetOnMiss = reset;
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void SetTargettransform(Transform target)
        {
            _placeLocation = target;
        }

        /// <summary>
        /// Change the distance that will be considered close enough to the desired place location
        /// </summary>
        public void ChangeCorrectionDistance(float distance)
        {
            _correctionDistance = distance;
        }

        /// <summary>
        /// Sets isMoving to true to show that the object is being manipulated.
        /// </summary>
        public void ManipulationStart()
        {
            ArrowRenderer.material.SetColor("_Color", _originalArrowColor);
            _isMoving = true;
        }

        /// <summary>
        /// Checks if the pick object has been placed correctly and sets isMoving to false to show that the object has stopped being manipulated.
        /// </summary>
        public void ManipulationStop()
        {
            if (_isMoving)
            {
                if (Mathf.Abs(_pickOb.transform.localPosition.x - _placeLocation.localPosition.x) <= _correctionDistance &&
                Mathf.Abs(_pickOb.transform.localPosition.y - _placeLocation.localPosition.y) <= _correctionDistance &&
                Mathf.Abs(_pickOb.transform.localPosition.z - _placeLocation.localPosition.z) <= _correctionDistance)
                {
                    _pickOb.transform.localPosition = new Vector3(_placeLocation.localPosition.x, _placeLocation.localPosition.y, _placeLocation.localPosition.z);

                    ArrowRenderer.material.SetColor("_Color", Color.green);

                    if (_shouldPlaySound)
                    {
                        PlayAudio(_correctAudio);
                    }
                    if (_isTrigger && !EditMode)
                    {
                        StartCoroutine(TriggerAction());
                    }
                }
                else if (_resetOnMiss)
                {
                    _pickOb.transform.localPosition = ResetPosition;
                    _pickOb.transform.localRotation = ResetRotation;

                    ArrowRenderer.material.SetColor("_Color", _originalArrowColor);
                    if (_shouldPlaySound)
                    {
                        PlayAudio(_incorrectAudio);
                    }
                }
            }
            _isMoving = false;
            _shouldPlaySound = true;
        }


        private void PlayAudio(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        private void AddHoverGuide(GameObject obj, string hoverMessage)
        {
            var HoverGuilde = obj.AddComponent<HoverGuilde>();
            HoverGuilde.SetGuildText(_hoverGuide);
            HoverGuilde.SetMessage(hoverMessage);
        }

        private IEnumerator TriggerAction()
        {
            yield return new WaitForSeconds(_triggerDuration);

            _activityManager.ActivateActionByIndex(_triggerStepIndex);
        }

        public void SetTrigger(Trigger trigger)
        {
            _isTrigger = trigger != null ? true : false;

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
