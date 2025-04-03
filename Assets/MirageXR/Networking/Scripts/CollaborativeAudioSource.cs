using uLipSync;
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
			uLipSync.uLipSync lipSync = GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = GetComponentInParent<uLipSyncBlendShape>();
			Debug.Log("Binding lip sync " + blendShapeController.ToString());
			lipSync.onLipSyncUpdate.AddListener(blendShapeController.OnLipSyncUpdate);
			blendShapeController.maxBlendShapeValue = 1;
		}

		public void UnBindLipSync()
		{
			uLipSync.uLipSync lipSync = GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = GetComponentInParent<uLipSyncBlendShape>();
			if (blendShapeController == null)
			{
				Debug.LogWarning("Could not unbind lip sync component since the blend shape driver was already destroyed.", this);
				return;
			}
			Debug.Log("Unbinding lip sync " + blendShapeController.ToString());
			lipSync.onLipSyncUpdate.RemoveListener(blendShapeController.OnLipSyncUpdate);
		}
	}
}
