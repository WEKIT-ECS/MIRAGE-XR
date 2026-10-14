using UnityEngine;
using ZXing;

namespace MirageXR
{
    /// <summary>
    /// Fast luminance source that converts Unity Color32 pixel arrays directly
    /// to 8-bit grayscale byte data for ZXing decoding without allocating intermediate buffers.
    /// </summary>
    public class UnityColor32LuminanceSource : BaseLuminanceSource
    {
        public UnityColor32LuminanceSource(Color32[] colors, int width, int height) : base(width, height)
        {
            luminances = new byte[width * height];
            int count = Mathf.Min(colors.Length, luminances.Length);
            for (int i = 0; i < count; i++)
            {
                var c = colors[i];
                // ITU-R BT.601 luma conversion: (R*19595 + G*38469 + B*7472) >> 16
                luminances[i] = (byte)((c.r * 19595 + c.g * 38469 + c.b * 7472) >> 16);
            }
        }

        private UnityColor32LuminanceSource(byte[] luminances, int width, int height) : base(width, height)
        {
            this.luminances = luminances;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new UnityColor32LuminanceSource(newLuminances, width, height);
        }
    }
}
