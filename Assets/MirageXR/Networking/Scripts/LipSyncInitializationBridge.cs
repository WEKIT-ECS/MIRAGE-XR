using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace MirageXR
{
	public class LipSyncInitializationBridge : MonoBehaviour
	{
		[SerializeField] private AvatarLoader _avatarLoader;

		[field: SerializeField] public SkinnedMeshRenderer AvatarRenderer;

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
				LipSyncInitializator lipSyncInitializator = GetComponentInChildren<LipSyncInitializator>();
				lipSyncInitializator.Initialize(this);
			}
		}
	}
}
