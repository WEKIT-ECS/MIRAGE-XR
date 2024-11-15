using UnityEngine;

namespace MirageXR
{
	public class LipSyncInitializer : MonoBehaviour
    {
		[SerializeField] private AvatarLoader _avatarLoader;

		[SerializeField] private SkinnedMeshRenderer _avatarRenderer;

		private OVRLipSyncContextMorphTarget morphTarget;

		private void Awake()
		{
			_avatarLoader.AvatarLoaded += OnAvatarLoaded;
		}

		private void OnDestroy()
		{
			_avatarLoader.AvatarLoaded -= OnAvatarLoaded;
		}

		private void OnAvatarLoaded(bool success)
		{
			if (success)
			{
				// initialize the lip syncing components only after the avatar was loaded
				// otherwise it gives error messages about missing blend shapes as the default prefab does not have them
				Initialize();
			}
		}

		private void Initialize()
		{
			morphTarget = gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
			morphTarget.skinnedMeshRenderer = _avatarRenderer;
		}
	}
}
