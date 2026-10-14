using NUnit.Framework;
using UnityEngine;
using ZXing;

namespace MirageXR.UnitTests
{
    [TestFixture]
    public class QRCodeScannerTests
    {
        [Test]
        public void CalculateQRPose_WithResultPoints_ReturnsValidPose()
        {
            var points = new ResultPoint[]
            {
                new ResultPoint(100f, 100f),
                new ResultPoint(300f, 100f),
                new ResultPoint(200f, 300f)
            };

            var pose = QRCodeScanner.CalculateQRPose(points, 640, 480);

            Assert.AreNotEqual(Vector3.zero, pose.position);
            Assert.AreNotEqual(Quaternion.identity, pose.rotation);
        }

        [Test]
        public void CalculateQRPose_WithVector2Points_ReturnsValidPose()
        {
            var points = new Vector2[]
            {
                new Vector2(100f, 100f),
                new Vector2(300f, 100f),
                new Vector2(200f, 300f)
            };

            var pose = QRCodeScanner.CalculateQRPose(points, 640, 480);

            Assert.AreNotEqual(Vector3.zero, pose.position);
            Assert.AreNotEqual(Quaternion.identity, pose.rotation);
        }

        [Test]
        public void CalculateQRPose_NullResultPoints_ReturnsFallbackPose()
        {
            var pose = QRCodeScanner.CalculateQRPose((ResultPoint[])null, 640, 480);

            Assert.AreNotEqual(Vector3.zero, pose.position);
        }

        [Test]
        public void CalculateQRPose_ConstrainsToYawRotationOnly()
        {
            var points = new Vector2[]
            {
                new Vector2(320f, 240f)
            };

            var pose = QRCodeScanner.CalculateQRPose(points, 640, 480);
            var euler = pose.rotation.eulerAngles;

            // X (pitch) and Z (roll) should be zero, only Y (yaw) is non-zero
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, euler.x), 0.01f);
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, euler.z), 0.01f);
        }

        [Test]
        public void DecodeTexture_NullTexture_ReturnsNullAndInvokesError()
        {
            var go = new GameObject("TestScanner");
            var scanner = go.AddComponent<QRCodeScanner>();

            string errorMessage = null;
            scanner.OnScanError += err => errorMessage = err;

            var result = scanner.DecodeTexture(null);

            Assert.IsNull(result);
            Assert.AreEqual("Invalid image texture.", errorMessage);
            Assert.IsFalse(scanner.IsScanning);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DecodeTexture_ValidQRCodeTexture_ReturnsScanResult()
        {
            var go = new GameObject("TestScanner");
            var scanner = go.AddComponent<QRCodeScanner>();

            string expectedGuid = "550e8400-e29b-41d4-a716-446655440000";
            string qrText = $"https://repository.wekit-ecs.com/deeplink/?activity={expectedGuid}";

            var writer = new BarcodeWriterGeneric
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var bitMatrix = writer.Encode(qrText);
            var texture = new Texture2D(bitMatrix.Width, bitMatrix.Height);
            for (int x = 0; x < bitMatrix.Width; x++)
            {
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    texture.SetPixel(x, bitMatrix.Height - 1 - y, bitMatrix[x, y] ? Color.black : Color.white);
                }
            }
            texture.Apply();

            QRCodeScanResult scanResult = null;
            scanner.OnQRCodeDetected += res => scanResult = res;

            var result = scanner.DecodeTexture(texture);

            Assert.IsNotNull(result);
            Assert.AreEqual(qrText, result.RawText);
            Assert.AreEqual(System.Guid.Parse(expectedGuid), result.ActivityId);
            Assert.AreSame(result, scanResult);
            Assert.IsFalse(scanner.IsScanning);

            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DecodeTexture_NonActivityQRCode_WithRequireActivityLink_InvokesInvalidQRCodeDetected()
        {
            var go = new GameObject("TestScanner");
            var scanner = go.AddComponent<QRCodeScanner>();
            scanner.RequireActivityLink = true;

            string qrText = "http://en.m.wikipedia.org";

            var writer = new BarcodeWriterGeneric
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var bitMatrix = writer.Encode(qrText);
            var texture = new Texture2D(bitMatrix.Width, bitMatrix.Height);
            for (int x = 0; x < bitMatrix.Width; x++)
            {
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    texture.SetPixel(x, bitMatrix.Height - 1 - y, bitMatrix[x, y] ? Color.black : Color.white);
                }
            }
            texture.Apply();

            QRCodeScanResult scanResult = null;
            string invalidDetected = null;
            scanner.OnQRCodeDetected += res => scanResult = res;
            scanner.OnInvalidQRCodeDetected += raw => invalidDetected = raw;

            var result = scanner.DecodeTexture(texture);

            Assert.IsNotNull(result);
            Assert.AreEqual(qrText, result.RawText);
            Assert.AreEqual(System.Guid.Empty, result.ActivityId);
            Assert.IsNull(scanResult); // OnQRCodeDetected should NOT be invoked for non-activity link
            Assert.AreEqual(qrText, invalidDetected); // OnInvalidQRCodeDetected should be invoked

            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(go);
        }
    }
}
