#if FUSION2
using Fusion;
#else
using UnityEngine;
#endif

namespace MirageXR
{
#if FUSION2
	public class NetworkedAvatarVisibilityController : NetworkBehaviour
    {
		private AvatarVisibilityController _avatarVisibilityController;

		public AvatarVisibilityController VisibilityController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarVisibilityController);
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
