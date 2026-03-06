using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public abstract class AvatarBaseController : MonoBehaviour
	{
		private AvatarReferences _avatarRefs;

		protected AvatarReferences AvatarRefs
		{
			get
			{
				if (_avatarRefs == null)
				{
					_avatarRefs = GetComponentInParent<AvatarReferences>();
				}
				return _avatarRefs;
			}
		}

		public void SetReferences(AvatarReferences avatarReferences)
		{
			_avatarRefs = avatarReferences;
		}
	}
}
