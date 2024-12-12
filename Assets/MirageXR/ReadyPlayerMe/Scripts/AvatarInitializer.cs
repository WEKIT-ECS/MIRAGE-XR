using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public abstract class AvatarInitializer : MonoBehaviour
	{
		public abstract int Priority { get; }

		public abstract void InitializeAvatar(GameObject avatar);

		public virtual void CleanupAvatar(GameObject avatar)
		{ }
	}
}
