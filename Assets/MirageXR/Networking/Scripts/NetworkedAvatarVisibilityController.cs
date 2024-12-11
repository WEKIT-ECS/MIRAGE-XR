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
		public AvatarVisibilityController VisibilityController
		{
			get => AvatarRefs.OfflineReferences.VisibilityController;
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
