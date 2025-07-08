using System;
using System.Threading;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// This class provides all the functionality required to create a basic Logout UI element.
    /// This is used to log out users from RPM accounts, once they have logged in.
    /// </summary>
    public class LogoutElement : MonoBehaviour
    {
        private const string TAG = nameof(LoginElement);
        public UnityEvent OnLogoutSuccess;
        public UnityEvent<string> OnLogoutFailed;
        [SerializeField] private Button logoutButton;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private void OnEnable()
        {
            logoutButton.onClick.AddListener(Logout);
        }

        private void OnDisable()
        {
            logoutButton.onClick.RemoveListener(Logout);
        }

        private async void Logout()
        {
            AuthManager.Logout();
            try
            {
                await AuthManager.LoginAsAnonymous(cancellationTokenSource.Token);
                LogOutSucceeded();
            }
            catch (Exception e)
            {
                if (e.Message == TaskExtensions.ON_REQUEST_CANCELLED_MESSAGE)
                {
                    return;
                }
                LogOutFailed(e.Message);
            }
        }

        private void LogOutFailed(string error)
        {
            SDKLogger.Log(TAG, $"Login failed with error: {error}");
            OnLogoutFailed?.Invoke(error);
        }

        private void LogOutSucceeded()
        {
            SDKLogger.Log(TAG, "Log out successful");
            OnLogoutSuccess?.Invoke();
        }
    }
}
