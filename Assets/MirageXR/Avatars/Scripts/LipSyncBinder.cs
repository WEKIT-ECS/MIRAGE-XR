using uLipSync;
using UnityEngine;

namespace MirageXR
{
    public static class LipSyncBinder
    {
        public static void BindLipSync(AudioSource speaker)
        {
			if (speaker == null) return;
			uLipSync.uLipSync lipSync = speaker.GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = speaker.GetComponentInParent<uLipSyncBlendShape>();
			if (lipSync == null || blendShapeController == null)
			{
				Debug.LogDebug("Could not bind lip sync: missing uLipSync or uLipSyncBlendShape component.", speaker.gameObject);
				return;
			}
			Debug.LogDebug("Binding lip sync " + blendShapeController.ToString(), speaker.gameObject);
			lipSync.onLipSyncUpdate.AddListener(blendShapeController.OnLipSyncUpdate);
			blendShapeController.maxBlendShapeValue = 1;
		}

        public static void UnBindLipSync(AudioSource speaker)
        {
			if (speaker == null) return;
			uLipSync.uLipSync lipSync = speaker.GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = speaker.GetComponentInParent<uLipSyncBlendShape>();
			if (blendShapeController == null || lipSync == null)
			{
				// Normal during destruction or scene teardown when components are destroyed in arbitrary order
				return;
			}
			Debug.LogDebug("Unbinding lip sync " + blendShapeController.ToString());
			lipSync.onLipSyncUpdate.RemoveListener(blendShapeController.OnLipSyncUpdate);
		}
    }
}
