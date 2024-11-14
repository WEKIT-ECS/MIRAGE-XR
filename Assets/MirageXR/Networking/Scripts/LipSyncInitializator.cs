using UnityEngine;

namespace MirageXR
{
	public class LipSyncInitializator : MonoBehaviour
    {
		public OVRLipSyncContextMorphTarget MorphTarget { get; private set; }

		public void Initialize(LipSyncInitializationBridge initializationBridge)
		{
			MorphTarget = gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
			MorphTarget.skinnedMeshRenderer = initializationBridge.AvatarRenderer;
		}
	}
}
