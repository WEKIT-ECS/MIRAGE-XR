using System;
using QRCoder;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Service for generating QR code textures and raw PNG bytes using QRCoder.
    /// Supports arbitrary text payloads as well as standard MirageXR activity redirect deep links.
    /// </summary>
    public static class QRCodeGeneratorService
    {
        /// <summary>
        /// Generates a Texture2D containing a QR code for the specified string payload.
        /// </summary>
        /// <param name="payload">The text content to encode in the QR code.</param>
        /// <param name="pixelsPerModule">Resolution multiplier (pixels per QR module block).</param>
        /// <param name="drawQuietZones">Whether to include the standard quiet zone margin around the code.</param>
        /// <param name="darkColor">Optional dark color (defaults to opaque black).</param>
        /// <param name="lightColor">Optional light color (defaults to opaque white).</param>
        /// <param name="eccLevel">Error correction level (default Q = 25% recovery capability).</param>
        /// <returns>A Texture2D representing the QR code, with Point filtering for sharpness.</returns>
        public static Texture2D GenerateQRCodeTexture(
            string payload,
            int pixelsPerModule = 10,
            bool drawQuietZones = true,
            Color32? darkColor = null,
            Color32? lightColor = null,
            QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.Q)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException(nameof(payload), "Payload cannot be null or empty.");
            }

            if (pixelsPerModule <= 0)
            {
                pixelsPerModule = 10;
            }

            Color32 dark = darkColor ?? new Color32(0, 0, 0, 255);
            Color32 light = lightColor ?? new Color32(255, 255, 255, 255);

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(payload, eccLevel))
            {
                var moduleMatrix = qrCodeData.ModuleMatrix;
                int matrixSize = moduleMatrix.Count;

                // If not drawing quiet zones, strip standard 4-module border if present
                int offset = 0;
                int effectiveSize = matrixSize;

                if (!drawQuietZones && matrixSize > 8)
                {
                    offset = 4;
                    effectiveSize = matrixSize - 8;
                }

                int textureSize = effectiveSize * pixelsPerModule;
                var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                Color32[] pixels = new Color32[textureSize * textureSize];

                for (int y = 0; y < effectiveSize; y++)
                {
                    int matrixY = y + offset;
                    // Invert Y for Unity UI coordinate space (0,0 is bottom-left)
                    int invertedY = effectiveSize - 1 - y;

                    for (int x = 0; x < effectiveSize; x++)
                    {
                        int matrixX = x + offset;
                        bool isDark = moduleMatrix[matrixY][matrixX];
                        Color32 moduleColor = isDark ? dark : light;

                        int startX = x * pixelsPerModule;
                        int startY = invertedY * pixelsPerModule;

                        for (int py = 0; py < pixelsPerModule; py++)
                        {
                            int rowStart = (startY + py) * textureSize + startX;
                            for (int px = 0; px < pixelsPerModule; px++)
                            {
                                pixels[rowStart + px] = moduleColor;
                            }
                        }
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();
                return texture;
            }
        }

        /// <summary>
        /// Generates a QR code Texture2D for a given activity GUID using the standard repository deep-link URL.
        /// </summary>
        public static Texture2D GenerateActivityQRCodeTexture(
            Guid activityId,
            int pixelsPerModule = 10,
            bool drawQuietZones = true)
        {
            if (activityId == Guid.Empty)
            {
                throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
            }

            var url = QRCodeActivityParser.CreateRedirectUrl(activityId);
            return GenerateQRCodeTexture(url, pixelsPerModule, drawQuietZones);
        }

        /// <summary>
        /// Generates PNG raw bytes of a QR code for saving to disk or sharing.
        /// </summary>
        public static byte[] GenerateQRCodeBytes(
            string payload,
            int pixelsPerModule = 10,
            bool drawQuietZones = true,
            QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.Q)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(payload, eccLevel))
            {
                var pngQrCode = new PngByteQRCode(qrCodeData);
                return pngQrCode.GetGraphic(pixelsPerModule, drawQuietZones);
            }
        }
    }
}
