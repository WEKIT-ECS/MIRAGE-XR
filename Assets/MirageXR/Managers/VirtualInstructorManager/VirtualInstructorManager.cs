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
    public class VirtualInstructorManager : MonoBehaviour
    {
        /// <summary>
        /// Represents a list of all virtual instructors.
        /// </summary>
        private List<VirtualInstructor> _Instrutors = new();

        /// <summary>
        /// Adds a virtual instructor to the list of instructors in the VirtualInstructorManager.
        /// </summary>
        public void AddInstrutor(VirtualInstructor instructor)
        {
            _Instrutors.Add(instructor);
        }
        /// <summary>
        /// Removes the specified virtual instructor from the instructor list.
        /// </summary>
        public void RemoveInstrutor(VirtualInstructor instructor)
        {
            _Instrutors.Remove(instructor);
        }

        /// <summary>
        /// Checks if there is at least one virtual instructor in the list of instructors.
        /// </summary>
        /// <returns>Returns true if there is at least one virtual instructor in the list, otherwise returns false.</returns>
        public bool IsVirtualInstructorInList()
        {
            if (_Instrutors.Count == 0) return false;
            return true;
        }

        /// <summary>
        /// Asynchronously asks the closest virtual instructor a question and returns their response.
        /// </summary>
        /// <param name="question">The audio clip containing the question to ask.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the audio clip response from the virtual instructor.</returns>
        public async Task<AudioClip> AskCloserstInstructor(AudioClip question)
        {
            // todo testen wenn die UI so weit ist un ich auch VI in den step packen kann. 
            if (_Instrutors == null) throw new NullReferenceException("_Instrutors is null");
            if (_Instrutors.Count == 0)
            {
                UnityEngine.Debug.LogError($"AskCloserstInstructor, No Instructor but _Instrutors count is {_Instrutors.Count}.");
                return null;
            }
            if (_Instrutors.Count == 1) return await _Instrutors[0].AskVirtualInstructor(question);
            if (_Instrutors.Count > 1)
            {
                VirtualInstructor winner = null;
                float distance = float.MaxValue; 
                foreach (var instrutor in _Instrutors)
                {
                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(instrutor.gameObject.transform.position);
                    bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                     viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                     viewportPos.z > 0;
                    if (isVisible)
                    {
                        if (Vector3.Distance(Camera.main.transform.position, instrutor.gameObject.transform.position) >
                            distance)
                        {
                            winner = instrutor;
                        }
                    }
                }
                if (!winner.IsNull()) return await winner.AskVirtualInstructor(question);
                UnityEngine.Debug.LogError($"AskCloserstInstructor, No Instructor but _Instrutors count is {_Instrutors.Count}.");
                return null;
            }
            UnityEngine.Debug.LogError(
                $"AskCloserstInstructor: {_Instrutors.Count} Instructors in the list but, not found");

            return null;
        }
    }
}