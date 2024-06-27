using System;
using ReadyPlayerMe.AvatarCreator;
using UnityEngine;
using UnityEngine.Events;

namespace ReadyPlayerMe.Samples.AvatarCreatorElements
{
    public class SessionHandler : MonoBehaviour
    {
        private readonly string sessionStoreKey = "StoredSession";
        
        public UnityEvent<UserSession> OnLogin;
        
        private async void Start()
        {
            if (PlayerPrefs.HasKey(sessionStoreKey))
            {
                AuthManager.SetUser(JsonUtility.FromJson<UserSession>(PlayerPrefs.GetString(sessionStoreKey)));
            }
            else
            {
                await AuthManager.LoginAsAnonymous();
            }
            OnLogin?.Invoke(AuthManager.UserSession);
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetString(sessionStoreKey, JsonUtility.ToJson(AuthManager.UserSession));
        }
        
    }
}
