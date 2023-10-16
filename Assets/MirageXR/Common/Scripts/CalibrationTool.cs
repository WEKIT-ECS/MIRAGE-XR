using System.Collections;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        [SerializeField] private CalibrationAnimation _calibrationAnimation;
        [SerializeField] private UnityEvent _onCalibrationStarted = new UnityEvent();
        [SerializeField] private UnityEvent _onCalibrationCanceled = new UnityEvent();
        [SerializeField] private UnityEvent _onCalibrationFinished = new UnityEvent();

        public UnityEvent onCalibrationStarted => _onCalibrationStarted;

        public UnityEvent onCalibrationCanceled => _onCalibrationCanceled;

        public UnityEvent onCalibrationFinished => _onCalibrationFinished;

        private IImageTarget _imageTarget;
        private float _animationTime = 5f;
        private bool _isTargetFound;
        private Coroutine _countdownToEnd;

        public void Initialization(float animationTime)
        {
            _animationTime = animationTime;
            var imageTarget = GetComponentInParent<ImageTargetBase>();

            if (imageTarget == null)
            {
                Debug.LogError("Can't find IImageTarget");
                return;
            }

            imageTarget.onTargetFound.RemoveAllListeners();
            imageTarget.onTargetFound.AddListener(OnTargetFound);
            imageTarget.onTargetLost.RemoveAllListeners();
            imageTarget.onTargetLost.AddListener(OnTargetLost);
        }

        public void Enable()
        {
            enabled = true;
            _calibrationAnimation.gameObject.SetActive(true);

            if (gameObject.activeInHierarchy)
            {
                OnTargetFound(null);
            }
        }

        public void Disable()
        {
            OnTargetLost(null);
            _calibrationAnimation.gameObject.SetActive(false);
            enabled = false;
        }

        private void OnTargetFound(IImageTarget imageTarget)
        {
            if (!enabled)
            {
                return;
            }

            _isTargetFound = true;
            _onCalibrationStarted.Invoke();
            _calibrationAnimation.PlayAnimation();
            _countdownToEnd = StartCoroutine(WaitAndDo(_animationTime, Calibrate));
        }

        private void OnTargetLost(IImageTarget imageTarget)
        {
            if (!enabled)
            {
                return;
            }

            _isTargetFound = false;
            _onCalibrationCanceled.Invoke();
            _calibrationAnimation.StopAnimation();
            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }
        }

        private void Calibrate()
        {
            _calibrationAnimation.StopAnimation();
            if (_isTargetFound)
            {
                _onCalibrationFinished.Invoke();
            }

            if (_countdownToEnd != null)
            {
                StopCoroutine(_countdownToEnd);
                _countdownToEnd = null;
            }
        }

        private static IEnumerator WaitAndDo(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
    }
}
