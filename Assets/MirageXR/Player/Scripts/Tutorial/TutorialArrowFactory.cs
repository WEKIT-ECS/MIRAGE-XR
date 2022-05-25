using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;

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

        public GameObject CreateArrow()
        {
            if (PlatformManager.Instance.WorldSpaceUi)
            {
                GameObject newArrow = Object.Instantiate(arrowPrefab3D, Vector3.zero, Quaternion.identity);
                Tutorial3DArrow arrowScript = newArrow.AddComponent<Tutorial3DArrow>();
                return newArrow;
            }

            return null;
        }
        
    }
}
