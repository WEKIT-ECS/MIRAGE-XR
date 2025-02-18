using LearningExperienceEngine;
using System;

namespace MirageXR
{
	public class LocalUserData : IDisposable
	{
		private const string DefaultUsername = "Anonymous";

		private IAuthorizationManager _authorizationManager;
		private string _userName;
		private string _avatarUrl;

		private IdentityOidcConnectService _oidcService;

		public string UserName
		{
			get => string.IsNullOrEmpty(_userName) ? DefaultUsername : _userName;
			set
			{
				_userName = value; 
				UserNameChanged?.Invoke(value);
			}
		}

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
		public event Action<string> UserNameChanged;

		public void Dispose()
		{
			UnsubscribeFromDataSources();
		}

		public void Initialize(IAuthorizationManager authorizationManager)
		{
			_authorizationManager = authorizationManager;
			SubscribeToDataSources();
		}

		public void UpdateAllData()
		{
			_userName = _authorizationManager.UserName;
			AvatarUrl = UserSettings.AvatarUrl;
		}

		private void SubscribeToDataSources()
		{
			_authorizationManager.OnLoginCompleted += OnLoginCompleted;
			_authorizationManager.OnLogoutCompleted += OnLogoutCompleted;
			UserSettings.AvatarUrlChanged += OnAvatarUrlChanged;
		}

		private void UnsubscribeFromDataSources()
		{
			_authorizationManager.OnLoginCompleted -= OnLoginCompleted;
			_authorizationManager.OnLogoutCompleted -= OnLogoutCompleted;
			UserSettings.AvatarUrlChanged -= OnAvatarUrlChanged;
		}

		private void OnLoginCompleted(string token)
		{
			_userName = _authorizationManager.UserName;
		}

		private void OnLogoutCompleted()
		{
			_userName = string.Empty;
		}

		private void OnAvatarUrlChanged(string obj)
		{
			AvatarUrl = UserSettings.AvatarUrl;
		}
	}
}
