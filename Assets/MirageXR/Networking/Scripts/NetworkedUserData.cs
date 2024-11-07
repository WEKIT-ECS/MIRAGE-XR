#if FUSION2
using Fusion;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#endif
using UnityEngine;

namespace MirageXR
{
#if FUSION2
	public class NetworkedUserData : NetworkBehaviour
	{
		private UserData _userDataSource;
		private ChangeDetector _changeDetector;

		public event Action<string> NetworkedUserNameChanged;

		[Networked, Capacity(25)]
		public string UserName { get; set; }

		public UserData UserDataSource
		{
			get => _userDataSource;
			set
			{
				if (_userDataSource != null)
				{
					_userDataSource.UserNameChanged -= OnUserNameChanged;
				}
				_userDataSource = value;
				UserName = _userDataSource.UserName;
				_userDataSource.UserNameChanged += OnUserNameChanged;
			}
		}

		private void OnUserNameChanged(string newUserName)
		{
			UserName = newUserName;
		}

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

			if (HasStateAuthority)
			{
				CollaborationManager.Instance.RegisterLocalUserData(this);
			}
		}
	}
#else
	public class NetworkedUserData : MonoBehaviour {}
#endif
}
