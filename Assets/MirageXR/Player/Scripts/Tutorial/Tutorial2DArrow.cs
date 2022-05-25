using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// This is a concrete implementation of a TutorialArrow.
    /// It's intended use is in 2D application versions.
    /// </summary>
    public class Tutorial2DArrow : TutorialArrow
    {
        private float arrowOffset = -0.01f;
        //
        private GameObject target;

        public override void PointTo(GameObject target, string instructionText)
        {
            this.target = target;
            gameObject.transform.position = target.transform.position;
            gameObject.transform.rotation = target.transform.rotation;

            gameObject.GetComponentInChildren<Text>().text = instructionText;
        }

        public override void Dissapear()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy && target != null)
            {
                transform.position = target.transform.position;
            }
        }

    }
}
