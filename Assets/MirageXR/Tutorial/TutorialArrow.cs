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

        /// <summary>
        /// Primary method of the TutorialArrow class, used to make the arrow point to
        /// the desired object. The arrow's position and rotation are set to the target's
        /// by default. The offsets should be used to position the arrow based on the target.
        /// </summary>
        /// <param name="target">The GameObject that should be pointed to.</param>
        /// <param name="instructionText">The instruction text that should be shown.</param>
        /// <param name="positionOffset">Offset from the target's position.</param>
        /// <param name="rotationOffset">Offset to the target's rotation.</param>
        public abstract void PointTo(GameObject target, string instructionText,
            Vector3? positionOffset, Vector3? rotationOffset);

        public abstract void Dissapear();
    }
}
