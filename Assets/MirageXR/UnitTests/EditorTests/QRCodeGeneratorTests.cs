using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZXing;

namespace MirageXR.UnitTests
{
    [TestFixture]
    public class QRCodeGeneratorTests
    {
        [Test]
        public void GenerateQRCodeTexture_ValidPayload_ReturnsValidTexture()
        {
            var payload = "https://example.com/miragexr-test";
            var texture = QRCodeGeneratorService.GenerateQRCodeTexture(payload, pixelsPerModule: 5);

            Assert.IsNotNull(texture);
            Assert.Greater(texture.width, 0);
            Assert.Greater(texture.height, 0);
            Assert.AreEqual(texture.width, texture.height);

            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void GenerateActivityQRCodeTexture_DecodesToCorrectRedirectUrl()
        {
            var activityId = Guid.NewGuid();
            var expectedUrl = QRCodeActivityParser.CreateRedirectUrl(activityId);

            var texture = QRCodeGeneratorService.GenerateActivityQRCodeTexture(activityId, pixelsPerModule: 8);
            Assert.IsNotNull(texture);

            // Decode using ZXing to verify round-trip correctness
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };

            var luminanceSource = new UnityColor32LuminanceSource(
                texture.GetPixels32(),
                texture.width,
                texture.height);

            var result = reader.Decode(luminanceSource);

            Assert.IsNotNull(result, "ZXing failed to decode the generated QR code texture.");
            Assert.AreEqual(expectedUrl, result.Text);

            // Also test activity parser can extract the GUID
            Assert.IsTrue(QRCodeActivityParser.TryParseActivityId(result.Text, out var parsedId));
            Assert.AreEqual(activityId, parsedId);

            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void GenerateQRCodeBytes_ValidPayload_ReturnsValidPngHeader()
        {
            var payload = "TestPNGPayload";
            var bytes = QRCodeGeneratorService.GenerateQRCodeBytes(payload, pixelsPerModule: 4);

            Assert.IsNotNull(bytes);
            Assert.Greater(bytes.Length, 8);

            // Standard PNG magic bytes: 0x89, 'P', 'N', 'G', 0x0D, 0x0A, 0x1A, 0x0A
            Assert.AreEqual(0x89, bytes[0]);
            Assert.AreEqual((byte)'P', bytes[1]);
            Assert.AreEqual((byte)'N', bytes[2]);
            Assert.AreEqual((byte)'G', bytes[3]);
        }

        [Test]
        public void GenerateQRCodeTexture_NullOrEmptyPayload_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                QRCodeGeneratorService.GenerateQRCodeTexture(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                QRCodeGeneratorService.GenerateQRCodeTexture(string.Empty);
            });
        }
    }
}
