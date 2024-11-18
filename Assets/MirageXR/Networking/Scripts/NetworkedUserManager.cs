#if FUSION2
using Fusion;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endif
using UnityEngine;

namespace MirageXR
{
	public class NetworkedUserManager : MonoBehaviour
	{
#if FUSION2
		private Dictionary<PlayerRef, NetworkedUserData> _networkedUserData = new Dictionary<PlayerRef, NetworkedUserData>();

		public LocalUserData LocalUserData
		{
			get; private set;
		} = new LocalUserData();

		private NetworkRunner _networkRunner;
		private NetworkRunner NetworkRunner
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _networkRunner);
		}


		public IEnumerable<PlayerRef> UserList
		{
			get => _networkedUserData.Keys;
		}

		public PlayerRef LocalUser { get => NetworkRunner.LocalPlayer; }

		public event System.Action UserListChanged;
		public event System.Action AnyUserNameChanged;

		private void Start()
		{
			LocalUserData.Initialize();
		}

		public async Task RegisterNetworkedUserData(PlayerRef owner, NetworkedUserData networkedUserData)
		{
			if (networkedUserData != null && !_networkedUserData.ContainsKey(owner))
			{
				if (owner == NetworkRunner.LocalPlayer)
				{
					Debug.Log($"{owner} is the local user, so it gets the prepared local user data");
					await LocalUserData.UpdateAllDataAsync();
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

		public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef leftPlayer)
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
	}
}
