using i5.Toolkit.Core.VerboseLogging;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public enum DeviceFormat
    {
        Phone,
        Tablet,
        Unknown
    }

    [RequireComponent(typeof(Camera))]
    public class ViewCamera : MonoBehaviour
    {
        private readonly Rect _rectPhone = new Rect(0, 0, 1f, 1f);
        private readonly Rect _rectTablet = new Rect(0.7f, 0, 0.3f, 1f);
        private Camera _camera;
        private DeviceFormat _format = DeviceFormat.Phone;

        public DeviceFormat format => _format;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public async Task SetupFormat(DeviceFormat deviceFormat)
        {
            switch (deviceFormat)
            {
                case DeviceFormat.Phone:
                    await SetupViewForPortrait();
                    break;
                case DeviceFormat.Tablet:
                    await SetupViewForLandscape();
                    break;
                case DeviceFormat.Unknown:
                default:
                    AppLog.LogWarning("Unknown format");
                    break;
            }
        }

        private async Task SetupViewForLandscape()
        {
            if (_format == DeviceFormat.Tablet)
            {
                return;
            }

            var canvas = RootView_v2.Instance.canvas;
            canvas.enabled = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            await WaitForLandscapeOrientation();
            _camera.rect = _rectTablet;
            canvas.enabled = true;
            await Task.Delay(250);
            _format = DeviceFormat.Tablet;
        }

        private async Task SetupViewForPortrait()
        {
            if (_format == DeviceFormat.Phone)
            {
                return;
            }

            var canvas = RootView_v2.Instance.canvas;
            canvas.enabled = false;
            Screen.orientation = ScreenOrientation.Portrait;
            await WaitForPortraitOrientation();
            _camera.rect = _rectPhone;
            canvas.enabled = true;
            await Task.Delay(250);
            _format = DeviceFormat.Phone;
        }

        private static async Task WaitForLandscapeOrientation()
        {
            while (Screen.width < Screen.height)
            {
                await Task.Yield();
            }
        }

        private static async Task WaitForPortraitOrientation()
        {
            while (Screen.height < Screen.width)
            {
                await Task.Yield();
            }
        }
    }
}