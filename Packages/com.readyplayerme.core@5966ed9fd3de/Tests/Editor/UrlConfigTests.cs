using UnityEngine;
using NUnit.Framework;
using ReadyPlayerMe.Core.WebView;

namespace ReadyPlayerMe.Core.Tests
{
    public class UrlConfigTests : MonoBehaviour
    {
        private static readonly string URL_BASE = $"https://{CoreSettingsHandler.CoreSettings.Subdomain}.readyplayer.me";
        private readonly string URL_DEFAULT = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&selectBodyType";
        private readonly string URL_LANG_GERMAN = $"{URL_BASE}/de/avatar?frameApi&source=unity-avatar-creator&selectBodyType";
        private readonly string URL_LANG_BRAZIL = $"{URL_BASE}/pt-BR/avatar?frameApi&source=unity-avatar-creator&selectBodyType";

        private readonly string URL_GENDER_MALE = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&gender=male&selectBodyType";
        private readonly string URL_GENDER_NONE = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&gender=male&selectBodyType";

        private readonly string URL_TYPE_FULLBODY = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&bodyType=fullbody";
        private readonly string URL_TYPE_HALFBODY = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&bodyType=halfbody";

        private readonly string URL_CLEAR_CACHE = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&clearCache&selectBodyType";

        private readonly string URL_QUICK_START = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&quickStart";
        private readonly string URL_TOKEN = $"{URL_BASE}/avatar?frameApi&source=unity-avatar-creator&token=TOKEN&selectBodyType";
        private const string LOGIN_TOKEN = "TOKEN";

        public UrlConfig config;

        [SetUp]
        public void Setup()
        {
            config = new UrlConfig();
        }

        [Test]
        public void Url_Name_Change_German()
        {
            config.language = Language.German;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_LANG_GERMAN, res);
        }

        [Test]
        public void Url_Name_Change_Brazil()
        {
            config.language = Language.PortugueseBrazil;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_LANG_BRAZIL, res);
        }

        [Test]
        public void Url_Gender_Change_Male()
        {
            config.gender = Gender.Male;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_GENDER_MALE, res);
        }

        [Test]
        public void Url_Gender_Change_None()
        {
            config.gender = Gender.Male;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_GENDER_NONE, res);
        }

        [Test]
        public void Url_BodyType_Change_Fullbody()
        {
            config.bodyTypeOption = BodyTypeOption.FullBody;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_TYPE_FULLBODY, res);
        }

        [Test]
        public void Url_BodyType_Change_Halfbody()
        {
            config.bodyTypeOption = BodyTypeOption.HalfBody;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_TYPE_HALFBODY, res);
        }

        [Test]
        public void Url_ClearCache_Change()
        {
            config.clearCache = true;
            var res = config.BuildUrl();
            Assert.AreEqual(URL_CLEAR_CACHE, res);
        }

        [Test]
        public void Url_With_Token()
        {
            var testConfig = new UrlConfig();
            var res = testConfig.BuildUrl(LOGIN_TOKEN);
            Assert.AreEqual(URL_TOKEN, res);
        }

        [Test]
        public void Url_Default()
        {
            var testConfig = new UrlConfig();
            var res = testConfig.BuildUrl();
            Assert.AreEqual(URL_DEFAULT, res);
        }
    }
}
