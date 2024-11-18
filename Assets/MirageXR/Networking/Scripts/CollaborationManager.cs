#if FUSION2
using Castle.Core.Logging;
using Fusion;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
#endif

using UnityEngine;

namespace MirageXR
{
	public class CollaborationManager : MonoBehaviour
	{
		[SerializeField] private bool _useInvitationCode = false;
		[SerializeField] private bool _useSessionPassword = false;

		[SerializeField] private HandTrackingManager _handTrackingManager;
		[SerializeField] private GameObject _sessionDataPrefab;

#if FUSION2
		private ConnectionManager _connectionManager;
		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;
		[SerializeField] private Recorder _recorder;
		private FusionVoiceClient _fusionVoiceClient;

		private List<AudioSource> _voiceSources = new List<AudioSource>();
		private bool _muteVoiceChat = false;

		public static CollaborationManager Instance { get; private set; }

		public string InvitationCode { get; private set; }
		public string SessionPassword { get; private set; }

		public string SessionName { get => ConnectionManager.roomName; set => ConnectionManager.roomName = value; }

		public bool VoiceMicrophoneEnabled { get => _recorder.TransmitEnabled; set => _recorder.TransmitEnabled = value; }

		public bool MuteVoiceChat
		{
			get => _muteVoiceChat;
			set
			{
				foreach (AudioSource audioSource in _voiceSources)
				{
					audioSource.mute = value;
				}
			}
		}

		private NetworkedUserManager _networkedUserManager;
		public NetworkedUserManager UserManager
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _networkedUserManager);
		}

		private ConnectionManager ConnectionManager
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _connectionManager);
		}

		public NetworkRunner NetworkRunner
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _networkRunner);
		}

		public NetworkEvents NetworkEvents
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _networkEvents);
		}

		private FusionVoiceClient FusionVoiceClient
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _fusionVoiceClient);
		}

		public void Awake()
		{
			if (Instance != null)
			{
				Destroy(this);
				return;
			}
			Instance = this;
		}

		public async Task<bool> StartNewSession()
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

			Debug.Log("Photon Voice is now using the following microphone: " + _recorder.MicrophoneDevice.Name);

			var res = await _connectionManager.Connect();

			if (NetworkRunner.IsSharedModeMasterClient)
			{
				NetworkRunner.Spawn(_sessionDataPrefab);
			}

			return res;
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
	}
}
