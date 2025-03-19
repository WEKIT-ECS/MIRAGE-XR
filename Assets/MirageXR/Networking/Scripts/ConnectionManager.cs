#if FUSION2
using Fusion;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MirageXR
{
	/**
     * 
     * Handles:
     * - connection launch (either with room name or matchmaking session properties)
     * - user representation spawn on connection
     **/
#if XRSHARED_ADDON_AVAILABLE
    public class ConnectionManager : MonoBehaviour, Fusion.XR.Shared.IUserSpawner
#else
	public class ConnectionManager : MonoBehaviour
#endif
	{
		[Flags]
		public enum ConnectionCriteria
		{
			RoomName = 1,
			SessionProperties = 2
		}

		[Serializable]
		public struct StringSessionProperty
		{
			public string propertyName;
			public string value;
		}

		[Header("Room configuration")]
		[SerializeField] private GameMode gameMode = GameMode.Shared;
		[SerializeField] private string roomName = "SampleFusion";

		[Tooltip("Set it to 0 to use the DefaultPlayers value, from the Global NetworkProjectConfig (simulation section)")]
		[SerializeField] private int playerCount = 0;

		[FormerlySerializedAs("connectionCriteria")]
		[Header("Room selection criteria")]
		[SerializeField] private ConnectionCriteria connectionCriterion = ConnectionCriteria.RoomName;
		[Tooltip("If connectionCriteria include SessionProperties, additionalSessionProperties (editable in the inspector) will be added to sessionProperties")]
		[SerializeField] private List<StringSessionProperty> additionalSessionProperties = new();

		[Header("Local user spawner")]
		[SerializeField] private NetworkObject userPrefab;

		[Header("Scene settings")]
		[SerializeField] private LoadSceneMode loadSceneMode = LoadSceneMode.Additive;
		
		[Header("Info")]
		[SerializeField] private List<StringSessionProperty> actualSessionProperties = new();

		public NetworkObject UserPrefab
		{
			get => userPrefab;
			set => userPrefab = value;
		}

		public string RoomName
		{
			get => roomName;
			set => roomName = value;
		}

		// Dictionary of spawned user prefabs, to store them on the server for host topology, and destroy them on disconnection (for shared topology, use Network Objects's "Destroy When State Authority Leaves" option)
		private readonly Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new();

		private Dictionary<string, SessionProperty> _sessionProperties;
		private INetworkSceneManager _sceneManager;

		private bool ShouldConnectWithRoomName => (connectionCriterion & ConnectionCriteria.RoomName) != 0;
		private bool ShouldConnectWithSessionProperties => (connectionCriterion & ConnectionCriteria.SessionProperties) != 0;

		private Dictionary<string, SessionProperty> AllConnectionSessionProperties
		{
			get
			{
				var propDict = new Dictionary<string, SessionProperty>();
				actualSessionProperties = new List<StringSessionProperty>();
				if (_sessionProperties != null)
				{
					foreach (var prop in _sessionProperties)
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
		public void Initialize(CollaborationManager collaborationManager)
		{
			collaborationManager.OnPlayerJoinEvent.AddListener(OnPlayerJoined);
			collaborationManager.OnPlayerLeftEvent.AddListener(OnPlayerLeft);
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

		public async UniTask<bool> ConnectAsync(NetworkRunner runner)
		{
			_sceneManager ??= gameObject.AddComponent<NetworkSceneManagerDefault>();

			var args = new StartGameArgs
			{
				GameMode = gameMode,
				Scene = CurrentSceneInfo(),
				SceneManager = _sceneManager
			};

			if (ShouldConnectWithRoomName)
			{
				args.SessionName = roomName;
			}

			var sessionProperties = AllConnectionSessionProperties;

			if (ShouldConnectWithSessionProperties)
			{
				Debug.Log($"Setting session properties... ({sessionProperties.Count} properties)");
				args.SessionProperties = sessionProperties;
			}

			if (playerCount > 0)
			{
				args.PlayerCount = playerCount;
			}			

			var result = await runner.StartGame(args);

			if (!result.Ok)
			{
				Debug.LogError($"Could not connect to session. Reason: {result.ShutdownReason}");
				return false;
			}

			var prop = string.Empty;
			if (runner.SessionInfo.Properties is { Count: > 0 })
			{
				prop = "SessionProperties: ";
				foreach (var p in runner.SessionInfo.Properties) prop += $" ({p.Key}={p.Value.PropertyValue}) ";
			}

			Debug.Log($"Session info: Room name {runner.SessionInfo.Name}. Region: {runner.SessionInfo.Region}. {prop}");
			if ((connectionCriterion & ConnectionCriteria.RoomName) == 0)
			{
				roomName = runner.SessionInfo.Name;
			}

			return true;
		}

		private void OnPlayerJoinedSharedMode(NetworkRunner runner, PlayerRef player)
		{
			if (player == runner.LocalPlayer && userPrefab != null)
			{
				// Spawn the user prefab for the local user
				var networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, player, (networkRunner, obj) => { });
				runner.SetPlayerObject(player, networkPlayerObject);
			}
		}

		private void OnPlayerJoinedHostMode(NetworkRunner runner, PlayerRef player)
		{
			// The user's prefab has to be spawned by the host
			if (runner.IsServer && userPrefab != null)
			{
				Debug.Log($"OnPlayerJoined. PlayerId: {player.PlayerId}");
				// We make sure to give the input authority to the connecting player for their user's object
				var networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, inputAuthority: player);

				// Keep track of the player avatars so we can remove it when they disconnect
				_spawnedUsers.Add(player, networkPlayerObject);
			}
		}

		private void OnPlayerLeftHostMode(NetworkRunner runner, PlayerRef player)
		{
			// Find and remove the players avatar (only the host would have stored the spawned game object)
			if (_spawnedUsers.TryGetValue(player, out var networkObject))
			{
				runner.Despawn(networkObject);
				_spawnedUsers.Remove(player);
			}
		}

		private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
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

		private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (runner.Topology == Topologies.ClientServer)
			{
				OnPlayerLeftHostMode(runner, player);
			}
		}
	}
}
#else
using UnityEngine;
    public class ConnectionManager : MonoBehaviour {}
#endif
