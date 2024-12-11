using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class BaseNetworkedAvatarController : NetworkBehaviour
	{
		protected NetworkedAvatarReferences _avatarRefs;

		// As we are in shared topology, having the StateAuthority means we are the local user
		public virtual bool IsLocalController => Object && Object.HasStateAuthority;

		public void SetReferences(NetworkedAvatarReferences avatarRefs)
		{
			_avatarRefs = avatarRefs;
		}

		protected virtual void Start()
		{
			if (_avatarRefs == null)
			{
				_avatarRefs = GetComponentInParent<NetworkedAvatarReferences>();
			}
		}
	}
}
