using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public abstract class AvatarInitializer : MonoBehaviour
	{
		public abstract int Priority { get; }

		public abstract void InitializeAvatar(GameObject avatar);

		public virtual UniTask InitializeAvatarAsync(GameObject avatar)
		{
			return UniTask.CompletedTask;
		}

		public virtual void CleanupAvatar(GameObject avatar)
		{ }
	}
}
