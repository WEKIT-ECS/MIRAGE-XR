using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// A concrete implementation of a TutorialArrow. Its intended
    /// use is in 3D environments such as the Hololens.
    /// </summary>
    public class Tutorial3DArrow : TutorialArrow
    {
        public static readonly Vector3 DEFAULT_POSITION_OFFSET = Vector3.forward * (-0.02f);
        private GameObject target;

        public Vector3? PositionOffset;
        public Vector3? RotationOffset;

        public override void PointTo(GameObject target, string instructionText,
            Vector3? positionOffset, Vector3? rotationOffset)
        {
            PositionOffset = positionOffset;
            RotationOffset = rotationOffset;

            this.target = target;
            if (PositionOffset != null)
            {
                gameObject.transform.position = (Vector3)(target.transform.position + PositionOffset);
            }
            else
            {
                gameObject.transform.position = target.transform.position + DEFAULT_POSITION_OFFSET;
            }

            if (RotationOffset != null)
            {
                gameObject.transform.rotation = target.transform.rotation * Quaternion.Euler((Vector3)RotationOffset);
            }
            else
            {
                gameObject.transform.rotation = target.transform.rotation;
            }

            gameObject.GetComponentInChildren<Text>().text = instructionText;

            gameObject.SetActive(true);
        }

        public override void Dissapear()
        {
            gameObject.SetActive(false);
        }


        private void Update()
        {
            if (gameObject.activeInHierarchy && target != null)
            {
                if (PositionOffset != null)
                {
                    gameObject.transform.position = (Vector3)(target.transform.position + PositionOffset);
                }
                else
                {
                    gameObject.transform.position = target.transform.position + DEFAULT_POSITION_OFFSET;
                }

                if (RotationOffset != null)
                {
                    gameObject.transform.rotation = target.transform.rotation * Quaternion.Euler((Vector3)RotationOffset);
                }
                else
                {
                    gameObject.transform.rotation = target.transform.rotation;
                }
            }
        }

    }
}