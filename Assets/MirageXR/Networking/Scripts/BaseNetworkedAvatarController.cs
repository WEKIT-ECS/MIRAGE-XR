#if FUSION2
using Fusion;
#endif
using UnityEngine;

namespace MirageXR
{
#if FUSION2
	public class BaseNetworkedAvatarController : NetworkBehaviour
#else
	public class BaseNetworkedAvatarController : MonoBehaviour
#endif
	{
		private NetworkedAvatarReferences _avatarRefs;

#if FUSION2
		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalController => Object && Object.HasStateAuthority;
#endif

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
}
