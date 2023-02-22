using UnityEngine;

namespace MirageXR
{
    public class CalibrationAnimation : MonoBehaviour
    {
        private static readonly int Load = Animator.StringToHash("Load");

        [SerializeField] private GameObject _lookHereText;
        [SerializeField] private GameObject _calibratingText;
        [SerializeField] private Animator _loadingAnimator;
        [SerializeField] private AudioSource _audioSource;

        public void StopAnimation()
        {
            if (_loadingAnimator.GetBool(Load))
            {
                _loadingAnimator.Play("loading", 0, 0);
            }

            _lookHereText.SetActive(true);
            _calibratingText.SetActive(false);
            _loadingAnimator.SetBool(Load, false);
            _loadingAnimator.gameObject.SetActive(false);
            _audioSource.Stop();
        }

        public void PlayAnimation()
        {
            _loadingAnimator.gameObject.SetActive(true);
            _lookHereText.SetActive(false);
            _calibratingText.SetActive(true);
            _loadingAnimator.SetBool(Load, true);
            _audioSource.Play();
        }
    }
}
