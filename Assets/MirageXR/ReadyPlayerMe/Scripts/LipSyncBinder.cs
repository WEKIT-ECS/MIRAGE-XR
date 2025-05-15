using uLipSync;
using UnityEngine;

namespace MirageXR
{
    public static class LipSyncBinder
    {
        public static void BindLipSync(AudioSource speaker)
        {
			uLipSync.uLipSync lipSync = speaker.GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = speaker.GetComponentInParent<uLipSyncBlendShape>();
			Debug.LogDebug("Binding lip sync " + blendShapeController.ToString(), speaker.gameObject);
			lipSync.onLipSyncUpdate.AddListener(blendShapeController.OnLipSyncUpdate);
			blendShapeController.maxBlendShapeValue = 1;
		}

        public static void UnBindLipSync(AudioSource speaker)
        {
			uLipSync.uLipSync lipSync = speaker.GetComponent<uLipSync.uLipSync>();
			uLipSyncBlendShape blendShapeController = speaker.GetComponentInParent<uLipSyncBlendShape>();
			if (blendShapeController == null)
			{
				Debug.LogWarning("Could not unbind lip sync component since the blend shape driver was already destroyed.", speaker.gameObject);
				return;
			}
			Debug.LogDebug("Unbinding lip sync " + blendShapeController.ToString());
			lipSync.onLipSyncUpdate.RemoveListener(blendShapeController.OnLipSyncUpdate);
		}
    }
}
