using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Defines the contract for all virtual instructor components in MirageXR.
    /// 
    /// This interface is used by the VirtualInstructorOrchestrator to interact with instructors in a decoupled way.
    /// It ensures consistency for AI-based input/output handling, regardless of the underlying implementation.
    /// 
    /// Members:
    /// - Position: world position of the instructor (used for camera visibility/distance logic)
    /// - AskVirtualInstructorAudio(): handles audio-based questions and returns AI-generated speech
    /// - AskVirtualInstructorString(): handles text-based questions and returns AI-generated speech
    /// - ConvertTextToSpeech(): converts text directly to speech via TTS
    /// </summary>
    public interface IVirtualInstructor
    {
        Vector3 Position { get; }
        Task<AudioClip> AskVirtualInstructorAudio(AudioClip question, string messageQueue);
        Task<AudioClip> AskVirtualInstructorString(string question, string messageQueue);
        Task<AudioClip> ConvertTextToSpeech(string message);
        void PlayAudio(AudioClip audioClip);
  
        public event Action<AudioClip> OnInstructorResponseAvailable;
    }
}