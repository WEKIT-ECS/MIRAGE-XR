#if FUSION2
using Fusion;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using Fusion.Sockets;
using MirageXR.View;
using Random = UnityEngine.Random;
#endif

using System;
using UnityEngine;

namespace MirageXR
{
	[RequireComponent(typeof(NetworkedUserManager), typeof(ConnectionManager))]
	public class CollaborationManager : MonoBehaviour, IDisposable, INetworkRunnerCallbacks
	{
		[SerializeField] private bool _useInvitationCode = false;
		[SerializeField] private bool _useSessionPassword = false;
		[SerializeField] private HandTrackingManager _handTrackingManager;
		[SerializeField] private GameObject _sessionDataPrefab;
		[SerializeField] private GameObject _speakerPrefab;

#if FUSION2
		private ConnectionManager _connectionManager;
		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;
		private FusionVoiceClient _fusionVoiceClient;
		private NetworkedUserManager _networkedUserManager;
		private IAssetBundleManager _assetBundleManager;
		private NetworkActivityView _networkActivityView;
		private Recorder _recorder;

		private readonly List<AudioSource> _voiceSources = new();
		private bool _muteVoiceChat;

		public int PlayerId => _networkRunner != null ? _networkRunner.LocalPlayer.PlayerId : -1;
		public PlayerRef LocalPlayer => _networkRunner != null ? _networkRunner.LocalPlayer : default;
		public bool IsConnectedToServer => _networkRunner != null && _networkRunner.IsConnectedToServer;
		public bool IsSharedModeMasterClient => _networkRunner != null && _networkRunner.IsSharedModeMasterClient;
		public string InvitationCode { get; private set; }
		public string SessionPassword { get; private set; }

		public string SessionName { get => ConnectionManager.RoomName; set => ConnectionManager.RoomName = value; }

		public bool VoiceMicrophoneEnabled { get => _recorder.TransmitEnabled; set => _recorder.TransmitEnabled = value; }

		public bool MuteVoiceChat
		{
			get => _muteVoiceChat;
			set
			{
				foreach (var audioSource in _voiceSources)
				{
					audioSource.mute = value;
				}
				_muteVoiceChat = value;
			}
		}

		public NetworkedUserManager UserManager => _networkedUserManager;
		public ConnectionManager ConnectionManager => _connectionManager;
		public NetworkRunner NetworkRunner => _networkRunner;
		public FusionVoiceClient FusionVoiceClient => _fusionVoiceClient;

		public NetworkEvents.PlayerEvent OnPlayerJoinEvent => _onPlayerJoin;
		public NetworkEvents.PlayerEvent OnPlayerLeftEvent => _onPlayerLeft;
		public NetworkEvents.RunnerEvent OnConnectedToServerEvent => _onConnectedToServer;
		public NetworkEvents.DisconnectFromServerEvent OnDisconnectedFromServerEvent => _onDisconnectedFromServer;
		public NetworkEvents.ReliableDataEvent OnReliableDataEvent => _onReliableData;

		private readonly NetworkEvents.PlayerEvent _onPlayerJoin = new();
		private readonly NetworkEvents.PlayerEvent _onPlayerLeft = new();
		private readonly NetworkEvents.RunnerEvent _onConnectedToServer = new();
		private readonly NetworkEvents.DisconnectFromServerEvent _onDisconnectedFromServer = new();
		private readonly NetworkEvents.ReliableDataEvent _onReliableData = new();

		public void Initialize(IAuthorizationManager authorizationManager, IAssetBundleManager assetBundleManager)
		{
			_assetBundleManager = assetBundleManager;
			_networkedUserManager = GetComponent<NetworkedUserManager>();
			_connectionManager = GetComponent<ConnectionManager>();

			var recorderObj = new GameObject("Recorder");
			recorderObj.transform.SetParent(transform);
			_recorder = recorderObj.AddComponent<Recorder>();

			_networkedUserManager.Initialize(this, authorizationManager);
			_connectionManager.Initialize(this);
		}

		public void Dispose()
		{
#if FUSION2
			_networkedUserManager.Dispose();
#endif
		}

		public void Disconnect()
		{
			_onPlayerLeft.Invoke(_networkRunner, LocalPlayer);
			_networkRunner.RemoveCallbacks(this);
			_networkRunner.Shutdown();
		}

		public void AddVoiceSpeaker(Speaker speaker, object userData)
		{
			_fusionVoiceClient?.AddSpeaker(speaker, userData);
		}

