#if FUSION2
using Fusion;
using System;
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
#if FUSION2
		private ChangeDetector _changeDetector;
#endif

		private NetworkedAvatarController _avatarController;
		public NetworkedAvatarController AvatarController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarController);
		}

		public event Action<string> NetworkedUserNameChanged;

#if FUSION2
		[Networked, Capacity(25)]
#endif
		public string UserName { get; set; }

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
					_localUserDataSource.UserNameChanged -= OnUserNameChanged;
				}
				_localUserDataSource = value;
				UserName = _localUserDataSource.UserName;
				_localUserDataSource.UserNameChanged += OnUserNameChanged;
			}
		}

		private void OnUserNameChanged(string newUserName)
		{
			UserName = newUserName;
		}

#if FUSION2
		public override void Render()
		{
			base.Render();

			foreach (var change in _changeDetector.DetectChanges(this))
			{
				switch (change)
				{
					case nameof(UserName):
						NetworkedUserNameChanged?.Invoke(UserName);
						break;
				}
			}
		}

		public override async void Spawned()
		{
			base.Spawned();

			_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

			Runner.SetPlayerObject(Object.StateAuthority, Object);

			CollaborationManager.Instance.UserManager.RegisterNetworkedUserData(Object.StateAuthority, this);
		}
#endif
	}
}
