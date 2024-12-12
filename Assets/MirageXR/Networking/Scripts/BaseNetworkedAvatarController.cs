#if FUSION2
using Fusion;
#endif
using UnityEngine;

namespace MirageXR
{
#if FUSION2
	public class BaseNetworkedAvatarController : NetworkBehaviour
	{
		private NetworkedAvatarReferences _avatarRefs;

		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalController => Object && Object.HasStateAuthority;

		protected NetworkedAvatarReferences AvatarRefs
		{
			get
			{
				if (_avatarRefs == null)
				{
					_avatarRefs = GetComponentInParent<NetworkedAvatarReferences>();
				}
				return _avatarRefs;
			}
		}

		public void SetReferences(NetworkedAvatarReferences avatarRefs)
		{
			_avatarRefs = avatarRefs;
		}
	}

#else
	public class BaseNetworkedAvatarController : MonoBehaviour
	{
	}
#endif
}
