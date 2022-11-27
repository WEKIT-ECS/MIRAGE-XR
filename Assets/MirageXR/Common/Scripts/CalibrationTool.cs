using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        private static float ANIMATION_TIME = 3f;

        [SerializeField] private GameObject _calibrationAnimation;

        [SerializeField] private UnityEvent _onTargetFound = new UnityEvent();
        [SerializeField] private UnityEvent _onTargetLost = new UnityEvent();
        [SerializeField] private UnityEvent _onCalibrationFinished = new UnityEvent();

        public UnityEvent onTargetFound => _onTargetFound;

        public UnityEvent onTargetLost => _onTargetLost;

        public UnityEvent onCalibrationFinished => _onCalibrationFinished;

        public float animationTime => ANIMATION_TIME;

        private bool _isTargetFound;
        private Coroutine _countdown;

        public static CalibrationTool Instance { get; private set; }

        public void SetCalibrationModel(GameObject calibrationModel)
        {
            _calibrationAnimation = calibrationModel;
        }

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
            Reset();
        }

        public void OnTargetFound()
        {
            _isTargetFound = true;
            onTargetFound.Invoke();
            _countdown = StartCoroutine(WaitAndDo(ANIMATION_TIME, Calibrate));
        }

        public void OnTargetLost()
        {
            _isTargetFound = false;
            onTargetLost.Invoke();
            if (_countdown != null)
            {
                StopCoroutine(_countdown);
            }
        }

        public void SetPlayer()
        {
            if (_calibrationAnimation)
            {
                _calibrationAnimation.SetActive(true);
            }
        }

        public void Reset()
        {
            if (_calibrationAnimation)
            {
                _calibrationAnimation.SetActive(false);
            }
        }

        public async void Calibrate()
        {
            if (_isTargetFound)
            {
                await RootObject.Instance.workplaceManager.CalibrateWorkplace(transform);
                onCalibrationFinished.Invoke();
            }

            if (_countdown != null)
            {
                StopCoroutine(_countdown);
                _countdown = null;
            }
        }

        private static IEnumerator WaitAndDo(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
    }
}
