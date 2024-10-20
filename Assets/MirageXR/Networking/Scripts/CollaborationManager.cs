using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MirageXR
{
	public class CollaborationManager : MonoBehaviour
	{
		[SerializeField] private bool _useInvitationCode = false;
		[SerializeField] private bool _useSessionPassword = false;

		[SerializeField] private Recorder _recorder;

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

		private ConnectionManager _connectionManager;

		private ConnectionManager ConnectionManager
		{
			get
			{
				if (_connectionManager == null)
				{
					_connectionManager = GetComponent<ConnectionManager>();
				}
				return _connectionManager;
			}
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

			if (string.IsNullOrEmpty(InvitationCode))
			{
				InvitationCode = GenerateInvitationCode();
			}

			if (_useInvitationCode)
			{
				ConnectionManager.additionalSessionProperties.Add(new ConnectionManager.StringSessionProperty()
				{
					propertyName = "invitationCode",
					value = InvitationCode
				});
			}

			SessionPassword = GeneratePassword(8);

			if (_useSessionPassword)
			{
				ConnectionManager.additionalSessionProperties.Add(new ConnectionManager.StringSessionProperty()
				{
					propertyName = "password",
					value = SessionPassword
				});
			}

			return await _connectionManager.Connect();
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
			int randomCode = UnityEngine.Random.Range(0, 99999);
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
	}
}
