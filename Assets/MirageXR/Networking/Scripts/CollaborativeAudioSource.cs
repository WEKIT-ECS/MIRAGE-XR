using uLipSync;
using UnityEngine;

namespace MirageXR
{
	public class CollaborativeAudioSource : MonoBehaviour
    {
		private AudioSource _audioSource;

		private void Start()
		{
#if FUSION2
			_audioSource = GetComponent<AudioSource>();
			RootObject.Instance.CollaborationManager.AddVoiceSource(_audioSource);

			BindLipSync();
		}

		private void OnDestroy()
		{
			if (RootObject.Instance is null)
			{
				return;
			}

			RootObject.Instance.CollaborationManager.RemoveVoiceSource(_audioSource);

			UnBindLipSync();
#endif
		}

		public void BindLipSync()
		{
			LipSyncBinder.BindLipSync(_audioSource);
		}

		public void UnBindLipSync()
		{
			LipSyncBinder.UnBindLipSync(_audioSource);
		}
	}
}
