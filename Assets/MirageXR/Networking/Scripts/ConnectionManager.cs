#if FUSION2
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MirageXR
{
	/**
     * 
     * Handles:
     * - connection launch (either with room name or matchmaking session properties)
     * - user representation spawn on connection
     **/
#if XRSHARED_ADDON_AVAILABLE
    public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks, Fusion.XR.Shared.IUserSpawner
#else
	public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
#endif
	{
		[System.Flags]
		public enum ConnectionCriterias
		{
			RoomName = 1,
			SessionProperties = 2
		}

		[System.Serializable]
		public struct StringSessionProperty
		{
			public string propertyName;
			public string value;
		}

		[Header("Room configuration")]
		public GameMode gameMode = GameMode.Shared;
		public string roomName = "SampleFusion";
		public bool connectOnStart = true;
		[Tooltip("Set it to 0 to use the DefaultPlayers value, from the Global NetworkProjectConfig (simulation section)")]
		public int playerCount = 0;

		[Header("Room selection criteria")]
		public ConnectionCriterias connectionCriterias = ConnectionCriterias.RoomName;
		[Tooltip("If connectionCriterias include SessionProperties, additionalSessionProperties (editable in the inspector) will be added to sessionProperties")]
		public List<StringSessionProperty> additionalSessionProperties = new List<StringSessionProperty>();
		public Dictionary<string, SessionProperty> sessionProperties;

		public INetworkSceneManager sceneManager;

		[Header("Local user spawner")]
		public NetworkObject userPrefab;

		[Header("Scene settings")]
		public LoadSceneMode loadSceneMode = LoadSceneMode.Additive;

		#region IUserSpawner
		public NetworkObject UserPrefab
		{
			get => userPrefab;
			set => userPrefab = value;
		}
		#endregion

		[Header("Event")]
		public UnityEvent onWillConnect = new UnityEvent();

		[Header("Info")]
		public List<StringSessionProperty> actualSessionProperties = new List<StringSessionProperty>();

		// Dictionary of spawned user prefabs, to store them on the server for host topology, and destroy them on disconnection (for shared topology, use Network Objects's "Destroy When State Authority Leaves" option)
		private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new Dictionary<PlayerRef, NetworkObject>();

		private NetworkRunner _runner;

		bool ShouldConnectWithRoomName => (connectionCriterias & ConnectionManager.ConnectionCriterias.RoomName) != 0;
		bool ShouldConnectWithSessionProperties => (connectionCriterias & ConnectionManager.ConnectionCriterias.SessionProperties) != 0;

		public async UniTask InitializeAsync(NetworkRunner networkRunner)
		{
			_runner = networkRunner;
			_runner.ProvideInput = true;
			_runner.AddCallbacks(this);
			if (connectOnStart)
			{
				await Connect();
			}
		}

		private Dictionary<string, SessionProperty> AllConnectionSessionProperties
		{
			get
			{
				var propDict = new Dictionary<string, SessionProperty>();
				actualSessionProperties = new List<StringSessionProperty>();
				if (sessionProperties != null)
				{
					foreach (var prop in sessionProperties)
					{
						propDict.Add(prop.Key, prop.Value);
						actualSessionProperties.Add(new StringSessionProperty { propertyName = prop.Key, value = prop.Value });
					}
				}
				if (additionalSessionProperties != null)
				{
					foreach (var additionalProperty in additionalSessionProperties)
					{
						propDict[additionalProperty.propertyName] = additionalProperty.value;
						actualSessionProperties.Add(additionalProperty);
					}

				}
				return propDict;
			}
		}

		public virtual NetworkSceneInfo CurrentSceneInfo()
		{
			var activeScene = SceneManager.GetActiveScene();
			SceneRef sceneRef = default;

			if (activeScene.buildIndex < 0 || activeScene.buildIndex >= SceneManager.sceneCountInBuildSettings)
			{
				Debug.LogError("Current scene is not part of the build settings");
			}
			else
			{
				sceneRef = SceneRef.FromIndex(activeScene.buildIndex);
			}

			var sceneInfo = new NetworkSceneInfo();
			if (sceneRef.IsValid)
			{
				sceneInfo.AddSceneRef(sceneRef, loadSceneMode);
			}
			return sceneInfo;
		}

		public async Task<bool> Connect()
		{
			sceneManager ??= gameObject.AddComponent<NetworkSceneManagerDefault>();
			onWillConnect?.Invoke();

			var args = new StartGameArgs
			{
				GameMode = gameMode,
				Scene = CurrentSceneInfo(),
				SceneManager = sceneManager
			};
			// Connection criteria
			if (ShouldConnectWithRoomName)
			{
				args.SessionName = roomName;
			}

			Dictionary<string, SessionProperty> sessionProperties = AllConnectionSessionProperties;

			if (ShouldConnectWithSessionProperties)
			{
				Debug.Log($"Setting session properties... ({sessionProperties.Count} properties)");
				args.SessionProperties = sessionProperties;
			}
			// Room details
			if (playerCount > 0)
			{
				args.PlayerCount = playerCount;
			}			

			var result = await _runner.StartGame(args);

			if (!result.Ok)
			{
				Debug.LogError($"Could not connect to session. Reason: {result.ShutdownReason}");
				return false;
			}

			var prop = "";
			if (_runner.SessionInfo.Properties is { Count: > 0 })
			{
				prop = "SessionProperties: ";
				foreach (var p in _runner.SessionInfo.Properties) prop += $" ({p.Key}={p.Value.PropertyValue}) ";
			}
			Debug.Log($"Session info: Room name {_runner.SessionInfo.Name}. Region: {_runner.SessionInfo.Region}. {prop}");
			if ((connectionCriterias & ConnectionCriterias.RoomName) == 0)
			{
				roomName = _runner.SessionInfo.Name;
			}

			return true;
		}

		#region Player spawn
		public void OnPlayerJoinedSharedMode(NetworkRunner runner, PlayerRef player)
		{
			if (player == runner.LocalPlayer && userPrefab != null)
			{
				// Spawn the user prefab for the local user
				var networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, player, (networkRunner, obj) => { });
				runner.SetPlayerObject(player, networkPlayerObject);
			}
		}

		public void OnPlayerJoinedHostMode(NetworkRunner runner, PlayerRef player)
		{
			// The user's prefab has to be spawned by the host
			if (runner.IsServer && userPrefab != null)
			{
				Debug.Log($"OnPlayerJoined. PlayerId: {player.PlayerId}");
				// We make sure to give the input authority to the connecting player for their user's object
				NetworkObject networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, inputAuthority: player, (runner, obj) =>
				{
				});

				// Keep track of the player avatars so we can remove it when they disconnect
				_spawnedUsers.Add(player, networkPlayerObject);
			}
		}

		// Despawn the user object upon disconnection
		public void OnPlayerLeftHostMode(NetworkRunner runner, PlayerRef player)
		{
			// Find and remove the players avatar (only the host would have stored the spawned game object)
			if (_spawnedUsers.TryGetValue(player, out NetworkObject networkObject))
			{
				runner.Despawn(networkObject);
				_spawnedUsers.Remove(player);
			}
		}

		#endregion

		#region INetworkRunnerCallbacks
		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			if (runner.Topology == Topologies.ClientServer)
			{
				OnPlayerJoinedHostMode(runner, player);
			}
			else
			{
				OnPlayerJoinedSharedMode(runner, player);
			}
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (runner.Topology == Topologies.ClientServer)
			{
				OnPlayerLeftHostMode(runner, player);
			}
		}
		#endregion

		#region INetworkRunnerCallbacks (debug log only)
		public void OnConnectedToServer(NetworkRunner runner)
		{
			Debug.Log("OnConnectedToServer");
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
			Debug.Log("Shutdown: " + shutdownReason);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
		{
			Debug.Log("OnDisconnectedFromServer: " + reason);
		}

		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
		{
			Debug.Log("OnConnectFailed: " + reason);
		}
		#endregion

		#region Unused INetworkRunnerCallbacks 
		public void OnInput(NetworkRunner runner, NetworkInput input) { }
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, ArraySegment<byte> data) { }
		public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float progress) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
		public void OnSceneLoadStart(NetworkRunner runner) { }
		public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		#endregion
	}

}
#else
using UnityEngine;
    public class ConnectionManager : MonoBehaviour {}
#endif
