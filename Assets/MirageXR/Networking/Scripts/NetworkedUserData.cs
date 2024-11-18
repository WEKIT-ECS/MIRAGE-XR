#if FUSION2
using Fusion;
using System;
using System.Net.NetworkInformation;
#else
using System;
using UnityEngine;
#endif


namespace MirageXR
{
#if FUSION2
	public class NetworkedUserData : NetworkBehaviour
#else
	public class NetworkedUserData : MonoBehaviour
#endif
	{
		private LocalUserData _localUserDataSource;

		private NetworkedAvatarController _avatarController;
		public NetworkedAvatarController AvatarController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarController);
		}

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
			_localUserDataSource.UserNameChanged -= OnLocalUserNameChanged;
			_localUserDataSource.AvatarUrlChanged -= OnLocalAvatarUrlChanged;
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

		public override async void Spawned()
		{
			base.Spawned();

			Runner.SetPlayerObject(Object.StateAuthority, Object);

			await CollaborationManager.Instance.UserManager.RegisterNetworkedUserData(Object.StateAuthority, this);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);

			UnsubscribeFromLocalData();
		}
#endif
	}
}
