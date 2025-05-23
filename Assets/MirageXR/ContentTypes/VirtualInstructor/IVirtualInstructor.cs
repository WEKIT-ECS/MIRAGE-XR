using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public interface IVirtualInstructor
    {
        Vector3 Position { get; }
        bool ModeratorStatus();
        Task<AudioClip> AskVirtualInstructorAudio(AudioClip question, string messageQueue);
        Task<AudioClip> AskVirtualInstructorString(string question, string messageQueue);
        Task<AudioClip> ConvertTextToSpeech(string message);
        void PlayAudio(AudioClip audioClip);
    }
}