using i5.Toolkit.Core.ServiceCore;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// MirageXR text-to-speech manager.
    /// </summary>
    public class Maggie : MonoBehaviour
    {
        private static MaggieManager maggieManager;

        private void Start()
        {
            ITextToSpeechService ttsService;

            // set up the manager
            if (!ServiceManager.ServiceExists<ITextToSpeechService>())
            {
                ttsService = new HTKTextToSpeechService();
                ServiceManager.RegisterService(ttsService);
            }
            else
            {
                ttsService = ServiceManager.GetService<ITextToSpeechService>();
            }
            maggieManager = new MaggieManager(ttsService);
        }

        /// <summary>
        /// Speak out text.
        /// </summary>
        /// <param name="text">Text to be spoken.</param>
        public static void Speak(string text)
        {
            maggieManager.Speak(text);
        }

        /// <summary>
        /// Speak out activity ready message.
        /// </summary>
        public static void ActivityReady()
        {
            maggieManager.ActivityReady();
        }

        /// <summary>
        /// Speak out ok message.
        /// </summary>
        public static void Ok()
        {
            maggieManager.Ok();
        }

        /// <summary>
        /// Speak out error message.
        /// </summary>
        public static void Error()
        {
            maggieManager.Error();
        }

        public static void Stop()
        {
            maggieManager.Stop();
        }
    }
}