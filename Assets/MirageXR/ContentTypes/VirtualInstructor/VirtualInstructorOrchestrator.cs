using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Manages virtual instructors.
    /// </summary>
    public class VirtualInstructorOrchestrator
    {
        /// <summary>
        /// Represents a list of all virtual instructors.
        /// </summary>
        private List<VirtualInstructor> _instructors = new();

        private VirtualInstructor _modertator;

        /// <summary>
        /// Adds a virtual instructor to the list of instructors in the VirtualInstructorManager.
        /// </summary>
        public void AddInstructor(VirtualInstructor instructor)
        {
            _instructors.Add(instructor);
            if(instructor.moderatorStatus())
            {
                _modertator = instructor;
            }
        }

        /// <summary>
        /// Removes the specified virtual instructor from the instructor list.
        /// </summary>
        public void RemoveInstructor(VirtualInstructor instructor)
        {
            if (_instructors.Contains(instructor)) _instructors.Remove(instructor);
            if (instructor.moderatorStatus())
            {
                _modertator = null;
            }
        }

        /// <summary>
        /// Checks if there is at least one virtual instructor in the list of instructors.
        /// </summary>
        /// <returns>Returns true if there is at least one virtual instructor in the list, otherwise returns false.</returns>
        public bool IsVirtualInstructorInList()
        {
            return _instructors.Count != 0;
        }

        /// <summary>
        /// Asynchronously asks the closest virtual instructor a question and returns their response.
        /// </summary>
        /// <param name="question">The audio clip containing the question to ask.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the audio clip response from the virtual instructor.</returns>
        public async Task<AudioClip> AskInstructorWithAudioQuestion(AudioClip question)
        {
            try
            {
                var instructor = DetermineVirtualInstructor();
                return await instructor.AskVirtualInstructorAudio(question);
            }
            catch
            {
                return await _modertator.AskVirtualInstructorAudio(question);
            }
            
        }

        public async Task<AudioClip> AskInstructorWithStringQuestion(string question)
        {
            try
            {
                VirtualInstructor instructor = DetermineVirtualInstructor();
                return await instructor.AskVirtualInstructorString(question);
            }
            catch
            {
                return await _modertator.AskVirtualInstructorString(question);
            }
            
        }

        public async Task<AudioClip> ConvertTextToSpeechWithInstructor(string message)
        {
            try
            {
                VirtualInstructor instructor = DetermineVirtualInstructor();
                return await instructor.ConvertTextToSpeech(message);
            }
            catch
            {
                return await _modertator.ConvertTextToSpeech(message);
            }
        }



        public void AddToQueueOfClosestInstructor(string message)
        {
            try
            {
                VirtualInstructor instructor = DetermineVirtualInstructor();
                instructor.AddToNextMessage(message);

            }
            catch (Exception e)
            {
                _modertator.AddToNextMessage(message);
            }

        }

        private VirtualInstructor DetermineVirtualInstructor()
        {
            if (!CheckInstructors() || !CheckCamera())
            {
                return null;
            }

            switch (_instructors.Count)
            {
                case 1:
                    return _instructors[0];
                case > 1:
                {
                    VirtualInstructor winner = null;
                    float distance = float.MaxValue;
                    foreach (var instructor in _instructors)
                    {
                        if (instructor == null || instructor.gameObject == null)
                        {
                            UnityEngine.Debug.LogError("Instructor or Instructor GameObject is null");
                            continue;
                        }

                        Vector3 viewportPos =
                            Camera.main.WorldToViewportPoint(instructor.gameObject.transform.position);
                        bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                         viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                         viewportPos.z > 0;
                        if (isVisible) // what if we want to send a message to a VI, than it is not necessary true! Edge caes!
                        {
                            if (Vector3.Distance(Camera.main.transform.position,
                                    instructor.gameObject.transform.position) >
                                distance)
                            {
                                distance = Vector3.Distance(Camera.main.transform.position,
                                    instructor.gameObject.transform.position);
                                winner = instructor;
                            }
                        }
                    }

                    return winner;
                }
                default:
                    return null;
            }
        }

        private bool CheckInstructors()
        {
            if (_instructors == null) throw new NullReferenceException("_Instructors is null");
            if (_instructors.Count != 0) return true;
            UnityEngine.Debug.LogError(
                $"AskClosestInstructor, No Instructor but _Instructors count is {_instructors.Count}.");
            return false;

        }

        private bool CheckCamera()
        {
            if (Camera.main != null) return true;
            UnityEngine.Debug.LogError("Main Camera is null");
            return false;
        }
    }
}