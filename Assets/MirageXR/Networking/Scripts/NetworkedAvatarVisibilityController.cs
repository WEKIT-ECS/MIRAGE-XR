#if FUSION2
using Fusion;
#else
using UnityEngine;
#endif

namespace MirageXR
{
#if FUSION2
	public class NetworkedAvatarVisibilityController : BaseNetworkedAvatarController
    {
		public AvatarVisibilityController2 VisibilityController
		{
			get => _avatarRefs.OfflineReferences.VisibilityController;
		}

		public override void Spawned()
		{
			base.Spawned();

			if (HasStateAuthority)
			{
				VisibilityController.Visible = false;
			}
		}
	}
#else
	public class NetworkedAvatarVisibilityController : MonoBehaviour
	{
	}
#endif
}
