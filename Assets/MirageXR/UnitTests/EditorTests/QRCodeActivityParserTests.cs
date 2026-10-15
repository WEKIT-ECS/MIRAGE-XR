using System;
using NUnit.Framework;

namespace MirageXR.UnitTests
{
    [TestFixture]
    public class QRCodeActivityParserTests
    {
        private const string TestGuidString = "07aa86d7-6f1e-4b0d-b0a5-fe6fdeb11248";
        private static readonly Guid ExpectedGuid = new Guid(TestGuidString);

        [Test]
        public void TryParseActivityId_DirectGuidString_ReturnsTrue()
        {
            var result = QRCodeActivityParser.TryParseActivityId(TestGuidString, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_DirectGuidWithBraces_ReturnsTrue()
        {
            var input = "{" + TestGuidString + "}";
            var result = QRCodeActivityParser.TryParseActivityId(input, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_RepositoryRedirectUrl_ReturnsTrue()
        {
            var url = "https://repository.wekit-ecs.com/deeplink/?activity=" + TestGuidString;
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_RepositoryRedirectUrlWithExtraParams_ReturnsTrue()
        {
            var url = "https://repository.wekit-ecs.com/deeplink/?mode=view&activity=" + TestGuidString + "&auth=token";
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_WekitDeepLinkLoad_ReturnsTrue()
        {
            var url = "wekit://load?activity=" + TestGuidString;
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_WekitDeepLinkWithWhitespace_ReturnsTrue()
        {
            var url = "  wekit://load?activity=" + TestGuidString + "  ";
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsTrue(result);
            Assert.AreEqual(ExpectedGuid, guid);
        }

        [Test]
        public void TryParseActivityId_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(QRCodeActivityParser.TryParseActivityId(null, out _));
            Assert.IsFalse(QRCodeActivityParser.TryParseActivityId(string.Empty, out _));
            Assert.IsFalse(QRCodeActivityParser.TryParseActivityId("   ", out _));
        }

        [Test]
        public void TryParseActivityId_InvalidUrlWithoutActivityParam_ReturnsFalse()
        {
            var url = "https://repository.wekit-ecs.com/deeplink/?other=123";
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsFalse(result);
            Assert.AreEqual(Guid.Empty, guid);
        }

        [Test]
        public void TryParseActivityId_MalformedGuid_ReturnsFalse()
        {
            var url = "https://repository.wekit-ecs.com/deeplink/?activity=not-a-valid-guid";
            var result = QRCodeActivityParser.TryParseActivityId(url, out var guid);

            Assert.IsFalse(result);
            Assert.AreEqual(Guid.Empty, guid);
        }

        [TearDown]
        public void TearDown()
        {
            QRCodeActivityParser.ResetForTesting();
        }

        [Test]
        public void CreateRedirectUrl_ValidGuid_ReturnsExpectedUrl()
        {
            var mockConfig = new MockConfigManager();
            var url = QRCodeActivityParser.CreateRedirectUrl(ExpectedGuid, mockConfig);

            Assert.AreEqual("https://repository.wekit-ecs.com/deeplink/?activity=" + TestGuidString, url);
        }

        [Test]
        public void CreateRedirectUrl_WithPort_ReturnsUrlWithPort()
        {
            var mockConfig = new MockConfigManager();
            mockConfig.SetNetworkServerPort("8000");
            var url = QRCodeActivityParser.CreateRedirectUrl(ExpectedGuid, mockConfig);

            Assert.AreEqual("https://repository.wekit-ecs.com:8000/deeplink/?activity=" + TestGuidString, url);
        }

        [Test]
        public void CreateRedirectUrl_AddressChanged_UpdatesRedirectUrl()
        {
            var mockConfig = new MockConfigManager();
            QRCodeActivityParser.Initialize(mockConfig);

            mockConfig.SetNetworkServerAddress("http://custom-repo.example.com");
            var url = QRCodeActivityParser.CreateRedirectUrl(ExpectedGuid);

            Assert.AreEqual("http://custom-repo.example.com/deeplink/?activity=" + TestGuidString, url);
        }

        [Test]
        public void CreateRedirectUrl_PortChanged_UpdatesRedirectUrl()
        {
            var mockConfig = new MockConfigManager();
            QRCodeActivityParser.Initialize(mockConfig);

            mockConfig.SetNetworkServerPort("9000");
            var url = QRCodeActivityParser.CreateRedirectUrl(ExpectedGuid);

            Assert.AreEqual("https://repository.wekit-ecs.com:9000/deeplink/?activity=" + TestGuidString, url);
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
