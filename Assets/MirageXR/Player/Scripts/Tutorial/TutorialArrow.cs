using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// An abstract class defining the fields and methods
    /// required for a TutorialArrow.
    /// </summary>
    public abstract class TutorialArrow : MonoBehaviour
    {
        [SerializeField] protected Text instructionText;

        public Text GetInstructionText()
        {
            return this.instructionText;
        }

        public abstract void PointTo(GameObject target, string instructionText);

        public abstract void Dissapear();
    }
}
