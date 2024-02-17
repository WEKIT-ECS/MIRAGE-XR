using System.Collections;
using UnityEngine;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        [SerializeField] private CalibrationAnimation _calibrationAnimation;

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
            RootObject.Instance.calibrationManager.OnCalibrationStarted();
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
            RootObject.Instance.calibrationManager.OnCalibrationCanceled();
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
                var eulerAngles = transform.rotation.eulerAngles;
                var rotation = new Vector3(0, eulerAngles.x + eulerAngles.y + eulerAngles.z - 90f, 0);
                var position = transform.position;
                RootObject.Instance.calibrationManager.OnCalibrationFinished(new Pose(position, Quaternion.Euler(rotation)));
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
