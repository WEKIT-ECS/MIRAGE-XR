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

        [Test]
        public void CreateRedirectUrl_ValidGuid_ReturnsExpectedUrl()
        {
            var url = QRCodeActivityParser.CreateRedirectUrl(ExpectedGuid);

            Assert.AreEqual("https://repository.wekit-ecs.com/deeplink/?activity=" + TestGuidString, url);
        }
    }
}
