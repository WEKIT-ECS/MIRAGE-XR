#if FUSION2
using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using LearningExperienceEngine;
#endif
using System;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedUserManager : MonoBehaviour, IDisposable
	{
#if FUSION2
		private readonly Dictionary<PlayerRef, NetworkedUserData> _networkedUserData = new();

		public LocalUserData LocalUserData { get; private set; } = new();

		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;

		public IEnumerable<PlayerRef> UserList => _networkedUserData.Keys;

		public PlayerRef LocalUser => _networkRunner.LocalPlayer;

		public event System.Action UserListChanged;
		public event System.Action AnyUserNameChanged;

		public void Initialize(IAuthorizationManager authorizationManager, NetworkRunner networkRunner, NetworkEvents networkEvents)
		{
			_networkRunner = networkRunner;
			_networkEvents = networkEvents;
			_networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
			LocalUserData.Initialize(authorizationManager);
		}

		public void RegisterNetworkedUserDataAsync(PlayerRef owner, NetworkedUserData networkedUserData)
		{
			if (networkedUserData != null && !_networkedUserData.ContainsKey(owner))
			{
				if (owner == _networkRunner.LocalPlayer)
				{
					Debug.Log($"{owner} is the local user, so it gets the prepared local user data");
					LocalUserData.UpdateAllData();
					networkedUserData.LocalUserDataSource = LocalUserData;
				}

				Debug.Log($"Adding user {owner} ({networkedUserData.UserName}) to the user data list");

				_networkedUserData.Add(owner, networkedUserData);
				networkedUserData.NetworkedUserNameChanged += OnUserNameChanged;
				UserListChanged?.Invoke();
			}
		}

		private void OnUserNameChanged(string userName)
		{
			AnyUserNameChanged?.Invoke();
		}

		private void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef leftPlayer)
		{
			if (_networkedUserData.ContainsKey(leftPlayer))
			{
				_networkedUserData[leftPlayer].NetworkedUserNameChanged -= OnUserNameChanged;
				_networkedUserData.Remove(leftPlayer);
				UserListChanged?.Invoke();
			}
		}

		public NetworkedUserData GetNetworkedUserDataOrDefault(PlayerRef playerRef)
		{
			return _networkedUserData.GetValueOrDefault(playerRef);
		}
#endif
		public void Dispose()
		{
#if FUSION2
			LocalUserData.Dispose();
#endif
		}
	}
}
