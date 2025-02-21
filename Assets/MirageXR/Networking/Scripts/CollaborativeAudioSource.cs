using UnityEngine;

namespace MirageXR
{
	public class CollaborativeAudioSource : MonoBehaviour
    {
		AudioSource _audioSource;

		private void Start()
		{
#if FUSION2
			_audioSource = GetComponent<AudioSource>();
			RootObject.Instance.CollaborationManager.AddVoiceSource(_audioSource);
		}

		private void OnDestroy()
		{
			if (RootObject.Instance is null)
			{
				return;
			}

			RootObject.Instance.CollaborationManager.RemoveVoiceSource(_audioSource);
#endif
		}
	}
}
