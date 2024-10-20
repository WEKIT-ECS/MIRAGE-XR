using Fusion;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedUserData : NetworkBehaviour
	{
		private UserData _userDataSource;

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

		public override async void Spawned()
		{
			base.Spawned();

			if (HasStateAuthority)
			{
				CollaborationManager.Instance.RegisterUserData(this);
			}
		}
	}
}
