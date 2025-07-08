using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    public class PhotoPrivacyTerms : MonoBehaviour
    {
        private const string PRIVACY_POLICY_URL = "https://readyplayer.me/privacy";
        private const string TERMS_OF_USE_URL = "https://readyplayer.me/terms";

        public void OpenPrivacyPolicy()
        {
            Application.OpenURL(PRIVACY_POLICY_URL);
        }

        public void OpenTermsOfUse()
        {
            Application.OpenURL(TERMS_OF_USE_URL);
        }
    }
}
