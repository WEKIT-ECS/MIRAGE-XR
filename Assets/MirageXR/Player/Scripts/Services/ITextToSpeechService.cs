using i5.Toolkit.Core.ServiceCore;

namespace MirageXR
{
    public interface ITextToSpeechService : IService
    {
        bool IsSpeaking();

        void StartSpeaking(string text);

        void StopSpeaking();
    }
}
