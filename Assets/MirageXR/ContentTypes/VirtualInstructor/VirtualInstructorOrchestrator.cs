using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public class UnityEventVirtualInstructorList : UnityEvent<List<IVirtualInstructor>> { }

    /// <summary>
    /// Manages runtime virtual instructors and routes user input to the most relevant instructor.
    /// </summary>
    public class VirtualInstructorOrchestrator
    {
        public UnityEventVirtualInstructorList OnVirtualInstructorsAdded => _onVirtualInstructorsAdded;
        public UnityEventVirtualInstructorList OnVirtualInstructorsRemoved => _onVirtualInstructorsRemoved;

        public event Action<bool> OnInstructorAvailabilityChanged;

        private readonly UnityEventVirtualInstructorList _onVirtualInstructorsAdded = new();
        private readonly UnityEventVirtualInstructorList _onVirtualInstructorsRemoved = new();
        private readonly List<IVirtualInstructor> _instructors = new();

        private string _messageQueue = string.Empty;

        public void AddInstructor(IVirtualInstructor instructor)
        {
            if (instructor == null || _instructors.Contains(instructor)) return;

            _instructors.Add(instructor);
            _onVirtualInstructorsAdded.Invoke(new List<IVirtualInstructor>(_instructors));
            NotifyAvailability();
        }

        public void RemoveInstructor(IVirtualInstructor instructor)
        {
            if (instructor == null || !_instructors.Contains(instructor)) return;

            _instructors.Remove(instructor);
            _onVirtualInstructorsRemoved.Invoke(new List<IVirtualInstructor>(_instructors));
            NotifyAvailability();
        }

        public bool IsVirtualInstructorInList() => _instructors.Count > 0;

        public void AddToNextMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _messageQueue += $" {message} ";
        }

        public async Task<AudioClip> AskInstructorWithAudioQuestion(AudioClip question)
        {
            var instructor = DetermineVirtualInstructor();
            if (instructor == null) return null;

            var clip = await instructor.AskVirtualInstructorAudio(question, _messageQueue);
            instructor.PlayAudio(clip);
            return clip;
        }

        public async Task<AudioClip> AskInstructorWithStringQuestion(string question)
        {
            var instructor = DetermineVirtualInstructor();
            if (instructor == null) return null;

            var clip = await instructor.AskVirtualInstructorString(question, _messageQueue);
            instructor.PlayAudio(clip);
            return clip;
        }

        public async Task<AudioClip> ConvertTextToSpeechWithInstructor(string message)
        {
            var instructor = DetermineVirtualInstructor();
            if (instructor == null) return null;

            var clip = await instructor.ConvertTextToSpeech(message);
            instructor.PlayAudio(clip);
            return clip;
        }

        private IVirtualInstructor DetermineVirtualInstructor()
        {
            if (_instructors.Count == 0 || Camera.main == null)
            {
                Debug.LogWarning("[Orchestrator] No instructors or camera available.");
                return null;
            }

            if (_instructors.Count == 1)
                return _instructors[0];

            var cam = Camera.main;
            IVirtualInstructor closest = null;
            float minDistance = float.MaxValue;

            foreach (var instructor in _instructors)
            {
                if (instructor == null) continue;

                try
                {
                    var viewPos = cam.WorldToViewportPoint(instructor.Position);
                    bool isVisible = viewPos.x is >= 0 and <= 1 &&
                                     viewPos.y is >= 0 and <= 1 &&
                                     viewPos.z > 0;

                    if (!isVisible) continue;

                    float distance = Vector3.Distance(cam.transform.position, instructor.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = instructor;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Orchestrator] Error while determining instructor visibility: {e.Message}");
                }
            }

            return closest;
        }

        private void NotifyAvailability()
        {
            OnInstructorAvailabilityChanged?.Invoke(_instructors.Count > 0);
        }
    }
}
