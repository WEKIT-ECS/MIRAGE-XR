using HoloToolkit.Unity;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using UnityEngine;

namespace MirageXR
{
    public class HTKTextToSpeechService : ITextToSpeechService
    {
        private GameObject managerObj;
        private TextToSpeech textToSpeech;

        private TextToSpeechVoice voice;

        public TextToSpeechVoice Voice
        {
            get => voice;
            set
            {
                voice = value;
                if (textToSpeech != null)
                {
                    textToSpeech.Voice = voice;
                }
            }
        }

        public HTKTextToSpeechService(TextToSpeechVoice voice = TextToSpeechVoice.Zira)
        {
            this.voice = voice;
        }

        public void Initialize(IServiceManager owner)
        {
            managerObj = new GameObject();
            managerObj.name = "TextToSpeech Manager";
            textToSpeech = managerObj.AddComponent<TextToSpeech>();
            textToSpeech.Voice = voice;
            PersistenceScene.MarkPersistent(managerObj);
        }

        public void Cleanup()
        {
            GameObject.Destroy(textToSpeech);
            managerObj.name = "GameObject";
            PersistenceScene.UnmarkPersistent(managerObj);
            Object.Destroy(managerObj);
        }

        public bool IsSpeaking()
        {
            return textToSpeech.IsSpeaking();
        }

        public void StartSpeaking(string text)
        {
            textToSpeech.StartSpeaking(text);
        }

        public void StopSpeaking()
        {
            textToSpeech.StopSpeaking();
        }
    }
}