using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MirageXR
{
    public class VirtualInstructorManager : MonoBehaviour
    {
        // todo What have we do here for the Implimtation?
        private List<VirtualInstructor> _Instrutors;

        public void AddInstrutor(VirtualInstructor instructor)
        {
            _Instrutors.Add(instructor);
        }
        
  

        public void AskCloserstInstructor(AudioClip question)
        {
            if (_Instrutors.Count() >= 1)
            {
                //_Instrutors[0].AskVirtualInstructor(question); // Hier musses wir die Position der GameObject in relation zu der kamera herausfinden. 
            }
        }
        // Das hat die AI von der IDE vorgeschlagen... 
        // public VirtualInstructor GetClosestInstructor(Vector3 position)
        // {
        //     if (_Instrutors.Count == 0)
        //     {
        //         return null;
        //     }
        //
        //     VirtualInstructor closestInstructor = _Instrutors[0];
        //     float closestDistance = Vector3.Distance(position, closestInstructor.Instructor.transform.position);
        //
        //     for (int i = 1; i < _Instrutors.Count; i++)
        //     {
        //         float distance = Vector3.Distance(position, _Instrutors[i].Instructor.transform.position);
        //         if (distance < closestDistance)
        //         {
        //             closestInstructor = _Instrutors[i];
        //             closestDistance = distance;
        //         }
        //     }
        //
        //     return closestInstructor;
        // }

        public void Initialize()
        {
            UnityEngine.Debug.Log("VirtualInstructorManager ist da."); // just a test for me can be removed. 
        }
    }
}
