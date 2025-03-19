#if FUSION2
using Fusion;
using System;
#else
using System;
using UnityEngine;
#endif

namespace MirageXR
{
	public class NetworkedUserData : BaseNetworkedAvatarController
	{
		private LocalUserData _localUserDataSource;

		public event Action<string> NetworkedUserNameChanged;
		public event Action<string> NetworkedAvatarUrlChanged;

#if FUSION2
		[Networked, Capacity(25), OnChangedRender(nameof(OnNetworkedUserNameChanged))]
#endif
		public string UserName { get; set; }

#if FUSION2
		[Networked, Capacity(64), OnChangedRender(nameof(OnNetworkedAvatarUrlChanged))]
#endif
		public string AvatarUrl { get; set; }

		public NetworkedAvatarReferences AvatarReferences { get => AvatarRefs; }

		/// <summary>
		/// The data source for the local user
		/// Information written into this object are copied into the NetworkedUserData and distributed across the network
		/// </summary>
		public LocalUserData LocalUserDataSource
		{
			get => _localUserDataSource;
			set
			{
				if (_localUserDataSource != null)
				{
					UnsubscribeFromLocalData();
				}
				_localUserDataSource = value;
				SubscribeToLocalData();
				ApplyLocalToNetworkedData();
			}
		}

		private void ApplyLocalToNetworkedData()
		{
			Debug.LogTrace("Applying local user data to networked data");
			UserName = _localUserDataSource.UserName;
			AvatarUrl = _localUserDataSource.AvatarUrl;
		}

		private void SubscribeToLocalData()
		{
			_localUserDataSource.UserNameChanged += OnLocalUserNameChanged;
			_localUserDataSource.AvatarUrlChanged += OnLocalAvatarUrlChanged;
		}

		private void UnsubscribeFromLocalData()
		{
			if (_localUserDataSource != null)
			{
				_localUserDataSource.UserNameChanged -= OnLocalUserNameChanged;
				_localUserDataSource.AvatarUrlChanged -= OnLocalAvatarUrlChanged;
			}
		}

		private void OnLocalUserNameChanged(string newUserName)
		{
			UserName = newUserName;
		}

		private void OnLocalAvatarUrlChanged(string newAvatarUrl)
		{
			AvatarUrl = newAvatarUrl;
		}

#if FUSION2
		private void OnNetworkedUserNameChanged()
		{
			NetworkedUserNameChanged?.Invoke(UserName);
		}

		private void OnNetworkedAvatarUrlChanged()
		{
			Debug.LogDebug("(Networked Change) AvatarURL is now " + AvatarUrl);
			NetworkedAvatarUrlChanged?.Invoke(AvatarUrl);
		}

		public override void Spawned()
		{
			base.Spawned();
			Runner.SetPlayerObject(Object.StateAuthority, Object);
			RootObject.Instance.CollaborationManager.UserManager.RegisterNetworkedUserDataAsync(Object.StateAuthority, this);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			UnsubscribeFromLocalData();
		}
#endif
	}
}
