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
using Random = UnityEngine.Random;
#endif

using System;
using UnityEngine;

namespace MirageXR
{
	[RequireComponent(typeof(NetworkedUserManager), typeof(ConnectionManager))]
#if FUSION2
	[RequireComponent(typeof(NetworkRunner), typeof(FusionVoiceClient), typeof(NetworkEvents))]
#endif
	public class CollaborationManager : MonoBehaviour, IDisposable
	{
		[SerializeField] private bool _useInvitationCode = false;
		[SerializeField] private bool _useSessionPassword = false;
		[SerializeField] private HandTrackingManager _handTrackingManager;
		[SerializeField] private GameObject _sessionDataPrefab;

#if FUSION2
		[SerializeField] private Recorder _recorder;

		private ConnectionManager _connectionManager;
		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;
		private FusionVoiceClient _fusionVoiceClient;
		private NetworkedUserManager _networkedUserManager;

		private readonly List<AudioSource> _voiceSources = new List<AudioSource>();
		private bool _muteVoiceChat = false;

		public int PlayerId => NetworkRunner != null ? NetworkRunner.LocalPlayer.PlayerId : -1;
		public bool IsConnectedToServer => NetworkRunner != null && NetworkRunner.IsConnectedToServer;
		public string InvitationCode { get; private set; }
		public string SessionPassword { get; private set; }

		public string SessionName { get => ConnectionManager.roomName; set => ConnectionManager.roomName = value; }

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
		public NetworkEvents NetworkEvents => _networkEvents;
		public FusionVoiceClient FusionVoiceClient => _fusionVoiceClient;

		public NetworkEvents.RunnerEvent OnConnectedToServer => _onConnectedToServer;
		public NetworkEvents.DisconnectFromServerEvent OnDisconnectedFromServer => _onDisconnectedFromServer;

		private readonly NetworkEvents.RunnerEvent _onConnectedToServer = new();
		private readonly NetworkEvents.DisconnectFromServerEvent _onDisconnectedFromServer = new();

		public async UniTask InitializeAsync(IAuthorizationManager authorizationManager)
		{
			_networkedUserManager = GetComponent<NetworkedUserManager>();
			_connectionManager = GetComponent<ConnectionManager>();
			_networkRunner = GetComponent<NetworkRunner>();
			_networkEvents = GetComponent<NetworkEvents>();
			_fusionVoiceClient = GetComponent<FusionVoiceClient>();

			_networkedUserManager.Initialize(authorizationManager, _networkRunner, _networkEvents);
			await _connectionManager.InitializeAsync(_networkRunner);

			_networkEvents.OnConnectedToServer.AddListener(NetworkEventsOnConnectedToServer);
			_networkEvents.OnDisconnectedFromServer.AddListener(NetworkEventsOnDisconnectedFromServer);
		}

		private void NetworkEventsOnConnectedToServer(NetworkRunner networkRunner)
		{
			_onConnectedToServer.Invoke(networkRunner);
		}

		private void NetworkEventsOnDisconnectedFromServer(NetworkRunner networkRunner, NetDisconnectReason netDisconnectReason)
		{
			_onDisconnectedFromServer.Invoke(networkRunner, netDisconnectReason);
		}

		public async UniTask<bool> StartNewSession()
		{
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

			if (_recorder == null)
			{
				_recorder = GetComponentInChildren<Recorder>();
			}

			_recorder.MicrophoneDevice = new Photon.Voice.DeviceInfo(Microphone.devices.First());

			Debug.Log($"Photon Voice is now using the following microphone: {_recorder.MicrophoneDevice.Name}");

			var result = await ConnectionManager.Connect();

			if (NetworkRunner.IsSharedModeMasterClient)
			{
				NetworkRunner.Spawn(_sessionDataPrefab);
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

		public void Dispose()
		{
#if FUSION2
			_networkedUserManager.Dispose();
#endif
		}
	}
}
