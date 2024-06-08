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
    public class VirtualInstructorManager
    {
        /// <summary>
        /// Represents a list of all virtual instructors.
        /// </summary>
        private List<VirtualInstructor> _instructors = new();

        /// <summary>
        /// Adds a virtual instructor to the list of instructors in the VirtualInstructorManager.
        /// </summary>
        public void AddInstructor(VirtualInstructor instructor)
        {
            _instructors.Add(instructor);
        }
        /// <summary>
        /// Removes the specified virtual instructor from the instructor list.
        /// </summary>
        public void RemoveInstructor(VirtualInstructor instructor)
        {
            if (_instructors.Contains(instructor)) _instructors.Remove(instructor);
        }

        /// <summary>
        /// Checks if there is at least one virtual instructor in the list of instructors.
        /// </summary>
        /// <returns>Returns true if there is at least one virtual instructor in the list, otherwise returns false.</returns>
        public bool IsVirtualInstructorInList()
        {
            if (_instructors.Count == 0) return false;
            return true;
        }

        /// <summary>
        /// Asynchronously asks the closest virtual instructor a question and returns their response.
        /// </summary>
        /// <param name="question">The audio clip containing the question to ask.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the audio clip response from the virtual instructor.</returns>
        public async Task<AudioClip> AskClosestInstructor(AudioClip question)
        {
            
            if (_instructors == null) throw new NullReferenceException("_Instructors is null");
            if (_instructors.Count == 0)
            {
                UnityEngine.Debug.LogError($"AskClosestInstructor, No Instructor but _Instructors count is {_instructors.Count}.");
                return null;
            }
            if (Camera.main == null) 
            {
                UnityEngine.Debug.LogError("Main Camera is null");
                return null;
            }
            if (_instructors.Count == 1) return await _instructors[0].AskVirtualInstructor(question);
            if (_instructors.Count > 1)
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
                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(instructor.gameObject.transform.position);
                    bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                     viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                     viewportPos.z > 0;
                    if (isVisible)
                    {
                        if (Vector3.Distance(Camera.main.transform.position, instructor.gameObject.transform.position) >
                            distance)
                        {
                            distance = Vector3.Distance(Camera.main.transform.position,
                                instructor.gameObject.transform.position);
                            winner = instructor;
                        }
                    }
                }
                if (!winner.IsNull()) return await winner.AskVirtualInstructor(question);
                UnityEngine.Debug.LogError($"AskClosestInstructor, No Instructor but _Instructors count is {_instructors.Count}.");
                return null;
            }
            UnityEngine.Debug.LogError(
                $"AskClosestInstructor: {_instructors.Count} Instructors in the list but, not found");

            return null;
        }
    }
}