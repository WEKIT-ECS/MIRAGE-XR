using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.WebGLSample
{
    public class DebugLogger : MonoBehaviour
    {
        [SerializeField] private Text debugText;

        public void LogMessage(string messages)
        {
            debugText.text = messages;
        }
    }
}