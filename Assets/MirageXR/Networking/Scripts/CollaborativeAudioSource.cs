using UnityEngine;

namespace MirageXR
{
	public class CollaborativeAudioSource : MonoBehaviour
    {
		AudioSource _audioSource;

		private void Start()
		{
			_audioSource = GetComponent<AudioSource>();
			CollaborationManager.Instance.AddVoiceSource(_audioSource);
		}

		private void OnDestroy()
		{
			CollaborationManager.Instance.RemoveVoiceSource(_audioSource);
		}
	}
}
