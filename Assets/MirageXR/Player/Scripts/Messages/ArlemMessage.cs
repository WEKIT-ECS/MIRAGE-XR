using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public static class ArlemMessage
    {
        public static void ReadMessages(LearningExperienceEngine.Action action)
        {
            foreach (var message in action.exit.messages)
            {
                if (!string.IsNullOrEmpty(message.target) && !string.IsNullOrEmpty(message.text))
                {
                    Read(message.target, message.text);
                }
            }
        }

        private static void Read(string target, string message)
        {
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogWarning("Message target not set.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("Message text not set.");
                return;
            }

            if (target == "all" || target == LearningExperienceEngine.WorkplaceManager.GetUser())
            {
                Maggie.Speak(message);
            }
        }
    }
}