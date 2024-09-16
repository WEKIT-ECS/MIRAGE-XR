using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly List<VirtualInstructor> _instructors = new();

        private VirtualInstructor _moderator;
        
        private string _messageQueue; 
        
        
        /// <summary>
        /// Adds a virtual instructor to the list of instructors in the VirtualInstructorManager.
        /// </summary>
        /// <param name="instructor">The VirtualInstructor</param>
        public void AddInstructor(VirtualInstructor instructor)
        {
            _instructors.Add(instructor);
            if(instructor.ModeratorStatus())
            {
                _moderator = instructor;
            }
        }

        /// <summary>
        /// Removes the specified virtual instructor from the instructor list.
        /// </summary>
        /// <param name="instructor">The VirtualInstructor</param>
        public void RemoveInstructor(VirtualInstructor instructor)
        {
            if (_instructors.Contains(instructor)) _instructors.Remove(instructor);
            if (instructor.ModeratorStatus())
            {
                _moderator = null;
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
                var instructor = DetermineVirtualInstructor();
                return await instructor.AskVirtualInstructorAudio(question, _messageQueue);
        }

        
        /// <summary>
        /// Asynchronously sends a string question to a virtual instructor and returns the response as an <see cref="AudioClip"/>.
        /// </summary>
        /// <param name="question">The question to be asked, provided as a string.</param>
        /// <returns>
        /// A <see cref="Task{AudioClip}"/> that contains the responses of the LLM as an <see cref="AudioClip"/>.
        /// </returns>
        public async Task<AudioClip> AskInstructorWithStringQuestion(string question)
        {
                VirtualInstructor instructor = DetermineVirtualInstructor();
                return await instructor.AskVirtualInstructorString(question, _messageQueue);
        }

        /// <summary>
        /// Asynchronously converts a text message to speech using a virtual instructor, and returns the response as an <see cref="AudioClip"/>.
        /// If the instructor fails, it falls back to using the default VI for text-to-speech conversion.
        /// </summary>
        /// <param name="message">The message to be converted to speech.</param>
        /// <returns>
        /// A <see cref="Task{AudioClip}"/> that contains the responses of the LLM as an <see cref="AudioClip"/>.
        /// </returns>
        public async Task<AudioClip> ConvertTextToSpeechWithInstructor(string message)
        {
            try
            {
                VirtualInstructor instructor = DetermineVirtualInstructor();
                return await instructor.ConvertTextToSpeech(message);
            }
            catch
            {
                return await _moderator.ConvertTextToSpeech(message);
            }
        }



        /// <summary>
        /// Adds a message to the next message to be sent by the virtual instructor.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void AddToNextMessage(string message)
        {
            _messageQueue += " " + message + " "; 
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
                        if (!instructor || !instructor.gameObject)
                        {
                            UnityEngine.Debug.LogError("Instructor or Instructor GameObject is null");
                            continue;
                        }

                        try
                        { Vector3 viewportPos =
                                Camera.main.WorldToViewportPoint(instructor.gameObject.transform.position);
                        
                            bool isVisible = viewportPos.x is >= 0 and <= 1 &&
                                             viewportPos.y is >= 0 and <= 1 &&
                                             viewportPos.z > 0;
                            if (isVisible)
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
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError("Main camara is missing! See: " + e);
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