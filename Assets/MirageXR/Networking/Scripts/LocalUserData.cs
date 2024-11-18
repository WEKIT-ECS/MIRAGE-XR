using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class LocalUserData
	{
		private string _userName;
		private string _avatarUrl;

		private IdentityOidcConnectService _oidcService;

		public string UserName
		{
			get => _userName; set
			{
				_userName = value;
				UserNameChanged?.Invoke(value);
			}
		}
		public event Action<string> UserNameChanged;

		public string AvatarUrl
		{
			get => _avatarUrl;
			set
			{
				_avatarUrl = value;
				AvatarUrlChanged?.Invoke(value);
			}
		}
		public event Action<string> AvatarUrlChanged;

		~LocalUserData()
		{
			UnsubscribeFromDataSources();
		}

		public void Initialize()
		{
			_oidcService = ServiceManager.GetService<IdentityOidcConnectService>();
			SubscribeToDataSources();
		}

		public async Task UpdateAllDataAsync()
		{
			await FetchUserName();
			FetchAvatarUrl();
		}

		private void SubscribeToDataSources()
		{
			_oidcService.LoginCompleted += OnLoginCompleted;
			_oidcService.LogoutCompleted += OnLogoutCompleted;
			UserSettings.AvatarUrlChanged += OnAvatarUrlChanged;
		}

		private void UnsubscribeFromDataSources()
		{
			_oidcService.LoginCompleted -= OnLoginCompleted;
			_oidcService.LogoutCompleted -= OnLogoutCompleted;
			UserSettings.AvatarUrlChanged -= OnAvatarUrlChanged;
		}

		private async void OnLoginCompleted(object sender, EventArgs e)
		{
			await FetchUserName();
		}

		private async void OnLogoutCompleted(object sender, EventArgs e)
		{
			await FetchUserName();
		}

		private void OnAvatarUrlChanged(string obj)
		{
			FetchAvatarUrl();
		}

		private async Task FetchUserName()
		{
			if (_oidcService.IsLoggedIn)
			{
				IUserInfo userInfo = await _oidcService.GetUserDataAsync();
				if (userInfo != null)
				{
					UserName = userInfo.FullName;
				}
				else
				{
					Debug.LogError("Unable to get user data to display on the user data. Instead, the avatar will show as \"Anonymous\".");
					UserName = "Anonymous";
				}
			}
			else
			{
				UserName = "Guest";
			}
		}

		private void FetchAvatarUrl()
		{
			AvatarUrl = UserSettings.AvatarUrl;
		}
	}
}