		public void SendReliableDataToPlayer(PlayerRef player, ReliableKey key, byte[] data)
		{
			_networkRunner.SendReliableDataToPlayer(player, key, data);
		}

		public void SendReliableDataToAllPlayers(ReliableKey key, byte[] data)
		{
			var players = _networkRunner.ActivePlayers;
			foreach (var playerRef in players)
			{
				/*if (playerRef == LocalPlayer)
				{
					continue;
				}*/

				_networkRunner.SendReliableDataToPlayer(playerRef, key, data);
			}
		}

		public async UniTask<bool> StartNewSession()
		{
			if (_networkRunner == null)
			{
				CreateRunner();
			}

			// TODO: move the generation functions into the if clauses
			// they are currently outside now to demonstrate that we can already generate this but do not actually use it

			//if (string.IsNullOrEmpty(InvitationCode))
			//{
			//	InvitationCode = GenerateInvitationCode();
			//}

			//if (_useInvitationCode)
			//{
			//	ConnectionManager.additionalSessionProperties.Add(new ConnectionManager.StringSessionProperty()
			//	{
			//		propertyName = "invitationCode",
			//		value = InvitationCode
			//	});
			//}

			//SessionPassword = GeneratePassword(8);

			//if (_useSessionPassword)
			//{
			//	ConnectionManager.additionalSessionProperties.Add(new ConnectionManager.StringSessionProperty()
			//	{
			//		propertyName = "password",
			//		value = SessionPassword
			//	});
			//}

			_handTrackingManager.StartTracking();
			_recorder.MicrophoneDevice = new Photon.Voice.DeviceInfo(Microphone.devices.First());

			Debug.Log($"Photon Voice is now using the following microphone: {_recorder.MicrophoneDevice.Name}");

			var result = await ConnectionManager.ConnectAsync(_networkRunner);
			if (result)
			{
				_fusionVoiceClient.ConnectAndJoinRoom();
			}
			return result;
		}

		public void AddVoiceSource(AudioSource voiceSource)
		{
			_voiceSources.Add(voiceSource);
		}

		public void RemoveVoiceSource(AudioSource voiceSource)
		{
			_voiceSources.Remove(voiceSource);
		}

		private void CreateRunner()
		{
			var networkRunnerObj = new GameObject("NetworkRunner");
			networkRunnerObj.transform.SetParent(transform);
			_networkRunner = networkRunnerObj.AddComponent<NetworkRunner>();
			_fusionVoiceClient = networkRunnerObj.AddComponent<FusionVoiceClient>();
			_fusionVoiceClient.AutoConnectAndJoin = false;
			_fusionVoiceClient.UseFusionAppSettings = true;
			_fusionVoiceClient.PrimaryRecorder = _recorder;
			_fusionVoiceClient.SpeakerPrefab = _speakerPrefab;
			_networkRunner.AddCallbacks(this);
		}

		private string GenerateInvitationCode()
		{
			int randomCode = Random.Range(0, 99999);
			string code = randomCode.ToString("00000");
			Debug.Log("Generating new invitation code: " + InvitationCode);
			return code;
		}

		private string GeneratePassword(int length)
		{
			// Random.Next is obviously not secure but enough for our purposes here and compatible with all devices
			string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,;.:-_<>#!?*+/1234567890";
			StringBuilder passwordBuilder = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				char randomChar = chars[Random.Range(0, chars.Length - 1)];
				passwordBuilder.Append(randomChar);
			}
			return passwordBuilder.ToString();
		}
#endif

#region INetworkRunnerCallbacks
		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log("[CollaborationManager] OnPlayerJoined");
			_onPlayerJoin.Invoke(runner, player);
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log("[CollaborationManager] OnPlayerLeft");
			_onPlayerLeft.Invoke(runner, player);
		}

		public void OnConnectedToServer(NetworkRunner runner)
		{
			_onConnectedToServer.Invoke(runner);
			Debug.Log("[CollaborationManager] OnConnectedToServer");
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
			Debug.Log("[CollaborationManager] Shutdown: " + shutdownReason);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
		{
			_onDisconnectedFromServer.Invoke(runner, reason);
			Debug.Log("[CollaborationManager] OnDisconnectedFromServer: " + reason);
		}

		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
		{
			Debug.Log("[CollaborationManager] OnConnectFailed: " + reason);
		}

		public void OnInput(NetworkRunner runner, NetworkInput input) { }
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, ArraySegment<byte> data)
		{
			_onReliableData.Invoke(runner, player, reliableKey, data);
		}

		public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float progress) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
		public void OnSceneLoadStart(NetworkRunner runner) { }
		public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
#endregion
	}
}
