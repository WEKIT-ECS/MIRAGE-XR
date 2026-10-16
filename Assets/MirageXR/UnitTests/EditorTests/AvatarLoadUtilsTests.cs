using NUnit.Framework;

namespace MirageXR.UnitTests
{
    [TestFixture]
    public class AvatarLoadUtilsTests
    {
        private const string TestAvatarId = "FemaleBlondBlackClothing";

        [TearDown]
        public void TearDown()
        {
            AvatarLoadUtils.ResetForTesting();
        }

        [Test]
        public void IdToModelUrl_WithoutPort_ReturnsExpectedUrl()
        {
            var mockConfig = new MockConfigManager();
            AvatarLoadUtils.Initialize(mockConfig);

            var url = AvatarLoadUtils.IdToModelUrl(TestAvatarId);

            Assert.AreEqual("https://repository.wekit-ecs.com/avatar/get?avatar_name=" + TestAvatarId, url);
        }

        [Test]
        public void IdToModelUrl_WithPort_ReturnsUrlWithPort()
        {
            var mockConfig = new MockConfigManager();
            mockConfig.SetNetworkServerPort("8001");
            AvatarLoadUtils.Initialize(mockConfig);

            var url = AvatarLoadUtils.IdToModelUrl(TestAvatarId);

            Assert.AreEqual("https://repository.wekit-ecs.com:8001/avatar/get?avatar_name=" + TestAvatarId, url);
        }

        [Test]
        public void IdToModelUrl_SanitizesId()
        {
            var mockConfig = new MockConfigManager();
            mockConfig.SetNetworkServerPort("8001");
            AvatarLoadUtils.Initialize(mockConfig);

            var url = AvatarLoadUtils.IdToModelUrl("Female Blond & Black Clothing!");

            Assert.AreEqual("https://repository.wekit-ecs.com:8001/avatar/get?avatar_name=FemaleBlondBlackClothing", url);
        }

        [Test]
        public void IdToModelUrl_AddressChanged_UpdatesUrl()
        {
            var mockConfig = new MockConfigManager();
            AvatarLoadUtils.Initialize(mockConfig);

            mockConfig.SetNetworkServerAddress("http://custom-repo.example.com");
            var url = AvatarLoadUtils.IdToModelUrl(TestAvatarId);

            Assert.AreEqual("http://custom-repo.example.com/avatar/get?avatar_name=" + TestAvatarId, url);
        }

        [Test]
        public void IdToModelUrl_PortChanged_UpdatesUrl()
        {
            var mockConfig = new MockConfigManager();
            AvatarLoadUtils.Initialize(mockConfig);

            mockConfig.SetNetworkServerPort("9000");
            var url = AvatarLoadUtils.IdToModelUrl(TestAvatarId);

            Assert.AreEqual("https://repository.wekit-ecs.com:9000/avatar/get?avatar_name=" + TestAvatarId, url);
        }

        [Test]
        public void ResetForTesting_UnsubscribesAndClearsState()
        {
            var mockConfig = new MockConfigManager();
            AvatarLoadUtils.Initialize(mockConfig);

            AvatarLoadUtils.ResetForTesting();

            // After reset, changing config should not affect internal state
            mockConfig.SetNetworkServerAddress("http://should-not-affect.com");
            mockConfig.SetNetworkServerPort("1234");

            // Since _serverAddress and _serverPort are cleared, base URL will be empty
            var url = AvatarLoadUtils.IdToModelUrl(TestAvatarId);
            Assert.AreEqual("/avatar/get?avatar_name=" + TestAvatarId, url);
        }

        private class MockConfigManager : LearningExperienceEngine.NewDataModel.IConfigManager
        {
            private string _serverAddress = "https://repository.wekit-ecs.com";
            private string _serverPort = string.Empty;

            public event UnityEngine.Events.UnityAction<string> OnNetworkServerAddressChanged;
            public event UnityEngine.Events.UnityAction<string> OnNetworkServerPortChanged;
            public event UnityEngine.Events.UnityAction<string> OnNetworkWebsocketAddressChanged;
            public event UnityEngine.Events.UnityAction<string> OnNetworkWebsocketPortChanged;

            public bool IsInitialized => true;
            public Cysharp.Threading.Tasks.UniTask WaitForInitialization() => Cysharp.Threading.Tasks.UniTask.CompletedTask;
            public Cysharp.Threading.Tasks.UniTask InitializeAsync() => Cysharp.Threading.Tasks.UniTask.CompletedTask;

            public string GetDefaultNetworkServerAddress() => "https://repository.wekit-ecs.com";
            public string GetDefaultNetworkWebsocketAddress() => string.Empty;
            public string GetDefaultNetworkServerPort() => string.Empty;
            public string GetDefaultNetworkWebsocketPort() => string.Empty;
            public string GetNetworkServerAddress() => _serverAddress;
            public string GetNetworkWebsocketAddress() => string.Empty;
            public string GetNetworkWebsocketPort() => string.Empty;
            public string GetNetworkServerPort() => _serverPort;

            public void SetNetworkServerAddress(string address)
            {
                _serverAddress = address;
                OnNetworkServerAddressChanged?.Invoke(address);
            }

            public void SetNetworkServerPort(string port)
            {
                _serverPort = port;
                OnNetworkServerPortChanged?.Invoke(port);
            }

            public void SetNetworkWebsocketAddress(string websocket) => OnNetworkWebsocketAddressChanged?.Invoke(websocket);
            public void SetNetworkWebsocketPort(string port) => OnNetworkWebsocketPortChanged?.Invoke(port);
            public void ResetNetworkServerPort() => SetNetworkServerPort(string.Empty);
            public void ResetNetworkServerAddress() => SetNetworkServerAddress("https://repository.wekit-ecs.com");
            public void ResetNetworkWebsocketPort() { }
            public void ResetNetworkWebsocketAddress() { }
        }
    }
}
