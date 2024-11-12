using Fusion;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedUserManager : MonoBehaviour
	{
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

		public async Task InitializeLocalUserDataAsync()
		{
			if (string.IsNullOrEmpty(LocalUserData.UserName))
			{
				IdentityOidcConnectService oidcService = ServiceManager.GetService<IdentityOidcConnectService>();
				if (oidcService.IsLoggedIn)
				{
					IUserInfo userInfo = await oidcService.GetUserDataAsync();
					if (userInfo != null)
					{
						LocalUserData.UserName = userInfo.FullName;
					}
				}
			}
			if (string.IsNullOrEmpty(LocalUserData.UserName))
			{
				LocalUserData.UserName = "Guest";
			}
		}

		public void RegisterNetworkedUserData(PlayerRef owner, NetworkedUserData networkedUserData)
		{
			if (networkedUserData != null && !_networkedUserData.ContainsKey(owner))
			{
				Debug.Log($"Adding user {owner} ({networkedUserData.UserName}) to the user data list");
				if (owner == NetworkRunner.LocalPlayer)
				{
					Debug.Log($"{owner} is the local user, so it gets the prepared local user data");
					networkedUserData.LocalUserDataSource = LocalUserData;
				}

				_networkedUserData.Add(owner, networkedUserData);
				UserListChanged?.Invoke();
			}
		}

		public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef leftPlayer)
		{
			if (_networkedUserData.ContainsKey(leftPlayer))
			{
				_networkedUserData.Remove(leftPlayer);
				UserListChanged?.Invoke();
			}
		}

		public NetworkedUserData GetNetworkedUserDataOrDefault(PlayerRef playerRef)
		{
			return _networkedUserData.GetValueOrDefault(playerRef);
		}
	}
}
