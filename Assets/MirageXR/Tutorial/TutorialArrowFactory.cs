using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// A factory that creates TutorialArrows based on the
    /// current application platform.
    /// </summary>
    public class TutorialArrowFactory
    {
        private static TutorialArrowFactory instance;

        private readonly GameObject arrowPrefab3D;

        public enum ArrowType
        {
            DEFAULT
        }

        private TutorialArrowFactory()
        {
            arrowPrefab3D = Resources.Load("prefabs/Tutorial3DArrow", typeof(GameObject)) as GameObject;
        }

        public static TutorialArrowFactory Instance()
        {
            if (instance == null)
            {
                return new TutorialArrowFactory();
            }
            else
            {
                return instance;
            }
        }

        public TutorialArrow CreateArrow(ArrowType arrowType)
        {
            TutorialArrow retVal = null;
            switch (arrowType)
            {
                case ArrowType.DEFAULT:
                    GameObject newArrow = Object.Instantiate(arrowPrefab3D, Vector3.zero, Quaternion.identity);
                    retVal = newArrow.AddComponent<Tutorial3DArrow>();
                    break;
                // Put new arrow types here
            }

            return retVal;
        }

    }
}
