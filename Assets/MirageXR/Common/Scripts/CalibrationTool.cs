using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        private static float ANIMATION_TIME = 5f;

        public static CalibrationTool Instance { get; private set; }

        [SerializeField] private CalibrationAnimation _calibrationAnimation;
        [SerializeField] private ImageTargetBehaviour _imageTargetBehaviour;
        [SerializeField] private DefaultTrackableEventHandler _trackableEventHandler;
        private UnityEvent _onTargetFound = new UnityEvent();
        private UnityEvent _onTargetLost = new UnityEvent();
        private UnityEvent _onCalibrationFinished = new UnityEvent();

        public UnityEvent onTargetFound => _onTargetFound;

        public UnityEvent onTargetLost => _onTargetLost;

        public UnityEvent onCalibrationFinished => _onCalibrationFinished;

        public float animationTime => ANIMATION_TIME;

        public bool isEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                SetEnabled(value);
            }
        }

        private bool _isEnabled;
        private bool _isTargetFound;
        private Coroutine _countdownToEnd;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _trackableEventHandler.OnTargetFound.AddListener(OnTargetFound);
            _trackableEventHandler.OnTargetLost.AddListener(OnTargetLost);
            isEnabled = false;
        }

        private void SetEnabled(bool value)
        {
            if (value)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        private void OnTargetFound()
        {
            _isTargetFound = true;
            _onTargetFound.Invoke();
            _calibrationAnimation.PlayAnimation();
            _countdownToEnd = StartCoroutine(WaitAndDo(ANIMATION_TIME, Calibrate));
        }

        private void OnTargetLost()
        {
            _isTargetFound = false;
            _onTargetLost.Invoke();
            _calibrationAnimation.StopAnimation();
            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }
        }

        private void Enable()
        {
            _isEnabled = true;

            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }

            if (_imageTargetBehaviour != null && _calibrationAnimation != null)
            {
                _imageTargetBehaviour.enabled = true;
                _calibrationAnimation.gameObject.SetActive(true);
            }
        }

        private void Disable()
        {
            _isTargetFound = false;

            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }

            _calibrationAnimation.StopAnimation();
            _imageTargetBehaviour.enabled = false;
            _calibrationAnimation.gameObject.SetActive(false);
        }

        public async void Calibrate()
        {
            _calibrationAnimation.StopAnimation();
            if (_isTargetFound)
            {
                await RootObject.Instance.workplaceManager.CalibrateWorkplace(transform);
                _onCalibrationFinished.Invoke();
            }

            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }

            isEnabled = false;
        }

        private static IEnumerator WaitAndDo(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
    }
}
